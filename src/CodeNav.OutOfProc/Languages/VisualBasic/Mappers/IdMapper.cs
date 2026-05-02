using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Collections.Immutable;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

/// <summary>
/// Creates an unique id for a CodeItem based on its name and parameters
/// </summary>
public static class IdMapper
{
    public static string MapId(SyntaxToken identifier, SemanticModel semanticModel, ParameterListSyntax parameters)
    {
        return MapId(identifier.Text, semanticModel, parameters);
    }

    public static string MapId(string name, SemanticModel semanticModel, ParameterListSyntax parameters)
    {
        return name + ParameterMapper.MapParameters(parameters, semanticModel, useLongNames: true, prettyPrint: false);
    }

    public static string MapId(string name, ImmutableArray<IParameterSymbol> parameters, bool useLongNames, bool prettyPrint)
    {
        return name + MapParameters(parameters, useLongNames, prettyPrint);
    }

    private static string MapParameters(ImmutableArray<IParameterSymbol> parameters, bool useLongNames = false, bool prettyPrint = true)
    {
        var paramList = (from IParameterSymbol parameter in parameters select TypeMapper.Map(parameter.Type, useLongNames)).ToList();
        return prettyPrint ? $"({string.Join(", ", paramList)})" : string.Join(string.Empty, paramList);
    }
}
