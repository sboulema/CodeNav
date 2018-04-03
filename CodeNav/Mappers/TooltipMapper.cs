using System.Linq;
using CodeNav.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.Mappers
{
    public static class TooltipMapper
    {
        public static string Map(CodeItemAccessEnum access, string type, string name, ParameterListSyntax parameters)
        {
            return Map(access, type, name, ParameterMapper.MapParameters(parameters, true));
        }

        public static string Map(CodeItemAccessEnum access, string type, string name, VisualBasicSyntax.ParameterListSyntax parameters)
        {
            return Map(access, type, name, ParameterMapper.MapParameters(parameters, true));
        }

        public static string Map(CodeItemAccessEnum access, string type, string name, string extra)
        {
            var accessString = access == CodeItemAccessEnum.Unknown ? string.Empty : access.ToString();
            return string.Join(" ", new[] { accessString, type, name, extra }.Where(s => !string.IsNullOrEmpty(s)));
        }
    }
}
