using System.Linq;
using CodeNav.Models;
using EnvDTE;

namespace CodeNav.Mappers
{
    public static class CodeItemMapper
    {
        public static CodeFunctionItem MapFunction(CodeElement element)
        {
            var function = element as CodeFunction;
            return new CodeFunctionItem
            {
                Name = function.Name,
                StartPoint = function.StartPoint,
                Type = function.Type.AsString,
                Parameters = MapParameters(function),
                IconPath = MapIcon(function)
            };
        }

        public static string MapParameters(CodeFunction function)
        {
            var paramList = (from CodeElement parameter in function.Parameters select (parameter as CodeParameter).Type.AsString).ToList();
            return $"({string.Join(", ", paramList)})";
        }

        public static string MapIcon(CodeFunction function)
        {
            switch (function.Access)
            {
                case vsCMAccess.vsCMAccessPublic:
                    return "Icons/Method/MethodFriend_16x.xaml";
                case vsCMAccess.vsCMAccessPrivate:
                    return "Icons/Method/MethodPrivate_16x.xaml";
                case vsCMAccess.vsCMAccessProjectOrProtected:
                case vsCMAccess.vsCMAccessProtected:
                    return "Icons/Method/MethodProtect_16x.xaml";
                case vsCMAccess.vsCMAccessAssemblyOrFamily:
                case vsCMAccess.vsCMAccessWithEvents:
                case vsCMAccess.vsCMAccessDefault:
                case vsCMAccess.vsCMAccessProject:
                default:
                    return "Icons/Method/Method_purple_16x.xaml";
            }
        }
    }
}
