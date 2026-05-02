using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class ParameterMapper
{
    /// <summary>
    /// Parse parameters from a method and return a formatted string back
    /// </summary>
    /// <param name="parameters">List of method parameters</param>
    /// <param name="useLongNames">use fullNames for parameter types</param>
    /// <param name="prettyPrint">separate types with a comma</param>
    /// <returns>string listing all parameter types (eg. (int, string, bool))</returns>
    public static string MapParameters(ParameterListSyntax? parameters, SemanticModel semanticModel,
        bool useLongNames = false, bool prettyPrint = true)
    {
        if (parameters == null)
        {
            return string.Empty;
        }

        var parameterList = parameters
            .Parameters
            .Select(parameter =>
            {
                // https://github.com/dotnet/roslyn-analyzers/issues/7436
                var symbol = semanticModel.GetDeclaredSymbol(parameter) as IParameterSymbol;
                return TypeMapper.Map(symbol?.Type, useLongNames);
            });

        return prettyPrint ? $"({string.Join(", ", parameterList)})" : string.Join(string.Empty, parameterList);
    }
}
