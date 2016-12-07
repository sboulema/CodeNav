using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using CodeNav.Models;
using EnvDTE;

namespace CodeNav.Mappers
{
    public static class CodeItemMapper
    {
        public static List<CodeItem> MapDocument(CodeElements elements)
        {
            var document = new List<CodeItem>();

            foreach (CodeElement codeElement in elements)
            {
                if (codeElement.Kind == vsCMElement.vsCMElementNamespace)
                {
                    foreach (CodeElement namespaceMember in (codeElement as CodeNamespace).Members)
                    {
                        switch (namespaceMember.Kind)
                        {
                            case vsCMElement.vsCMElementClass:
                                document.Add(MapClass(namespaceMember));
                                break;
                            case vsCMElement.vsCMElementEnum:
                                document.Add(MapEnum(namespaceMember));
                                break;
                            case vsCMElement.vsCMElementInterface:
                                document.Add(MapInterface(namespaceMember));
                                break;
                        }
                    }
                }
            }

            return document;
        }

        public static T MapBase<T>(CodeElement source) where T : CodeItem
        {
            var element = Activator.CreateInstance<T>();
            element.Name = source.Name;
            element.Id = source.Name;
            element.StartPoint = source.StartPoint;
            element.Foreground = new SolidColorBrush(Colors.Black);
            element.FullName = source.FullName;
            return element;
        }

        public static CodeFunctionItem MapFunction(CodeElement element)
        {
            var function = element as CodeFunction;

            var output = MapBase<CodeFunctionItem>(element);
            output.Type = function.Type.AsString;
            output.Parameters = MapParameters(function);
            output.IconPath = function.FunctionKind == vsCMFunction.vsCMFunctionConstructor
                ? "Icons/Method/MethodAdded_16x.xaml"
                : MapIcon<CodeFunction>(element);

            return output;
        }

        public static CodeClassItem MapEnum(CodeElement element)
        {
            var enumItem = MapBase<CodeClassItem>(element);
            enumItem.IconPath = MapIcon<CodeEnum>(element);
            enumItem.Parameters = MapMembers(element as CodeEnum);
            enumItem.BorderBrush = new SolidColorBrush(Colors.DarkGray);

            foreach (CodeElement enumMember in (element as CodeEnum).Members)
            {
                var item = MapBase<CodeItem>(enumMember);
                item.IconPath = MapIcon<CodeVariable>(enumMember, true);
                enumItem.Members.Add(item);
            }

            return enumItem;
        }

        private static string MapInheritance(CodeElement element)
        {
            var codeClass = element as CodeClass;

            var basesList = (from CodeElement bases in codeClass.Bases select bases.Name).ToList();
            var interfaceList = (from CodeElement interfaces in codeClass.ImplementedInterfaces select interfaces.Name).ToList();
            basesList.AddRange(interfaceList);

            return $" : {string.Join(", ", basesList)}";
        }

        public static CodeClassItem MapInterface(CodeElement element)
        {
            var item = MapBase<CodeClassItem>(element);
            item.IconPath = MapIcon<CodeInterface>(element);
            item.BorderBrush = new SolidColorBrush(Colors.DarkGray);

            foreach (CodeElement member in (element as CodeInterface).Members)
            {
                item.Members.Add(MapFunction(member));
            }

            return item;
        }

        public static CodeClassItem MapClass(CodeElement element)
        {
            var classItem = MapBase<CodeClassItem>(element);
            classItem.IconPath = MapIcon<CodeClass>(element);
            classItem.Parameters = MapInheritance(element);
            classItem.BorderBrush = new SolidColorBrush(Colors.DarkGray);

            var classRegions = MapRegions(element.StartPoint, element.EndPoint);

            foreach (CodeElement classMember in (element as CodeClass).Members)
            {
                if (classMember.Kind == vsCMElement.vsCMElementFunction)
                {
                    var function = MapFunction(classMember);
                    if (!AddToRegion(classRegions, function))
                    {
                        classItem.Members.Add(function);
                    }                 
                }
                else if (classMember.Kind == vsCMElement.vsCMElementVariable)
                {
                    var variable = classMember as CodeVariable;
                    var constant = MapConstant(classMember);
                    if (variable.IsConstant && variable.Access != vsCMAccess.vsCMAccessPrivate)
                    {
                        if (!AddToRegion(classRegions, constant))
                        {
                            classItem.Members.Add(constant);
                        }
                    }
                }
                else if (classMember.Kind == vsCMElement.vsCMElementProperty)
                {
                    var property = MapProperty(classMember);
                    if (property != null)
                    {
                        if (!AddToRegion(classRegions, property))
                        {
                            classItem.Members.Add(property);
                        }
                    }
                }
                else
                {
                    var item = new CodeItem {Name = classMember.Name, StartPoint = classMember.StartPoint};
                    if (!AddToRegion(classRegions, item))
                    {
                        classItem.Members.Add(item);
                    }
                }
            }

            if (classRegions.Any())
            {
                classItem.Members.AddRange(classRegions);
            }

            return classItem;
        }

