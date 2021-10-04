using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace ExhaustiveMatch
{
    public class AttributeGenerator
    {
        public const string Name = "GenerateMatch";
        public const string Namespace = "ExhaustiveMatch";
        private static bool HasGenerated = false;

        private static readonly string _attributeText = $@"using System;
        namespace {Namespace}
        {{
            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
            public sealed class {Name} : Attribute
            {{
            }}
        }}
                ";
        
        public static void Generate(GeneratorExecutionContext context)
        {
            lock(Name)
            {
                if (HasGenerated)
                    return;
                HasGenerated = true;
            }
            context.AddSource("GenerateMatch_attribute.cs", SourceText.From(_attributeText, Encoding.UTF8));
        }

        public static (Compilation, INamedTypeSymbol) GetCompilationAndSymbol(GeneratorExecutionContext context)
        {
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
    }
}