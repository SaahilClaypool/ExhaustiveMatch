using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace ExhaustiveMatch
{
    [Generator]
    public class AttributeGenerator : ISourceGenerator
    {
        public const string Name = "GenerateMatch";
        public const string Namespace = "ExhaustiveMatch";
        private static bool HasGenerated = false;

        private static readonly string _attributeText = $@"using System;
        namespace {Namespace}
        {{
            [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
            public sealed class {Name} : Attribute
            {{
            }}
        }}
                ";

        public static void Generate(GeneratorExecutionContext context)
        {
            HasGenerated = true;
            context.AddSource("GenerateMatch_attribute.cs", SourceText.From(_attributeText, Encoding.UTF8));
        }

        public static (Compilation, INamedTypeSymbol) GetCompilationAndSymbol(GeneratorExecutionContext context)
        {
            if (!HasGenerated)
                Generate(context);

            if ((context.Compilation as CSharpCompilation)?.SyntaxTrees[0].Options is not CSharpParseOptions options)
            {
                throw new System.Exception("");
            }

            Compilation compilation =
                context.Compilation.AddSyntaxTrees(
                    CSharpSyntaxTree.ParseText(SourceText.From(_attributeText, Encoding.UTF8), options));

            INamedTypeSymbol? attributeSymbol =
                compilation.GetTypeByMetadataName($"{Namespace}.{Name}");

            return (compilation, attributeSymbol!);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiverTypeClass());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            Generate(context);
        }
    }
}