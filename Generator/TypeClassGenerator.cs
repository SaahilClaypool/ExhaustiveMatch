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

namespace ExhaustiveMatch
{
    [Generator]
    public class ClassEnumGenerator : ISourceGenerator
    {
        private const string AttributeName = "GenerateTypeClass";
        private const string AttributeNamespace = "ExhaustiveMatch";

        private readonly string _attributeText = $@"using System;

namespace {AttributeNamespace}
{{
    [AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public sealed class {AttributeName} : Attribute
    {{
    }}
}}
        ";
        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource(AttributeName, SourceText.From(_attributeText, Encoding.UTF8));

            if (context.SyntaxReceiver is not SyntaxReceiverTypeClass receiver)
            {
                return;
            }

            if ((context.Compilation as CSharpCompilation)?.SyntaxTrees[0].Options is not CSharpParseOptions options)
            {
                return;
            }

            Compilation compilation =
                context.Compilation.AddSyntaxTrees(
                    CSharpSyntaxTree.ParseText(SourceText.From(_attributeText, Encoding.UTF8), options));

            INamedTypeSymbol? attributeSymbol =
                compilation.GetTypeByMetadataName($"{AttributeNamespace}.{AttributeName}");

            if (attributeSymbol is null)
            {
                return;
            }

            List<(INamedTypeSymbol, Location?, ClassDeclarationSyntax syntax)> namedTypeSymbols = new();
            foreach (ClassDeclarationSyntax classDeclaration in receiver.CandidateClasses)
            {
                SemanticModel model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                INamedTypeSymbol? namedTypeSymbol = model.GetDeclaredSymbol(classDeclaration);

                AttributeData? attributeData = namedTypeSymbol?.GetAttributes().FirstOrDefault(ad =>
                    ad.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) != false);

                if (attributeData is not null)
                {
                    namedTypeSymbols.Add((namedTypeSymbol!,
                        attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation(), classDeclaration));
                }
            }

            foreach (var (namedSymbol, attributeLocation, enumDeclaration) in namedTypeSymbols)
            {
                string? classSource = ProcessClass(namedSymbol, context, attributeLocation, enumDeclaration);

                if (classSource is null)
                {
                    continue;
                }

                context.AddSource($"{namedSymbol.ContainingNamespace}_{namedSymbol.Name}.generated.cs",
                    SourceText.From(classSource, Encoding.UTF8));
            }

        }

        private string? ProcessClass(INamedTypeSymbol namedSymbol, GeneratorExecutionContext context, Location? attributeLocation, ClassDeclarationSyntax markerClass)
        {
            var enumMembers = markerClass.Members
                .Select(
                e => e.ToString()
            ).ToList();

            var actions = string.Join(", ", enumMembers.Select(member =>
                $"System.Func<T> when{member!}"
            ));

            var vars = string.Join(", ", enumMembers.Select(member =>
                $"T when{member!}"
            ));

            var caseStatements = string.Join("\n", enumMembers.Select(member =>

                @$"case {namedSymbol.Name}.{member!}:
                    return when{member!}.Invoke();"
!));

            var caseStatementVars = string.Join("\n", enumMembers.Select(member =>

                @$"case {namedSymbol.Name}.{member!}:
                    return when{member!};"
!));

            var classDecleration = @$"
namespace {namedSymbol.ContainingNamespace.Name}
{{
    public static class {namedSymbol.Name}Extensions
    {{
        public static T Match<T>(this {namedSymbol.Name} t, {actions})
        {{
            switch (t)
            {{
                {caseStatements}
            }}
            throw new System.Exception(""Unreachable"");
        }}

        public static T Match<T>(this {namedSymbol.Name} t, {vars})
        {{
            switch (t)
            {{
                {caseStatementVars}
            }}
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
                Debugger.Launch();
            }
#endif 
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiverTypeClass());
        }
    }

    internal class SyntaxReceiverTypeClass : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax { AttributeLists: { Count: > 0 } } classDeclarationSyntax)
            {
                CandidateClasses.Add(classDeclarationSyntax);
            }
        }
    }
}