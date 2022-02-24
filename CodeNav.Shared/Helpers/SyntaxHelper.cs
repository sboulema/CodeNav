#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using System;

namespace CodeNav.Shared.Helpers
{
    public static class SyntaxHelper
    {
        public static SemanticModel? GetCSharpSemanticModel(SyntaxTree tree)
        {
            SemanticModel semanticModel;

            try
            {
                var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                var compilation = CSharpCompilation.Create("CodeNavCompilation", new[] { tree }, new[] { mscorlib });
                semanticModel = compilation.GetSemanticModel(tree);
            }
            catch (ArgumentException)
            {
                return null;
            }

            return semanticModel;
        }

        public static SemanticModel? GetVBSemanticModel(SyntaxTree tree)
        {
            SemanticModel semanticModel;

            try
            {
                var mscorlibVB = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                var compilationVB = VisualBasicCompilation.Create("CodeNavCompilation", new[] { tree }, new[] { mscorlibVB });
                semanticModel = compilationVB.GetSemanticModel(tree);
            }
            catch (ArgumentException) // SyntaxTree not found to remove 
            {
                return null;
            }

            return semanticModel;
        }
    }
}
