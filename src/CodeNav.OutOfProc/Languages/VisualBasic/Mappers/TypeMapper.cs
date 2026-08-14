using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class TypeMapper
{
    public static string Map(SimpleAsClauseSyntax? asClause)
        => asClause?.Type?.ToString() ?? string.Empty;

    public static string Map(AsClauseSyntax? asClause)
        => asClause?.ToString()
            .Replace("As ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim() ?? string.Empty;
}