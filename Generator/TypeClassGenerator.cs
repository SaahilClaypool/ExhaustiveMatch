using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ExhaustiveMatch;

[Generator]
public class ClassEnumGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiverTypeClass receiver)
        {
            return;
        }

        var (compilation, attributeSymbol) = AttributeGenerator.GetCompilationAndSymbol(context);

        if (attributeSymbol is null)
        {
            return;
        }

        List<(INamedTypeSymbol, Location?, TypeDeclarationSyntax syntax, SemanticModel)> namedTypeSymbols = new();
        foreach (var typeDeclaration in receiver.CandidateClasses)
        {
            SemanticModel model = compilation.GetSemanticModel(typeDeclaration.SyntaxTree);
            INamedTypeSymbol? namedTypeSymbol = model.GetDeclaredSymbol(typeDeclaration);

            AttributeData? attributeData = namedTypeSymbol?.GetAttributes().FirstOrDefault(ad =>
                ad.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) != false);

            if (attributeData is not null)
            {
                namedTypeSymbols.Add((namedTypeSymbol!,
                    attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation(), typeDeclaration, model));
            }
        }

        foreach (var (namedSymbol, attributeLocation, typeDeclaration, model) in namedTypeSymbols)
        {
            string? classSource = ProcessClass(namedSymbol, context, attributeLocation, typeDeclaration, model);

            if (classSource is null)
            {
                continue;
            }

            context.AddSource($"{namedSymbol.ContainingNamespace}_{namedSymbol.Name}.generated.cs",
                SourceText.From(classSource, Encoding.UTF8));
        }

    }

    private string? ProcessClass(INamedTypeSymbol namedSymbol, GeneratorExecutionContext context, Location? attributeLocation, TypeDeclarationSyntax markerClass, SemanticModel model)
    {
        var enumMembers = markerClass
            .Members
            .Where(m => m is TypeDeclarationSyntax c)
            .Select(m => model.GetDeclaredSymbol(m) as ITypeSymbol)
            .Where(m => SymbolEqualityComparer.Default.Equals(m.BaseType, namedSymbol))
            .ToList();

        var genericExpression = markerClass
            .TypeParameterList
            ?.GetText()
            ?.ToString()
            ?.Trim();

        var typeGenerics = genericExpression
            ?.Trim()
            ?.Trim('<', '>')
            ?.Split(',')
            ?? new string[0];
            
        var methodType = "TReturnType";
        var methodGeneric = string.Join(", ", new string[] { methodType }.Concat(typeGenerics));

        var actions = string.Join(", ", enumMembers.Select(member =>
            $"System.Func<{member},{methodType}> when{member!.Name!}"
        ));

        var vars = string.Join(", ", enumMembers.Select(member =>
            $"{methodType} when{member!.Name}"
        ));

        var caseStatements = string.Join("\n", enumMembers.Select((member, idx) =>
            @$"if (t is {member!} t{idx})
                    return when{member!.Name}.Invoke(t{idx});"
        !));

        var caseStatementVars = string.Join("\n", enumMembers.Select((member, idx) =>
            @$"if (t is {member!} t{idx})
                    return when{member!.Name};"
        !));

        var classDecleration = @$"
namespace {namedSymbol.ContainingNamespace.Name}
{{
    public static class {namedSymbol.Name}MatchExtensions
    {{
        public static {methodType} Match<{methodGeneric}>(this {namedSymbol.Name}{genericExpression} t, {actions})
        {{
            {caseStatements}
            throw new System.Exception(""Unreachable"");
        }}

        public static {methodType} Match<{methodGeneric}>(this {namedSymbol.Name}{genericExpression} t, {vars})
        {{
            {caseStatementVars}
            throw new System.Exception(""Unreachable"");
        }}
    }}
}}
";

        return classDecleration;
    }

    public void Initialize(GeneratorInitializationContext context)
    {

#if DEBUG
        if (!Debugger.IsAttached)
        {
            // Debugger.Launch();
        }
#endif
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiverTypeClass());
    }
}

internal class SyntaxReceiverTypeClass : ISyntaxReceiver
{
    public List<TypeDeclarationSyntax> CandidateClasses { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is TypeDeclarationSyntax { AttributeLists: { Count: > 0 } } typeDeclaration && typeDeclaration is ClassDeclarationSyntax or RecordDeclarationSyntax)
        {
            CandidateClasses.Add(typeDeclaration);
        }
    }
}