using CodeNav.OutOfProc.Constants;
using Microsoft.CodeAnalysis;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class TooltipMapper
{
    public static string Map(
        SyntaxNode node,
        CodeItemAccessEnum access,
        string returnType,
        string name,
        string parameters)
    {
        var accessText = access switch
        {
            CodeItemAccessEnum.Public => "Public",
            CodeItemAccessEnum.Private => "Private",
            CodeItemAccessEnum.Protected => "Protected",
            CodeItemAccessEnum.Internal => "Friend",
            _ => string.Empty
        };

        var parts = new List<string>();

        if (!string.IsNullOrEmpty(accessText))
        {
            parts.Add(accessText);
        }

        if (!string.IsNullOrEmpty(name))
        {
            parts.Add(name);
        }

        if (!string.IsNullOrEmpty(parameters))
        {
            parts.Add(parameters);
        }

        if (!string.IsNullOrEmpty(returnType))
        {
            parts.Add($"As {returnType}");
        }

        return string.Join(" ", parts);
    }
}