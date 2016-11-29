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

        public static CodeClassItem MapClass(CodeElement element)
        {
            var classItem = new CodeClassItem
            {
                Name = element.Name,
                StartPoint = element.StartPoint,
                IconPath = MapIcon(element as CodeClass)
            };

            foreach (CodeElement classMember in (element as CodeClass).Members)
            {
                if (classMember.Kind == vsCMElement.vsCMElementFunction)
                {
                    classItem.Members.Add(MapFunction(classMember));
                }
                else if (classMember.Kind == vsCMElement.vsCMElementVariable)
                {
                    var variable = MapVariable(classMember);
                    if (variable != null)
                    {
                        classItem.Members.Add(variable);
                    }                 
                }
                else if (classMember.Kind == vsCMElement.vsCMElementProperty)
                {
                    var property = MapProperty(classMember);
                    if (property != null)
                    {
                        classItem.Members.Add(property);
                    }
                }
                else
                {
                    classItem.Members.Add(new CodeItem { Name = classMember.Name, StartPoint = classMember.StartPoint });
                }
            }

            return classItem;
        }

        private static CodeItem MapVariable(CodeElement element)
        {
            var variable = element as CodeVariable;
            if (variable.Access == vsCMAccess.vsCMAccessPrivate) return null;
            return new CodeItem
            {
                Name = variable.Name,
                StartPoint = variable.StartPoint,
                IconPath = MapIcon(variable)
            };
        }

        private static CodeItem MapProperty(CodeElement element)
        {
            var property = element as CodeProperty;
            if (property.Access == vsCMAccess.vsCMAccessPrivate) return null;
            return new CodeItem
            {
                Name = property.Name,
                StartPoint = property.StartPoint,
                IconPath = MapIcon(property)
            };
        }

        private static string MapParameters(CodeFunction function)
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
                default:
                    return "Icons/Method/Method_purple_16x.xaml";
            }
        }

        public static string MapIcon(CodeClass codeClass)
        {
            switch (codeClass.Access)
            {
                case vsCMAccess.vsCMAccessPublic:
                    return "Icons/Class/ClassFriend_16x.xaml";
                case vsCMAccess.vsCMAccessPrivate:
                    return "Icons/Class/ClassPrivate_16x.xaml";
                case vsCMAccess.vsCMAccessProtected:
                case vsCMAccess.vsCMAccessProjectOrProtected:
                    return "Icons/Class/ClassProtected_16x.xaml";
                default:
                    return "Icons/Class/Class_yellow_16x.xaml";
            }
        }

        public static string MapIcon(CodeProperty property)
        {
            switch (property.Access)
            {
                case vsCMAccess.vsCMAccessPublic:
                    return "Icons/Property/PropertyFriend_16x.xaml";
                case vsCMAccess.vsCMAccessPrivate:
                    return "Icons/Property/PropertyPrivate_16x.xaml";
                case vsCMAccess.vsCMAccessProtected:
                case vsCMAccess.vsCMAccessProjectOrProtected:
                    return "Icons/Property/PropertyProtected_16x.xaml";
                default:
                    return "Icons/Property/Property_16x.xaml";
            }
        }

        public static string MapIcon(CodeVariable variable)
        {
            if (!variable.IsConstant) return string.Empty;

            switch (variable.Access)
            {
                case vsCMAccess.vsCMAccessPublic:
                    return "Icons/Constant/ConstantFriend_16x.xaml";
                case vsCMAccess.vsCMAccessPrivate:
                    return "Icons/Constant/ConstantPrivate_16x.xaml";
                case vsCMAccess.vsCMAccessProtected:
                case vsCMAccess.vsCMAccessProjectOrProtected:
                    return "Icons/Constant/ConstantProtected_16x.xaml";
                default:
                    return "Icons/Constant/Constant_16x.xaml";
            }
        }
    }
}
