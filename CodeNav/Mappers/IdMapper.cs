using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using Zu.TypeScript.TsTypes;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.Mappers
{
    /// <summary>
    /// Creates an unique id for a CodeItem based on its name and parameters
    /// </summary>
    public static class IdMapper
    {
        public static string MapId(SyntaxToken identifier, ParameterListSyntax parameters)
        {
            return MapId(identifier.Text, parameters);
        }

        public static string MapId(SyntaxToken identifier, VisualBasicSyntax.ParameterListSyntax parameters, SemanticModel semanticModel)
        {
            return MapId(identifier.Text, parameters, semanticModel);
        }

        public static string MapId(string name, NodeArray<ParameterDeclaration> parameters)
        {
            return name + string.Join(string.Empty, parameters.Select(p => p.IdentifierStr));
        }

        public static string MapId(string name, ParameterListSyntax parameters)
        {
            return name + ParameterMapper.MapParameters(parameters, true, false);
        }

        public static string MapId(string name, VisualBasicSyntax.ParameterListSyntax parameters, SemanticModel semanticModel)
        {
            return name + ParameterMapper.MapParameters(parameters, semanticModel, true, false);
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
}
