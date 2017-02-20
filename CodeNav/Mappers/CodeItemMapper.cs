using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using CodeNav.Helpers;
using CodeNav.Models;
using CodeNav.Properties;
using EnvDTE;
using EnvDTE80;
// ReSharper disable SuspiciousTypeConversion.Global

namespace CodeNav.Mappers
{
    public static class CodeItemMapper
    {
        public static List<CodeItem> MapDocument(CodeElements elements)
        {
            var document = new List<CodeItem>();

            if (elements == null) return document;

            foreach (CodeElement2 element in elements)
            {
                document.Add(MapMember(element));
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
                LogHelper.Log($"Could not find a StartPoint for {source.FullName}");
            }           
            element.Foreground = CreateSolidColorBrush(Colors.Black);
            element.Tooltip = element.FullName;
            element.Access = MapAccessToEnum(source);

            element.FontSize = Settings.Default.Font.SizeInPoints;
            element.ParameterFontSize = Settings.Default.Font.SizeInPoints - 1;
            element.RegionFontSize = Settings.Default.Font.SizeInPoints - 3;
            element.FontFamily = new FontFamily(Settings.Default.Font.FontFamily.Name);
            element.FontStyle = FontStyleMapper.Map(Settings.Default.Font.Style);

            return element;
        }

        private static CodeItem MapFunction(CodeElement2 element)
        {
            var function = element as CodeFunction;
            if (function == null) return null;

            var item = MapBase<CodeFunctionItem>(element);
            item.Type = MapReturnType(function.Type);
            item.Parameters = MapParameters(function);
            item.IconPath = function.FunctionKind == vsCMFunction.vsCMFunctionConstructor
                ? "Icons/Method/MethodAdded_16x.xaml"
                : MapIcon<CodeFunction>(element);
            item.Tooltip = $"{item.Access} {function.Type.AsString} {item.Name}{MapParameters(function, true)}";
            item.Id = item.Name + MapParameters(function, true, false);
            item.Kind = function.FunctionKind == vsCMFunction.vsCMFunctionConstructor 
                ? CodeItemKindEnum.Constructor 
                : CodeItemKindEnum.Method;

            return item;
        }

        private static CodeItem MapMember(CodeElement2 element)
        {
            switch (element.Kind)
            {
                case vsCMElement.vsCMElementFunction:
                    return MapFunction(element);
                case vsCMElement.vsCMElementEnum:
                    return MapEnum(element);
                case vsCMElement.vsCMElementInterface:
                    return MapInterface(element);
                case vsCMElement.vsCMElementVariable:
                    return MapVariable(element);
                case vsCMElement.vsCMElementProperty:
                    return MapProperty(element);
                case vsCMElement.vsCMElementStruct:
                    return MapStruct(element);
                case vsCMElement.vsCMElementClass:
                    return MapClass(element);
                case vsCMElement.vsCMElementEvent:
                    return MapEvent(element);
                case vsCMElement.vsCMElementDelegate:
                    return MapDelegate(element);
                case vsCMElement.vsCMElementNamespace:
                    return MapNamespace(element);
                default:
                    return null;
            }
        }

        private static CodeNamespaceItem MapNamespace(CodeElement element)
        {
            var namespaceType = element as CodeNamespace;
            if (namespaceType == null) return null;

            var item = MapBase<CodeNamespaceItem>(element);
            foreach (CodeElement2 namespaceMember in namespaceType.Members)
            {
                item.Members.Add(MapMember(namespaceMember));
            }
            return item;
        }

        private static CodeClassItem MapEnum(CodeElement2 element)
        {
            var enumType = element as CodeEnum;
            if (enumType == null) return null;

            var item = MapBase<CodeClassItem>(element);
            item.IconPath = MapIcon<CodeEnum>(element);
            item.Parameters = MapMembers(enumType);
            item.BorderBrush = CreateSolidColorBrush(Colors.DarkGray);            
            item.Kind = CodeItemKindEnum.Enum;

            foreach (CodeElement2 member in enumType.Members)
            {
                var memberItem = MapMember(member);
                memberItem.IconPath = MapIcon<CodeVariable>(member, true);
                memberItem.Kind = CodeItemKindEnum.EnumItem;
                item.Members.Add(memberItem);
            }

            return item;
        }

        private static CodeClassItem MapInterface(CodeElement2 element)
        {
            var interfaceItem = element as CodeInterface;
            if (interfaceItem == null) return null;

            var item = MapBase<CodeClassItem>(element);
            item.IconPath = MapIcon<CodeInterface>(element);
            item.BorderBrush = CreateSolidColorBrush(Colors.DarkGray);

            foreach (CodeElement2 member in interfaceItem.Members)
            {
                item.Members.Add(MapMember(member));
            }

            return item;
        }

        private static CodeClassItem MapStruct(CodeElement2 element)
        {
            var itemType = element as CodeStruct;
            if (itemType == null) return null;

            var item = MapBase<CodeClassItem>(element);
            item.IconPath = MapIcon<CodeStruct>(element);
            item.Parameters = MapMembers(itemType);
            item.BorderBrush = CreateSolidColorBrush(Colors.DarkGray);
            item.Kind = CodeItemKindEnum.Struct;

            foreach (CodeElement2 member in itemType.Members)
            {
                item.Members.Add(MapMember(member));
            }

            return item;
        }

