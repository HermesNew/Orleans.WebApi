using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Orleans.WebApi.Generator;
internal class SyntaxReceiver : ISyntaxReceiver
{
    public List<TypeDeclarationSyntax> CandidateTypes { get; } = new();

    /// <summary>
    /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
    /// </summary>
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is TypeDeclarationSyntax { AttributeLists: { Count: >= 1 } classAttributes } classDeclarationSyntax)
        {
            var hasAttributeSyntax = classAttributes
                .SelectMany(a => a.Attributes)
                .Any(a => a.Name is
                    IdentifierNameSyntax { Identifier.ValueText: "MapToWebApi" }
                    or
                    QualifiedNameSyntax
                    {
                        Left: IdentifierNameSyntax { Identifier.ValueText: "Orleans.WebApi.Abstractions" },
                        Right: IdentifierNameSyntax { Identifier.ValueText: "MapToWebApi" }
                    }
                 );

            if (hasAttributeSyntax)
            {
                CandidateTypes.Add(classDeclarationSyntax);
            }
        }
    }
}
