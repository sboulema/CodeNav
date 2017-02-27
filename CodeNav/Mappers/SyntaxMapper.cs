using System;
using System.Collections.Generic;
using System.Windows.Media;
using CodeNav.Models;
using CodeNav.Properties;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Text;

namespace CodeNav.Mappers
{
	public static class SyntaxMapper
    {
        private static CodeViewUserControl _control;
        private static SyntaxTree _tree;

        public static List<CodeItem> MapDocument(Document document, CodeViewUserControl control)
        {
            _control = control;

            var text = GetText(document);
            _tree = CSharpSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)_tree.GetRoot();
            var result = new List<CodeItem>();

            foreach (var member in root.Members)
            {
                result.Add(MapMember(member));
            }

            return result;
        }

        private static CodeItem MapMember(MemberDeclarationSyntax member)
        {
            switch (member.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    return MapMethod(member as MethodDeclarationSyntax);
                case SyntaxKind.EnumDeclaration:
                    return MapEnum(member as EnumDeclarationSyntax);
                case SyntaxKind.EnumMemberDeclaration:
                    return MapEnumMember(member as EnumMemberDeclarationSyntax);
                case SyntaxKind.InterfaceDeclaration:
                    return MapInterface(member as InterfaceDeclarationSyntax);
                case SyntaxKind.FieldDeclaration:
                    return MapField(member as FieldDeclarationSyntax);
                case SyntaxKind.PropertyDeclaration:
                    return MapProperty(member as PropertyDeclarationSyntax);
                case SyntaxKind.StructDeclaration:
                    return MapStruct(member as StructDeclarationSyntax);
                case SyntaxKind.ClassDeclaration:
                    return MapClass(member as ClassDeclarationSyntax);
                case SyntaxKind.EventDeclaration:
                    return MapEvent(member as EventDeclarationSyntax);
                case SyntaxKind.DelegateDeclaration:
                    return MapDelegate(member as DelegateDeclarationSyntax);
                case SyntaxKind.NamespaceDeclaration:
                    return MapNamespace(member as NamespaceDeclarationSyntax);
                case SyntaxKind.ConstructorDeclaration:
                    return MapConstructor(member as ConstructorDeclarationSyntax);
                default:
                    return null;
            }
        }

        private static CodeItem MapEnumMember(EnumMemberDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = MapBase<CodeItem>(member, member.Identifier);
            item.Kind = CodeItemKindEnum.EnumMember;
            item.IconPath = MapIcon(item.Kind, item.Access);

            return item;
        }

        private static CodeItem MapField(FieldDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = MapBase<CodeItem>(member, member.Declaration.Variables.First().Identifier, member.Modifiers);
            if (item.Access == CodeItemAccessEnum.Private || item.Access == CodeItemAccessEnum.Protected) return null;
            item.Kind = IsConstant(member.Modifiers)
                ? CodeItemKindEnum.Constant
                : CodeItemKindEnum.Variable;
            item.IconPath = MapIcon(item.Kind, item.Access);

            return item;
        }

        private static bool IsConstant(SyntaxTokenList modifiers)
        {
            return modifiers.Any(m => m.Kind() == SyntaxKind.ConstKeyword);
        }

        private static CodeItem MapDelegate(DelegateDeclarationSyntax member)
        {
            var item = MapBase<CodeItem>(member, member.Identifier, member.Modifiers);
            if (item.Access == CodeItemAccessEnum.Private) return null;
            item.Kind = CodeItemKindEnum.Delegate;
            item.IconPath = MapIcon(item.Kind, item.Access);
            return item;
        }

        private static CodeItem MapEvent(EventDeclarationSyntax member)
        {
            var item = MapBase<CodeItem>(member, member.Identifier, member.Modifiers);
            if (item.Access == CodeItemAccessEnum.Private) return null;
            item.Kind = CodeItemKindEnum.Event;
            item.IconPath = MapIcon(item.Kind, item.Access);
            return item;
        }

        private static CodeClassItem MapStruct(StructDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = MapBase<CodeClassItem>(member, member.Identifier, member.Modifiers);
            item.Kind = CodeItemKindEnum.Struct;
            item.IconPath = MapIcon(item.Kind, item.Access);
            item.BorderBrush = CreateSolidColorBrush(Colors.DarkGray);
            
            foreach (var structMember in member.Members)
            {
                item.Members.Add(MapMember(structMember));
            }

            return item;
        }

