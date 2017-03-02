using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.Mappers
{
    public static class TypeMapper
    {
        public static string Map(ITypeSymbol type, bool useLongNames = false)
        {
            return Map(type.ToString(), useLongNames);
        }

        public static string Map(TypeSyntax type, bool useLongNames = false)
        {
            return Map(type.ToString(), useLongNames);
        }

        public static string Map(string type, bool useLongNames = false)
        {
            if (useLongNames) return type;

            var match = new Regex("(.*)<(.*)>").Match(type);
            if (match.Success)
            {
                return $"{match.Groups[1].Value.Split('.').Last()}<{match.Groups[2].Value.Split('.').Last()}>";
            }
            return type.Contains(".") ? type.Split('.').Last() : type;
        }
    }
}
