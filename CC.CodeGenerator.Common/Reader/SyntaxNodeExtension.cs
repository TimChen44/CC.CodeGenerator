using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.CSharp
{
    public static class SyntaxNodeExtension
    {
        public static SyntaxNode? GetFirstSyntaxNode(this SyntaxNode syntaxNode, Func<SyntaxNode, bool> predicate)
        {
            foreach (var child in syntaxNode.ChildNodes())
            {
                if (predicate(child))
                {
                    return child;
                }

                GetFirstSyntaxNode(child, predicate);
            }
            return null;
        }

        public static string GetNamespace(this SyntaxNode syntaxNode)
        {
            if (syntaxNode.IsKind(SyntaxKind.NamespaceDeclaration))
            {
                return syntaxNode.ChildNodes().First().ToFullString().Trim();
            }
            else if (syntaxNode.Parent != null)
            {
                return GetNamespace(syntaxNode.Parent);
            }
            else
            {
                return null;
            }
        }
    }
}