        public static List<CodeRegionItem> MapRegions(TextPoint lowerBound, TextPoint upperBound)
        {
            var regionList = new List<CodeRegionItem>();

            var searchPoint = lowerBound.CreateEditPoint();
            var endPoint = upperBound.CreateEditPoint();

            while (searchPoint.FindPattern("#region", 0, ref endPoint))
            {
                var start = searchPoint.CreateEditPoint();
                var name = start.GetLines(start.Line, start.Line + 1).Replace("#region", string.Empty).Trim();
                searchPoint.FindPattern("#endregion", 0, ref endPoint);
                var end = searchPoint.CreateEditPoint();
                var region = new CodeRegionItem
                {
                    Name = name,
                    StartPoint = start,
                    EndPoint = end,
                    Id = name,
                    Foreground = new SolidColorBrush(Colors.Black),
                    BorderBrush = new SolidColorBrush(Colors.DarkGray)
                };
                regionList.Add(region);
            }

            return regionList;
        }

        public static bool AddToRegion(List<CodeRegionItem> regions, CodeItem item)
        {
            foreach (var region in regions)
            {
                if (item.StartPoint.GreaterThan(region.StartPoint) && item.StartPoint.LessThan(region.EndPoint))
                {
                    region.Members.Add(item);
                    return true;
                }
            }
            return false;
        }

        private static CodeItem MapConstant(CodeElement element)
        {
            var constantItem = MapBase<CodeItem>(element);
            constantItem.IconPath = MapIcon<CodeVariable>(element);
            return constantItem;
        }

        private static CodeItem MapProperty(CodeElement element)
        {
            var property = element as CodeProperty;
            if (property.Access == vsCMAccess.vsCMAccessPrivate) return null;

            var item = MapBase<CodeItem>(element);
            item.IconPath = MapIcon<CodeProperty>(element);
            return item;
        }

        private static string MapMembers(CodeEnum element)
        {
            var memberList = (from CodeElement member in element.Members select member.Name).ToList();
            return $"{string.Join(", ", memberList)}";
        }

        private static string MapParameters(CodeFunction function)
        {
            var paramList = (from CodeElement parameter in function.Parameters select (parameter as CodeParameter).Type.AsString).ToList();
            return $"({string.Join(", ", paramList)})";
        }

        public static string MapIcon<T>(CodeElement element, bool isEnumMember = false)
        {
            if (typeof(T) == typeof(CodeFunction))
            {
                switch ((element as CodeFunction).Access)
                {
                    case vsCMAccess.vsCMAccessPrivate:
                        return "Icons/Method/MethodPrivate_16x.xaml";
                    case vsCMAccess.vsCMAccessProjectOrProtected:
                    case vsCMAccess.vsCMAccessProtected:
                        return "Icons/Method/MethodProtect_16x.xaml";
                    default:
                        return "Icons/Method/Method_purple_16x.xaml";
                }
            }

            if (typeof(T) == typeof(CodeClass))
            {
                switch ((element as CodeClass).Access)
                {
                    case vsCMAccess.vsCMAccessPrivate:
                        return "Icons/Class/ClassPrivate_16x.xaml";
                    case vsCMAccess.vsCMAccessProtected:
                    case vsCMAccess.vsCMAccessProjectOrProtected:
                        return "Icons/Class/ClassProtected_16x.xaml";
                    default:
                        return "Icons/Class/Class_yellow_16x.xaml";
                }
            }

            if (typeof(T) == typeof(CodeProperty))
            {
                switch ((element as CodeProperty).Access)
                {
                    case vsCMAccess.vsCMAccessPrivate:
                        return "Icons/Property/PropertyPrivate_16x.xaml";
                    case vsCMAccess.vsCMAccessProtected:
                    case vsCMAccess.vsCMAccessProjectOrProtected:
                        return "Icons/Property/PropertyProtected_16x.xaml";
                    default:
                        return "Icons/Property/Property_16x.xaml";
                }
            }

            if (typeof(T) == typeof(CodeEnum))
            {
                switch ((element as CodeEnum).Access)
                {
                    case vsCMAccess.vsCMAccessPrivate:
                        return "Icons/Enum/EnumPrivate_16x.xaml";
                    case vsCMAccess.vsCMAccessProtected:
                    case vsCMAccess.vsCMAccessProjectOrProtected:
                        return "Icons/Enum/EnumProtect_16x";
                    default:
                        return "Icons/Enum/Enumerator_orange_16x.xaml";
                }
            }

            if (typeof(T) == typeof(CodeVariable))
            {
                switch ((element as CodeVariable).Access)
                {
                    case vsCMAccess.vsCMAccessPrivate:
                        return isEnumMember ? "Icons/Enum/EnumItemPrivate_16x.xaml" : "Icons/Constant/ConstantPrivate_16x.xaml";
                    case vsCMAccess.vsCMAccessProtected:
                    case vsCMAccess.vsCMAccessProjectOrProtected:
                        return isEnumMember ? "Icons/Enum/EnumItemProtect_16x.xaml" : "Icons/Constant/ConstantProtect_16x.xaml";
                    default:
                        return isEnumMember ? "Icons/Enum/EnumItem_16x.xaml" : "Icons/Constant/Constant_16x.xaml";
                }
            }

            if (typeof(T) == typeof(CodeInterface))
            {
                switch ((element as CodeInterface).Access)
                {
                    case vsCMAccess.vsCMAccessPrivate:
                        return "Icons/Interface/InterfacePrivate_16x.xaml";
                    case vsCMAccess.vsCMAccessProtected:
                    case vsCMAccess.vsCMAccessProjectOrProtected:
                        return "Icons/Interface/InterfaceProtect_16x";
                    default:
                        return "Icons/Interface/Interface_blue_16x.xaml";
                }
            }

            return string.Empty;
        }
    }
}
