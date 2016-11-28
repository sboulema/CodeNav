using System.Linq;
using EnvDTE;

namespace CodeNav.Mappers
{
    public static class CodeItemMapper
    {
        public static string MapParameters(CodeFunction function)
        {
            var paramList = (from CodeElement parameter in function.Parameters select (parameter as CodeParameter).Type.AsString).ToList();
            return $"({string.Join(", ", paramList)})";
        }
    }
}