        private static CodeClassItem MapEnum(EnumDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = MapBase<CodeClassItem>(member, member.Identifier, member.Modifiers);
            item.Kind = CodeItemKindEnum.Enum;
            item.IconPath = MapIcon(item.Kind, item.Access);
            item.Parameters = MapMembersToString(member.Members);
            item.BorderBrush = CreateSolidColorBrush(Colors.DarkGray);          

            foreach (var enumMember in member.Members)
            {
                item.Members.Add(MapMember(enumMember));
            }

            return item;
        }

        private static CodeClassItem MapClass(ClassDeclarationSyntax member)
		{
			//TODO: Implement interfaces and regions

			if (member == null) return null;

			var item = MapBase<CodeClassItem>(member, member.Identifier, member.Modifiers);
			item.Kind = CodeItemKindEnum.Class;
			item.IconPath = MapIcon(item.Kind, item.Access);
			item.Parameters = MapInheritance(member);
			item.BorderBrush = CreateSolidColorBrush(Colors.DarkGray);

			var regions = MapRegions(member.Span);
			//var implementedInterfaces = MapImplementedInterfaces(element);

			foreach (var classMember in member.Members)
			{
				var memberItem = MapMember(classMember);
				//if (memberItem != null && !AddToImplementedInterface(implementedInterfaces, memberItem)
				//	&& !AddToRegion(classRegions, memberItem))
				//{
			    if (memberItem != null && !AddToRegion(regions, memberItem))
			    {
                    item.Members.Add(memberItem);
                }
			}

            // Add implemented interfaces to class or region if they have a interface member inside them
            //if (implementedInterfaces.Any())
            //{
            //	foreach (var interfaceItem in implementedInterfaces)
            //	{
            //		if (interfaceItem.Members.Any())
            //		{
            //			if (!AddToRegion(classRegions, interfaceItem))
            //			{
            //				item.Members.Add(interfaceItem);
            //			}
            //		}
            //	}
            //}

            // Add regions to class if they have a region member inside them
            if (regions.Any())
            {
                foreach (var region in regions)
                {
                    if (region.Members.Any())
                    {
                        item.Members.Add(region);
                    }
                }
            }

            return item;
		}

        private static bool AddToRegion(List<CodeRegionItem> regions, CodeItem item)
        {
            if (item?.StartLine == null) return false;
            foreach (var region in regions)
            {
                if (item.StartLine >= region.StartLine && item.StartLine <= region.EndLine)
                {
                    region.Members.Add(item);
                    return true;
                }
            }
            return false;
        }

        private static List<CodeRegionItem> MapRegions(TextSpan span)
        {
            var root = _tree.GetRoot();
            var regionList = new List<CodeRegionItem>();

            foreach (var regionDirective in root.DescendantTrivia().Where(i => i.Kind() == SyntaxKind.RegionDirectiveTrivia))
            {
                regionList.Add(MapRegion(regionDirective.ToString().Replace("#region ", string.Empty), regionDirective.Span));
            }
                
            var count = regionList.Count;

            foreach (var endRegionDirective in root.DescendantTrivia().Where(j => j.Kind() == SyntaxKind.EndRegionDirectiveTrivia))
            {
                regionList[--count].EndLine = _tree.GetLineSpan(endRegionDirective.Span).StartLinePosition.Line;
            }

            return regionList;
        }

        private static CodeRegionItem MapRegion(string name, TextSpan span)
        {
            return new CodeRegionItem
            {
                Name = name,
                FullName = name,
                Id = name,
                StartLine = _tree.GetLineSpan(span).StartLinePosition.Line,
                Foreground = CreateSolidColorBrush(Colors.Black),
                BorderBrush = CreateSolidColorBrush(Colors.DarkGray),
                FontSize = Settings.Default.Font.SizeInPoints - 2
            };
        }

        private static string MapInheritance(ClassDeclarationSyntax member)
		{
			if (member?.BaseList == null) return string.Empty;

		    var inheritanceList = (from BaseTypeSyntax bases in member.BaseList.Types select bases.Type.ToString()).ToList();

		    return !inheritanceList.Any() ? string.Empty : $" : {string.Join(", ", inheritanceList)}";
		}

		private static T MapBase<T>(MemberDeclarationSyntax source, SyntaxToken identifier, SyntaxTokenList modifiers) where T : CodeItem
		{
			return MapBase<T>(source, identifier.Text, modifiers);
		}

		private static T MapBase<T>(MemberDeclarationSyntax source, NameSyntax name) where T : CodeItem
		{
			return MapBase<T>(source, name.ToString(), new SyntaxTokenList());
		}

