using CodeNav.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.Languages.CSharp.Mappers;

public static class TooltipMapper
{
    public static string Map(CodeItemAccessEnum access, string type, string name, ParameterListSyntax parameters)
        => Map(access, type, name, ParameterMapper.MapParameters(parameters, true));

    public static string Map(CodeItemAccessEnum access, string type, string name, string extra)
    {
        var accessString = access == CodeItemAccessEnum.Unknown ? string.Empty : access.ToString();
        return string.Join(" ", new[] { accessString, type, name, extra }.Where(s => !string.IsNullOrEmpty(s)));
    }
}
