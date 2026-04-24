using CodeNav.OutOfProc.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class TooltipMapper
{
    public static string Map(SyntaxNode? member, SemanticModel semanticModel, CodeItemAccessEnum access,
        string returnType, string name, ParameterListSyntax parameters)
        => Map(member, access, returnType, name, ParameterMapper.MapParameters(parameters, semanticModel, useLongNames: true));

    public static string Map(SyntaxNode? member, CodeItemAccessEnum access, string type, string name, string extra)
    {
        var accessString = access == CodeItemAccessEnum.Unknown
            ? string.Empty
            : access.ToString();

        var tooltip = string.Join(" ", new[] { accessString, type, name, extra }.Where(tooltipPart => !string.IsNullOrEmpty(tooltipPart)));

        if (TriviaSummaryMapper.HasSummary(member))
        {
            tooltip += $"{Environment.NewLine}{Environment.NewLine}{TriviaSummaryMapper.Map(member!)}";
        }

        return tooltip;
    }
}
