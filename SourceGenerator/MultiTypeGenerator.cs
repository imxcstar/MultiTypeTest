using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Generator]
public class MultiTypeSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
            return;

        foreach (var method in receiver.CandidateMethods)
        {
            var model = context.Compilation.GetSemanticModel(method.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(method) as IMethodSymbol;

            if (symbol == null) continue;

            var containingClass = symbol.ContainingType;
            var namespaceName = containingClass.ContainingNamespace.ToDisplayString();

            var methodsToGenerate = new List<string>();

            foreach (var parameter in symbol.Parameters)
            {
                var multiTypeAttribute = parameter.GetAttributes().FirstOrDefault(attr =>
                    attr.AttributeClass != null &&
                    attr.AttributeClass.Name == "MultiTypeAttribute");

                if (multiTypeAttribute != null)
                {
                    var types = multiTypeAttribute.ConstructorArguments[0].Values.Select(v => v.Value as INamedTypeSymbol).ToArray();
                    
                    foreach (var type in types)
                    {
                        methodsToGenerate.Add(GenerateMethod(symbol.Name, type!.ToDisplayString(), parameter.Name));
                    }
                }
            }

            var source = GenerateClass(namespaceName, containingClass.Name, methodsToGenerate);
            context.AddSource($"{containingClass.Name}_generated.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private string GenerateMethod(string methodName, string type, string paramName)
    {
        return $@"
        public void {methodName}({type} {paramName})
        {{
            {methodName}<{type}>({paramName});
        }}
        ";
    }

    private string GenerateClass(string namespaceName, string className, List<string> methods)
    {
        var methodsSource = string.Join("\n", methods);
        return $@"
        namespace {namespaceName}
        {{
            public partial class {className}
            {{
                {methodsSource}
            }}
        }}
        ";
    }

    class SyntaxReceiver : ISyntaxReceiver
    {
        public List<MethodDeclarationSyntax> CandidateMethods { get; } = new List<MethodDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax)
            {
                if (methodDeclarationSyntax.ParameterList.Parameters
                    .Any(param => param.AttributeLists
                        .SelectMany(al => al.Attributes)
                        .Any(attr => attr.Name.ToString() == "MultiType")))
                {
                    CandidateMethods.Add(methodDeclarationSyntax);
                }
            }
        }
    }
}