        private static CodeClassItem MapClass(CodeElement2 element)
        {
            var itemType = element as CodeClass;
            if (itemType == null) return null;

            var item = MapBase<CodeClassItem>(element);
            item.IconPath = MapIcon<CodeClass>(element);
            item.Parameters = MapInheritance(element);
            item.BorderBrush = CreateSolidColorBrush(Colors.DarkGray);

            var classRegions = MapRegions(element.StartPoint, element.EndPoint);
            var implementedInterfaces = MapImplementedInterfaces(element);

            foreach (CodeElement2 member in itemType.Members)
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

        private static CodeItem MapVariable(CodeElement2 element)
        {
            var itemType = element as CodeVariable;
            if (itemType == null) return null;

            var item = MapBase<CodeItem>(element);
            item.IconPath = itemType.IsConstant
                ? MapIcon<CodeVariable>(element)
                : "Icons/Field/Field_16x.xaml";
            item.Kind = itemType.IsConstant 
                ? CodeItemKindEnum.Constant
                : CodeItemKindEnum.Variable;

            if (item.Access == CodeItemAccessEnum.Private || item.Access == CodeItemAccessEnum.Protected) return null;

            return item;
        }

        private static CodeItem MapDelegate(CodeElement2 element)
        {
            var item = MapBase<CodeItem>(element);
            item.Kind = CodeItemKindEnum.Delegate;
            item.IconPath = MapIcon<CodeDelegate>(element);

            if (item.Access == CodeItemAccessEnum.Private) return null;

            return item;
        }

        private static CodeItem MapEvent(CodeElement2 element)
        {
            var item = MapBase<CodeItem>(element);

            if (item.Access == CodeItemAccessEnum.Private) return null;

            item.IconPath = $"Icons/Event/Event{MapAccessEnumToString(item.Access)}_16x.xaml";
            item.Kind = CodeItemKindEnum.Event;

            return item;
        }

        private static CodeFunctionItem MapProperty(CodeElement2 element)
        {
            var prop = element as CodeProperty;
            if (prop == null) return null;

            var item = MapBase<CodeFunctionItem>(element);
            item.Type = MapReturnType(prop.Type);

            if (item.Access == CodeItemAccessEnum.Private) return null;

            try
            {
                if (prop.Getter != null)
                {
                    item.Parameters += "get";
                }
            }
            catch (Exception)
            {
                LogHelper.Log($"Could not find a getter for {element.FullName}");
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
                LogHelper.Log($"Could not find a setter for {element.FullName}");
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
            if (codeClass == null) return null;

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

                foreach (CodeElement2 implementedInterfaceMember in implementedInterface.Members)
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
            if (codeClass == null) return null;

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

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static string MapParameters(CodeFunction function, bool useLongNames = false, bool prettyPrint = true)
        {
            var paramList = (from object parameter in function.Parameters select MapReturnType((parameter as CodeParameter).Type, useLongNames)).ToList();
            return prettyPrint ? $"({string.Join(", ", paramList)})" : string.Join(string.Empty, paramList);
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
        private static CodeItemAccessEnum MapAccessToEnum(CodeElement element)
        {
            vsCMAccess access;

            switch (element.Kind)
            {
                case vsCMElement.vsCMElementClass:
                    if ((element as CodeClass2).InheritanceKind == vsCMInheritanceKind.vsCMInheritanceKindSealed)
                        return CodeItemAccessEnum.Sealed;
                    access = (element as CodeClass).Access;
                    break;
                case vsCMElement.vsCMElementFunction:
                    if ((element as CodeFunction2).OverrideKind ==
                        (vsCMOverrideKind.vsCMOverrideKindOverride | vsCMOverrideKind.vsCMOverrideKindSealed))
                        return CodeItemAccessEnum.Sealed;
                    access = (element as CodeFunction).Access;
                    break;
                case vsCMElement.vsCMElementVariable:
                    access = (element as CodeVariable).Access;
                    break;
                case vsCMElement.vsCMElementProperty:
                    if ((element as CodeProperty2).OverrideKind ==
                        (vsCMOverrideKind.vsCMOverrideKindOverride | vsCMOverrideKind.vsCMOverrideKindSealed))
                        return CodeItemAccessEnum.Sealed;
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
                case vsCMElement.vsCMElementNamespace:
                    access = vsCMAccess.vsCMAccessDefault;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (access)
            {
                case vsCMAccess.vsCMAccessPrivate:
                    return CodeItemAccessEnum.Private;
                case vsCMAccess.vsCMAccessProject:
                case vsCMAccess.vsCMAccessProtected:
                case vsCMAccess.vsCMAccessProjectOrProtected:
                    return CodeItemAccessEnum.Protected;
                case vsCMAccess.vsCMAccessPublic:
                    return CodeItemAccessEnum.Public;
                case vsCMAccess.vsCMAccessAssemblyOrFamily:
                    return CodeItemAccessEnum.Internal;
                default:
                    return CodeItemAccessEnum.Unknown;
            }
        }

        private static string MapAccessEnumToString(CodeItemAccessEnum item)
        {
            switch (item)
            {
                case CodeItemAccessEnum.Private:
                    return "Private";
                case CodeItemAccessEnum.Internal:
                case CodeItemAccessEnum.Protected:
                    return "Protect";
                case CodeItemAccessEnum.Sealed:
                    return "Sealed";
                default:
                    return string.Empty;
            }
        }

        private static string MapIcon<T>(CodeElement2 element, bool isEnumMember = false)
        {
            var accessString = MapAccessEnumToString(MapAccessToEnum(element));

            if (typeof(T) == typeof(CodeFunction))
            {
                return $"Icons/Method/Method{accessString}_16x.xaml";
            }

            if (typeof(T) == typeof(CodeClass))
            {
                return $"Icons/Class/Class{accessString}_16x.xaml";
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
