#nullable enable

using Microsoft.CodeAnalysis;
using System;

namespace CodeNav.Helpers
{
    public static class SymbolHelper
    {
        public static T? GetSymbol<T>(SemanticModel semanticModel, SyntaxNode member) 
        {
            try
            {
                return (T?)semanticModel.GetDeclaredSymbol(member);
            }
            catch (ArgumentException e)
            {
                if (!e.Message.Contains("DeclarationSyntax") &&
                    !e.Message.Contains("SyntaxTree"))
                {
                    LogHelper.Log("Error during mapping", e);
                }
                return default;
            }
        }
    }
}
