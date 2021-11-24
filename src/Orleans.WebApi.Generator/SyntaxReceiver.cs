using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

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
                .Where(a => a.Name is
                    IdentifierNameSyntax { Identifier: { ValueText: "MapToWebApi" } }
                    or
                    QualifiedNameSyntax
                    {
                        Left: IdentifierNameSyntax { Identifier: { ValueText: "Orleans.WebApi.Abstractions" } },
                        Right: IdentifierNameSyntax { Identifier: { ValueText: "MapToWebApi" } }
                    }
                ).Any();

            if (hasAttributeSyntax)
            {
                CandidateTypes.Add(classDeclarationSyntax);
            }
        }
    }
}