        private static T MapBase<T>(MemberDeclarationSyntax source, SyntaxToken identifier) where T : CodeItem
        {
            return MapBase<T>(source, identifier.Text, new SyntaxTokenList());
        }

        private static T MapBase<T>(MemberDeclarationSyntax source, string name, SyntaxTokenList modifiers) where T : CodeItem
        {
            var element = Activator.CreateInstance<T>();

            element.Name = name;
            element.FullName = name;
            element.Id = name;
            element.Tooltip = name;
            element.StartLine = _tree.GetLineSpan(source.Span).StartLinePosition.Line;
            element.Foreground = CreateSolidColorBrush(Colors.Black);
            element.Access = MapAccessToEnum(modifiers);
            element.FontSize = Settings.Default.Font.SizeInPoints;
            element.ParameterFontSize = Settings.Default.Font.SizeInPoints - 1;
            element.FontFamily = new FontFamily(Settings.Default.Font.FontFamily.Name);
            element.FontStyle = FontStyleMapper.Map(Settings.Default.Font.Style);
			element.Control = _control;

            return element;
        }

		private static CodeItemAccessEnum MapAccessToEnum(SyntaxTokenList modifiers)
		{
		    if (modifiers.Any(m => m.Kind() == SyntaxKind.SealedKeyword))
			{
				return CodeItemAccessEnum.Sealed;
			}
		    if (modifiers.Any(m => m.Kind() == SyntaxKind.PublicKeyword))
		    {
		        return CodeItemAccessEnum.Public;
		    }
		    if (modifiers.Any(m => m.Kind() == SyntaxKind.PrivateKeyword))
		    {
		        return CodeItemAccessEnum.Private;
		    }
		    if (modifiers.Any(m => m.Kind() == SyntaxKind.ProtectedKeyword))
		    {
		        return CodeItemAccessEnum.Protected;
		    }
		    if (modifiers.Any(m => m.Kind() == SyntaxKind.InternalKeyword))
		    {
		        return CodeItemAccessEnum.Internal;
		    }
		    return CodeItemAccessEnum.Unknown;
		}

		private static CodeNamespaceItem MapNamespace(NamespaceDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = MapBase<CodeNamespaceItem>(member, member.Name);
            foreach (var namespaceMember in member.Members)
            {
                item.Members.Add(MapMember(namespaceMember));
            }
            return item;
        }

		private static CodeClassItem MapInterface(InterfaceDeclarationSyntax member)
		{
			if (member == null) return null;

			var item = MapBase<CodeClassItem>(member, member.Identifier, member.Modifiers);
			item.Kind = CodeItemKindEnum.Interface;
			item.BorderBrush = CreateSolidColorBrush(Colors.DarkGray);
			item.IconPath = MapIcon(item.Kind, item.Access);

			foreach (var interfaceMember in member.Members)
			{
				item.Members.Add(MapMember(interfaceMember));
			}

			return item;
		}

		private static CodeItem MapMethod(MethodDeclarationSyntax member)
        {
            if (member == null) return null;

			var item = MapBase<CodeFunctionItem>(member, member.Identifier, member.Modifiers);
			item.Type = MapReturnType(member.ReturnType);
			item.Parameters = MapParameters(member.ParameterList);
			item.Tooltip = $"{item.Access} {item.Type} {item.Name}{MapParameters(member.ParameterList, true)}";
            item.Id = MapId(member.Identifier, member.ParameterList);
            item.Kind = CodeItemKindEnum.Method;
			item.IconPath = MapIcon(item.Kind, item.Access);

			return item;
        }

        private static CodeItem MapConstructor(ConstructorDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = MapBase<CodeFunctionItem>(member, member.Identifier, member.Modifiers);
            item.Parameters = MapParameters(member.ParameterList);
            item.Tooltip = $"{item.Access} {item.Type} {item.Name}{MapParameters(member.ParameterList, true)}";
            item.Id = MapId(member.Identifier, member.ParameterList);
            item.Kind = CodeItemKindEnum.Constructor;
            item.IconPath = MapIcon(item.Kind, item.Access);

            return item;
        }

        public static string MapId(SyntaxToken identifier, ParameterListSyntax parameters)
        {
            return identifier.Text + MapParameters(parameters, true, false);
        }

