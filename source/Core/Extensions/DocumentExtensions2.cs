using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace N
{
    public static class C
    {
        public static void M(this SyntaxNode root, SyntaxTrivia oldTrivia, SyntaxTrivia newTrivia)
        {
            SyntaxNode newRoot = root.ReplaceTrivia(oldTrivia, newTrivia);
        }
    }
}
