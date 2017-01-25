using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CodeNav.Models;
using CodeNav.Properties;
using EnvDTE;
using EnvDTE80;

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
            element.FullName = source.FullName;
            element.Id = source.Name;
            try
            {
                element.StartPoint = source.StartPoint;
            }
            catch (Exception)
            {

            }
            element.Foreground = CreateSolidColorBrush(Colors.Black);
            element.Tooltip = element.FullName;
            return element;
        }

        private static CodeItem MapFunction(CodeElement element)
        {
            var function = element as CodeFunction;

            var item = MapBase<CodeFunctionItem>(element);
            item.Type = MapReturnType(function.Type);
            item.Parameters = MapParameters(function);
            item.IconPath = function.FunctionKind == vsCMFunction.vsCMFunctionConstructor
                ? "Icons/Method/MethodAdded_16x.xaml"
                : MapIcon<CodeFunction>(element);
            item.Tooltip = $"{MapAccess(element)} {function.Type.AsString} {item.Name}{MapParameters(function, true)}";
            item.Id = item.Name + MapParameters(function, true, false);
            item.Kind = function.FunctionKind == vsCMFunction.vsCMFunctionConstructor 
                ? CodeItemKindEnum.Constructor 
                : CodeItemKindEnum.Method;

            return item;
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
            if (element.Kind == vsCMElement.vsCMElementEvent)
            {
                return MapEvent(element);
            }
            if (element.Kind == vsCMElement.vsCMElementDelegate)
            {
                return MapDelegate(element);
            }
            return MapBase<CodeItem>(element);
        }

        private static CodeClassItem MapEnum(CodeElement element)
        {
            var enumType = element as CodeEnum;

            var item = MapBase<CodeClassItem>(element);
            item.IconPath = MapIcon<CodeEnum>(element);
            item.Parameters = MapMembers(element as CodeEnum);
            item.BorderBrush = CreateSolidColorBrush(Colors.DarkGray);            

            foreach (CodeElement member in enumType.Members)
            {
                var memberItem = MapMember(member);
                memberItem.IconPath = MapIcon<CodeVariable>(member, true);
                memberItem.Kind = CodeItemKindEnum.EnumItem;
                item.Members.Add(memberItem);
            }

            return item;
        }

        private static CodeClassItem MapInterface(CodeElement element)
        {
            var interfaceItem = element as CodeInterface;

            var item = MapBase<CodeClassItem>(element);
            item.IconPath = MapIcon<CodeInterface>(element);
            item.BorderBrush = CreateSolidColorBrush(Colors.DarkGray);

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
            item.BorderBrush = CreateSolidColorBrush(Colors.DarkGray);

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
            item.BorderBrush = CreateSolidColorBrush(Colors.DarkGray);

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
                    Foreground = CreateSolidColorBrush(Colors.Black),
                    BorderBrush = CreateSolidColorBrush(Colors.DarkGray)
            };
                regionList.Add(region);
            }

            return regionList;
        }

        private static CodeItem MapVariable(CodeElement element)
        {
            if (MapAccess(element).Equals("Private") || MapAccess(element).Equals("Protect")) return null;

            var item = MapBase<CodeItem>(element);
            item.IconPath = (element as CodeVariable).IsConstant
                ? MapIcon<CodeVariable>(element)
                : "Icons/Field/Field_16x.xaml";
            item.Kind = (element as CodeVariable).IsConstant 
                ? CodeItemKindEnum.Constant
                : CodeItemKindEnum.Variable;
            return item;
        }

        private static CodeItem MapDelegate(CodeElement element)
        {
            if (MapAccess(element).Equals("Private")) return null;

            var item = MapBase<CodeItem>(element);
            item.IconPath = MapIcon<CodeDelegate>(element);
            return item;
        }

        private static CodeItem MapEvent(CodeElement element)
        {
            if (MapAccess(element).Equals("Private")) return null;

            var item = MapBase<CodeItem>(element);
            var accessString = MapAccess(element);
            item.IconPath = $"Icons/Event/Event{accessString}_16x.xaml";
            item.Kind = CodeItemKindEnum.Event;
            return item;
        }

        private static CodeFunctionItem MapProperty(CodeElement element)
        {
            if (MapAccess(element).Equals("Private")) return null;

            var prop = element as CodeProperty;

            var item = MapBase<CodeFunctionItem>(element);
            item.Type = MapReturnType(prop.Type);

            try
            {
                if (prop.Getter != null)
                {
                    item.Parameters += "get";
                }
            }
            catch (Exception)
            {
            }
            try
            {
                if (prop.Setter != null)
                {
                    item.Parameters += ",set";
                }
            }
            catch (Exception)
            {
            }

            item.Parameters = $"{prop.Name} {{{item.Parameters}}}";
            item.IconPath = MapIcon<CodeProperty>(element);
            item.Tooltip = $"{prop.Type.AsString} {item.Name}";
            item.Kind = CodeItemKindEnum.Property;
            return item;
        }

        private static List<CodeRegionItem> MapImplementedInterfaces(CodeElement element)
        {
            var implementedInterfaces = new List<CodeRegionItem>();
            var codeClass = element as CodeClass;

            var list = new List<CodeInterface>();
            GetImplementedInterfaces(list, codeClass.ImplementedInterfaces);

            foreach (var implementedInterface in list)
            {
                var item = new CodeRegionItem
                {
                    Name = implementedInterface.Name,
                    Id = implementedInterface.Name,
                    FullName = implementedInterface.Name
                };

                foreach (CodeElement implementedInterfaceMember in implementedInterface.Members)
                {
                    item.Members.Add(MapMember(implementedInterfaceMember));
                }

                implementedInterfaces.Add(item);
            }

            return implementedInterfaces;
        }

        #region Helpers
        private static void GetImplementedInterfaces(List<CodeInterface> list, CodeElements implementedInterfaces)
        {
            foreach (CodeInterface implementedInterface in implementedInterfaces)
            {
                if (!list.Any(i => i.Name.Equals(implementedInterface.Name)))
                {
                    list.Add(implementedInterface);
                }               
                GetImplementedInterfaces(list, implementedInterface.Bases);
            }
        }

        private static string MapReturnType(CodeTypeRef type, bool useLongNames = false)
        {
            if (useLongNames) return type.AsString;

            var match = new Regex("(.*)<(.*)>").Match(type.AsString);
            if (match.Success)
            {
                return $"{match.Groups[1].Value.Split('.').Last()}<{match.Groups[2].Value.Split('.').Last()}>";
            }
            return type.AsString.Contains(".") ? type.AsString.Split('.').Last() : type.AsString;            
        }

        private static SolidColorBrush CreateSolidColorBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
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
                    if (interfaceMember.Id.Equals(item.Id))
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

        private static string MapParameters(CodeFunction function, bool useLongNames = false, bool prettyPrint = true)
        {
            var paramList = (from object parameter in function.Parameters select MapReturnType((parameter as CodeParameter).Type, useLongNames)).ToList();
            return prettyPrint ? $"({string.Join(", ", paramList)})" : string.Join(string.Empty, paramList);
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
                case vsCMElement.vsCMElementEvent:
                    access = (element as CodeEvent).Access;
                    break;
                case vsCMElement.vsCMElementDelegate:
                    access = (element as CodeDelegate).Access;
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

            if (typeof(T) == typeof(CodeDelegate))
            {
                return $"Icons/Delegate/Delegate{accessString}_16x.xaml";
            }

            return string.Empty;
        }
        #endregion
    }
}
