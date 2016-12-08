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

        private static T MapBase<T>(CodeElement source) where T : CodeItem
        {
            var element = Activator.CreateInstance<T>();
            element.Name = source.Name;
            element.Id = source.Name;
            try
            {
                element.StartPoint = source.StartPoint;
            }
            catch (Exception)
            {

            }          
            element.Foreground = new SolidColorBrush(Colors.Black);
            element.FullName = source.FullName;
            return element;
        }

        private static CodeItem MapFunction(CodeElement element)
        {
            var function = element as CodeFunction;

            var output = MapBase<CodeFunctionItem>(element);
            output.Type = MapReturnType(function.Type);
            output.Parameters = MapParameters(function);
            output.IconPath = function.FunctionKind == vsCMFunction.vsCMFunctionConstructor
                ? "Icons/Method/MethodAdded_16x.xaml"
                : MapIcon<CodeFunction>(element);

            return output;
        }

        private static CodeItem MapMember(CodeElement element)
        {
            if (element.Kind == vsCMElement.vsCMElementFunction)
            {
                return MapFunction(element);
            }
            if (element.Kind == vsCMElement.vsCMElementEnum)
            {
                return MapEnum(element);
            }
            if (element.Kind == vsCMElement.vsCMElementInterface)
            {
                return MapInterface(element);
            }
            if (element.Kind == vsCMElement.vsCMElementVariable)
            {
                return MapVariable(element);
            }
            if (element.Kind == vsCMElement.vsCMElementProperty)
            {
                return MapProperty(element);
            }
            if (element.Kind == vsCMElement.vsCMElementStruct)
            {
                return MapStruct(element);
            }
            if (element.Kind == vsCMElement.vsCMElementClass)
            {
                return MapClass(element);
            }
            return MapBase<CodeItem>(element);
        }

        private static CodeClassItem MapEnum(CodeElement element)
        {
            var enumType = element as CodeEnum;

            var item = MapBase<CodeClassItem>(element);
            item.IconPath = MapIcon<CodeEnum>(element);
            item.Parameters = MapMembers(element as CodeEnum);
            item.BorderBrush = new SolidColorBrush(Colors.DarkGray);

            foreach (CodeElement member in enumType.Members)
            {
                var memberItem = MapMember(member);
                memberItem.IconPath = MapIcon<CodeVariable>(member, true);
                item.Members.Add(memberItem);
            }

            return item;
        }

        private static CodeClassItem MapInterface(CodeElement element)
        {
            var interfaceItem = element as CodeInterface;

            var item = MapBase<CodeClassItem>(element);
            item.IconPath = MapIcon<CodeInterface>(element);
            item.BorderBrush = new SolidColorBrush(Colors.DarkGray);

            foreach (CodeElement member in interfaceItem.Members)
            {
                item.Members.Add(MapFunction(member));
            }

            return item;
        }

        private static CodeClassItem MapStruct(CodeElement element)
        {
            var itemType = element as CodeStruct;

            var item = MapBase<CodeClassItem>(element);
            item.IconPath = MapIcon<CodeStruct>(element);
            item.Parameters = MapMembers(itemType);
            item.BorderBrush = new SolidColorBrush(Colors.DarkGray);

            foreach (CodeElement member in itemType.Members)
            {
                item.Members.Add(MapMember(member));
            }

            return item;
        }

        private static CodeClassItem MapClass(CodeElement element)
        {
            var itemType = element as CodeClass;

            var item = MapBase<CodeClassItem>(element);
            item.IconPath = MapIcon<CodeClass>(element);
            item.Parameters = MapInheritance(element);
            item.BorderBrush = new SolidColorBrush(Colors.DarkGray);

            var classRegions = MapRegions(element.StartPoint, element.EndPoint);
            var implementedInterfaces = MapImplementedInterfaces(element);

            foreach (CodeElement member in itemType.Members)
            {
                var memberItem = MapMember(member);
                if (memberItem != null && !AddToImplementedInterface(implementedInterfaces, memberItem) 
                    && !AddToRegion(classRegions, memberItem))
                {
                    item.Members.Add(memberItem);
                }
            }

            // Add implemented interfaces to class or region if they have a interface member inside them
            if (implementedInterfaces.Any())
            {
                foreach (var interfaceItem in implementedInterfaces)
                {
                    if (interfaceItem.Members.Any())
                    {
                        if (!AddToRegion(classRegions, interfaceItem))
                        {
                            item.Members.Add(interfaceItem);
                        }                       
                    }
                }
            }

            // Add regions to class if they have a region member inside them
            if (classRegions.Any())
            {
                foreach (var region in classRegions)
                {
                    if (region.Members.Any())
                    {
                        item.Members.Add(region);
                    }
                }              
            }

            return item;
        }

        private static List<CodeRegionItem> MapRegions(TextPoint lowerBound, TextPoint upperBound)
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
                    FullName = name,
                    Id = name,
                    StartPoint = start,
                    EndPoint = end,
                    Foreground = new SolidColorBrush(Colors.Black),
                    BorderBrush = new SolidColorBrush(Colors.DarkGray)
                };
                regionList.Add(region);
            }

            return regionList;
        }

        private static CodeItem MapVariable(CodeElement element)
        {
            if (MapAccess(element).Equals("Private") || MapAccess(element).Equals("Protect")) return null;

            var item = MapBase<CodeItem>(element);
            item.IconPath = (element as CodeVariable).IsConstant ? MapIcon<CodeVariable>(element) : "Icons/Field/Field_16x.xaml";
            return item;
        }

        private static CodeFunctionItem MapProperty(CodeElement element)
        {
            if (MapAccess(element).Equals("Private")) return null;

            var prop = element as CodeProperty;

            var item = MapBase<CodeFunctionItem>(element);
            item.Type = MapReturnType(prop.Type);
            item.Parameters = $"{prop.Name} {{{(prop.Getter != null ? "get" : string.Empty)} {(prop.Setter != null ? ", set" : string.Empty)}}}" ;
            item.IconPath = MapIcon<CodeProperty>(element);
            return item;
        }

        private static List<CodeRegionItem> MapImplementedInterfaces(CodeElement element)
        {
            var implementedInterfaces = new List<CodeRegionItem>();
            var codeClass = element as CodeClass;
            foreach (CodeElement implementedInterface in codeClass.ImplementedInterfaces)
            {
                var item = new CodeRegionItem {Name = implementedInterface.Name, Id = implementedInterface.Name};

                foreach (CodeElement implementedInterfaceMember in (implementedInterface as CodeInterface).Members)
                {
                    item.Members.Add(MapMember(implementedInterfaceMember));
                }

                implementedInterfaces.Add(item);
            }

            return implementedInterfaces;
        }

        #region Helpers
        private static string MapReturnType(CodeTypeRef type)
        {
            return type.AsString.Contains(".") ? type.AsString.Split('.').Last() : type.AsString;
        }

        private static string MapInheritance(CodeElement element)
        {
            var codeClass = element as CodeClass;

            var basesList = (from CodeElement bases in codeClass.Bases select bases.Name).ToList();
            var interfaceList = (from CodeElement interfaces in codeClass.ImplementedInterfaces select interfaces.Name).ToList();
            basesList.AddRange(interfaceList);

            return $" : {string.Join(", ", basesList)}";
        }

        private static bool AddToRegion(List<CodeRegionItem> regions, CodeItem item)
        {
            if (item?.StartPoint == null) return false;
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

        private static bool AddToImplementedInterface(List<CodeRegionItem> implementedInterfaces, CodeItem item)
        {
            if (item == null) return false;
            foreach (var interfaceItem in implementedInterfaces)
            {
                foreach (var interfaceMember in interfaceItem.Members)
                {
                    if (interfaceMember.Name.Equals(item.Name))
                    {
                        interfaceItem.Members[interfaceItem.Members.IndexOf(interfaceMember)] = item;

                        // Determing the start/end point of and implemented interface by the start/end point of its members
                        if (interfaceItem.StartPoint == null || item.StartPoint.LessThan(interfaceItem.StartPoint))
                        {
                            interfaceItem.StartPoint = item.StartPoint;
                        }
                        if (interfaceItem.EndPoint == null || item.StartPoint.GreaterThan(interfaceItem.EndPoint))
                        {
                            interfaceItem.EndPoint = item.StartPoint;
                        }

                        return true;
                    }
                }
            }
            return false;
        }

        private static string MapMembers(CodeEnum element)
        {
            var memberList = (from CodeElement member in element.Members select member.Name).ToList();
            return $"{string.Join(", ", memberList)}";
        }

        private static string MapMembers(CodeStruct element)
        {
            var memberList = (from CodeElement member in element.Members select member.Name).ToList();
            return $"{string.Join(", ", memberList)}";
        }

        private static string MapParameters(CodeFunction function)
        {
            var paramList = (from object parameter in function.Parameters select MapReturnType((parameter as CodeParameter).Type)).ToList();
            return $"({string.Join(", ", paramList)})";
        }

        private static string MapAccess(CodeElement element)
        {
            vsCMAccess access;

            switch (element.Kind)
            {
                case vsCMElement.vsCMElementClass:
                    access = (element as CodeClass).Access;
                    break;
                case vsCMElement.vsCMElementFunction:
                    access = (element as CodeFunction).Access;
                    break;
                case vsCMElement.vsCMElementVariable:
                    access = (element as CodeVariable).Access;
                    break;
                case vsCMElement.vsCMElementProperty:
                    access = (element as CodeProperty).Access;
                    break;
                case vsCMElement.vsCMElementInterface:
                    access = (element as CodeInterface).Access;
                    break;
                case vsCMElement.vsCMElementEnum:
                    access = (element as CodeEnum).Access;
                    break;
                case vsCMElement.vsCMElementStruct:
                    access = (element as CodeStruct).Access;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (access)
            {
                case vsCMAccess.vsCMAccessPrivate:
                    return "Private";
                case vsCMAccess.vsCMAccessProject:
                case vsCMAccess.vsCMAccessProtected:
                case vsCMAccess.vsCMAccessProjectOrProtected:
                    return "Protect";
                default:
                    return string.Empty;
            }
        }

        private static string MapIcon<T>(CodeElement element, bool isEnumMember = false)
        {
            var accessString = MapAccess(element);

            if (typeof(T) == typeof(CodeFunction))
            {
                return $"Icons/Method/Method{accessString}_16x.xaml";
            }

            if (typeof(T) == typeof(CodeClass))
            {
                return $"Icons/Class/Class{accessString}_16x.xaml";
            }

            if (typeof(T) == typeof(CodeProperty))
            {
                return $"Icons/Property/Property{accessString}_16x.xaml";
            }

            if (typeof(T) == typeof(CodeEnum))
            {
                return $"Icons/Enum/Enum{accessString}_16x.xaml";
            }

            if (typeof(T) == typeof(CodeVariable))
            {
                return isEnumMember ? $"Icons/Enum/EnumItem{accessString}_16x.xaml" : $"Icons/Constant/Constant{accessString}_16x.xaml";
            }

            if (typeof(T) == typeof(CodeInterface))
            {
                return $"Icons/Interface/Interface{accessString}_16x.xaml";
            }

            if (typeof(T) == typeof(CodeStruct))
            {
                return $"Icons/Structure/Structure{accessString}_16x.xaml";
            }

            return string.Empty;
        }
        #endregion
    }
}
