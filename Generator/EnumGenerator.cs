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
    public class EnumGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {

            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            AttributeGenerator.Generate(context);

            var (compilation, attributeSymbol) = AttributeGenerator.GetCompilationAndSymbol(context);

            if (attributeSymbol is null)
            {
                return;
            }

            List<(INamedTypeSymbol, Location?, EnumDeclarationSyntax syntax)> namedTypeSymbols = new();
            foreach (EnumDeclarationSyntax enumDeclaration in receiver.CandidateClasses)
            {
                SemanticModel model = compilation.GetSemanticModel(enumDeclaration.SyntaxTree);
                INamedTypeSymbol? namedTypeSymbol = model.GetDeclaredSymbol(enumDeclaration);

                AttributeData? attributeData = namedTypeSymbol?.GetAttributes().FirstOrDefault(ad =>
                    ad.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) != false);

                if (attributeData is not null)
                {
                    namedTypeSymbols.Add((namedTypeSymbol!,
                        attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation(), enumDeclaration));
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

        private string? ProcessClass(INamedTypeSymbol namedSymbol, GeneratorExecutionContext context, Location? attributeLocation, EnumDeclarationSyntax enumDeclaration)
        {
            var enumMembers = enumDeclaration.Members.Select(
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
    public static class {namedSymbol.Name}MatchExtensions
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
                // Debugger.Launch();
            }
#endif 
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<EnumDeclarationSyntax> CandidateClasses { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is EnumDeclarationSyntax { AttributeLists: { Count: > 0 } } classDeclarationSyntax)
                {
                    CandidateClasses.Add(classDeclarationSyntax);
                }
            }
        }
    }
}