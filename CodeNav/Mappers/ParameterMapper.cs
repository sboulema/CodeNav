using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.Mappers
{
    public static class ParameterMapper
    {
        /// <summary>
        /// Parse parameters from a method and return a formatted string back
        /// </summary>
        /// <param name="parameters">List of method parameters</param>
        /// <param name="useLongNames">use fullNames for parameter types</param>
        /// <param name="prettyPrint">seperate types with a comma</param>
        /// <returns>string listing all parameter types (eg. (int, string, bool))</returns>
        public static string MapParameters(ParameterListSyntax parameters, bool useLongNames = false, bool prettyPrint = true)
        {
            if (parameters == null) return string.Empty;
            var paramList = (from ParameterSyntax parameter in parameters.Parameters select TypeMapper.Map(parameter.Type, useLongNames)).ToList();
            return prettyPrint ? $"({string.Join(", ", paramList)})" : string.Join(string.Empty, paramList);
        }

        public static string MapParameters(VisualBasicSyntax.ParameterListSyntax parameters, bool useLongNames = false, bool prettyPrint = true)
        {
            if (parameters == null) return string.Empty;
            var paramList = (from VisualBasicSyntax.ParameterSyntax parameter in parameters.Parameters select TypeMapper.Map(parameter.AsClause.Type, useLongNames)).ToList();
            return prettyPrint ? $"({string.Join(", ", paramList)})" : string.Join(string.Empty, paramList);
        }
    }
}