        /// <summary>
        /// Parse parameters from a method and return a formatted string back
        /// </summary>
        /// <param name="parameters">List of method parameters</param>
        /// <param name="useLongNames">us fullNames for parameter types</param>
        /// <param name="prettyPrint">seperate types with a comma</param>
        /// <returns>string listing all parameter types (eg. (int, string, bool))</returns>
        private static string MapParameters(ParameterListSyntax parameters, bool useLongNames = false, bool prettyPrint = true)
		{
			var paramList = (from ParameterSyntax parameter in parameters.Parameters select MapReturnType(parameter.Type, useLongNames)).ToList();
		    if (!paramList.Any()) return string.Empty;
			return prettyPrint ? $"({string.Join(", ", paramList)})" : string.Join(string.Empty, paramList);
		}

		private static CodeFunctionItem MapProperty(PropertyDeclarationSyntax member)
		{
			if (member == null) return null;

			var item = MapBase<CodeFunctionItem>(member, member.Identifier, member.Modifiers);
			item.Type = MapReturnType(member.Type);

			if (item.Access == CodeItemAccessEnum.Private) return null;

			if (member.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.GetAccessorDeclaration))
			{
				item.Parameters += "get";
			}

			if (member.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.SetAccessorDeclaration))
			{
				item.Parameters += ",set";
			}

			item.Parameters = $"{item.Name} {{{item.Parameters}}}";
			item.Tooltip = $"{item.Type} {item.Name}";
			item.Kind = CodeItemKindEnum.Property;
			item.IconPath = MapIcon(item.Kind, item.Access);
			return item;
		}

		private static string MapIcon(CodeItemKindEnum kind, CodeItemAccessEnum access)
		{
			const string iconFolder = "pack://application:,,,/CodeNav;component/Icons";
			var accessString = MapAccessEnumToString(access);

			switch (kind)
			{
				case CodeItemKindEnum.Class:
					return $"{iconFolder}/Class/Class{accessString}_16x.xaml";
				case CodeItemKindEnum.Constant:
					return $"{iconFolder}/Constant/Constant{accessString}_16x.xaml";
				case CodeItemKindEnum.Constructor:
					return $"{iconFolder}/Method/MethodAdded_16x.xaml";
				case CodeItemKindEnum.Delegate:
					return $"{iconFolder}/Delegate/Delegate{accessString}_16x.xaml";
				case CodeItemKindEnum.Enum:
					return $"{iconFolder}/Enum/Enum{accessString}_16x.xaml";
				case CodeItemKindEnum.EnumMember:
					return $"{iconFolder}/Enum/EnumItem{accessString}_16x.xaml";
				case CodeItemKindEnum.Event:
					return $"{iconFolder}/Event/Event{accessString}_16x.xaml";
				case CodeItemKindEnum.Interface:
					return $"{iconFolder}/Interface/Interface{accessString}_16x.xaml";
				case CodeItemKindEnum.Method:
					return $"{iconFolder}/Method/Method{accessString}_16x.xaml";
				case CodeItemKindEnum.Property:
					return $"{iconFolder}/Property/Property{accessString}_16x.xaml";
				case CodeItemKindEnum.Struct:
					return $"{iconFolder}/Structure/Structure{accessString}_16x.xaml";
				case CodeItemKindEnum.Variable:
					return $"{iconFolder}/Field/Field{accessString}_16x.xaml";
				default:
					return $"{iconFolder}/Property/Property{accessString}_16x.xaml";
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

		private static string MapReturnType(TypeSyntax type, bool useLongNames = false)
		{
			var typeAsString = string.Empty;

			if (type is IdentifierNameSyntax) {
				typeAsString = ((IdentifierNameSyntax) type).Identifier.Text;
			} else if (type is PredefinedTypeSyntax) {
				typeAsString = ((PredefinedTypeSyntax) type).Keyword.Text;
			}

			if (useLongNames) return typeAsString;

			var match = new Regex("(.*)<(.*)>").Match(typeAsString);
			if (match.Success)
			{
				return $"{match.Groups[1].Value.Split('.').Last()}<{match.Groups[2].Value.Split('.').Last()}>";
			}
			return typeAsString.Contains(".") ? typeAsString.Split('.').Last() : typeAsString;
		}

        private static string MapMembersToString(SeparatedSyntaxList<EnumMemberDeclarationSyntax> members)
        {
            var memberList = (from EnumMemberDeclarationSyntax member in members select member.Identifier.Text).ToList();
            return $"{string.Join(", ", memberList)}";
        }

        private static string GetText(Document document)
        {
            var doc = (TextDocument)document.Object("TextDocument");
            var p = doc.StartPoint.CreateEditPoint();
            return p.GetText(doc.EndPoint);
        }

        private static SolidColorBrush CreateSolidColorBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
	}
}
