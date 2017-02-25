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
			//TODO: Implement missing types
            switch (member.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    return MapMethod(member as MethodDeclarationSyntax);
                //case vsCMElement.vsCMElementEnum:
                //    return MapEnum(element);
                case SyntaxKind.InterfaceDeclaration:
                    return MapInterface(member as InterfaceDeclarationSyntax);
                //case vsCMElement.vsCMElementVariable:
                //    return MapVariable(element);
                case SyntaxKind.PropertyDeclaration:
                    return MapProperty(member as PropertyDeclarationSyntax);
                //case vsCMElement.vsCMElementStruct:
                //    return MapStruct(element);
                case SyntaxKind.ClassDeclaration:
                    return MapClass(member as ClassDeclarationSyntax);
                //case vsCMElement.vsCMElementEvent:
                //    return MapEvent(element);
                //case vsCMElement.vsCMElementDelegate:
                //    return MapDelegate(element);
                case SyntaxKind.NamespaceDeclaration:
                    return MapNamespace(member as NamespaceDeclarationSyntax);
                default:
                    return null;
            }
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

			//var classRegions = MapRegions(element.StartPoint, element.EndPoint);
			//var implementedInterfaces = MapImplementedInterfaces(element);

			foreach (var classMember in member.Members)
			{
				var memberItem = MapMember(classMember);
				//if (memberItem != null && !AddToImplementedInterface(implementedInterfaces, memberItem)
				//	&& !AddToRegion(classRegions, memberItem))
				//{
					item.Members.Add(memberItem);
				//}
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
			//if (classRegions.Any())
			//{
			//	foreach (var region in classRegions)
			//	{
			//		if (region.Members.Any())
			//		{
			//			item.Members.Add(region);
			//		}
			//	}
			//}

			return item;
		}

		private static string MapInheritance(ClassDeclarationSyntax member)
		{
			if (member == null) return null;

			var basesList = (from BaseTypeSyntax bases in member.BaseList.Types select bases.Type).ToList();
			//TODO: Implement implementedinterfaces
			//var interfaceList = (from CodeElement interfaces in codeClass.ImplementedInterfaces select interfaces.Name).ToList();
			//basesList.AddRange(interfaceList);

			return $" : {string.Join(", ", basesList)}";
		}

		private static T MapBase<T>(MemberDeclarationSyntax source, SyntaxToken identifier, SyntaxTokenList modifiers) where T : CodeItem
		{
			return MapBase<T>(source, identifier.Text, modifiers);
		}

		private static T MapBase<T>(MemberDeclarationSyntax source, NameSyntax name) where T : CodeItem
		{
			return MapBase<T>(source, name.ToString(), new SyntaxTokenList());
		}

		private static T MapBase<T>(MemberDeclarationSyntax source, string name, SyntaxTokenList modifiers) where T : CodeItem
        {
            var element = Activator.CreateInstance<T>();

            element.Name = name.ToString();
            element.FullName = name.ToString();
            element.Id = name.ToString();
            element.Tooltip = name.ToString();
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
			else if (modifiers.Any(m => m.Kind() == SyntaxKind.PublicKeyword))
			{
				return CodeItemAccessEnum.Public;
			}
			else if (modifiers.Any(m => m.Kind() == SyntaxKind.PrivateKeyword))
			{
				return CodeItemAccessEnum.Private;
			}
			else if (modifiers.Any(m => m.Kind() == SyntaxKind.ProtectedKeyword))
			{
				return CodeItemAccessEnum.Protected;
			}
			else if (modifiers.Any(m => m.Kind() == SyntaxKind.InternalKeyword))
			{
				return CodeItemAccessEnum.Internal;
			}
			else
			{
				return CodeItemAccessEnum.Unknown;
			}
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
			//TODO: implement uniqueIdentifier
			//item.Id = MapId(element);
			item.Kind = member.Kind() == SyntaxKind.ConstructorDeclaration
				? CodeItemKindEnum.Constructor
				: CodeItemKindEnum.Method;
			item.IconPath = MapIcon(item.Kind, item.Access);

			return item;
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

		private static string MapIcon(CodeItemKindEnum kind, CodeItemAccessEnum access, bool isEnumMember = false)
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
				case CodeItemKindEnum.EnumItem:
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
					return isEnumMember ? $"{iconFolder}/Enum/EnumItem{accessString}_16x.xaml" : $"{iconFolder}/Constant/Constant{accessString}_16x.xaml";
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
				typeAsString = (type as IdentifierNameSyntax).Identifier.Text;
			} else if (type is PredefinedTypeSyntax) {
				typeAsString = (type as PredefinedTypeSyntax).Keyword.Text;
			}

			if (useLongNames) return typeAsString;

			var match = new Regex("(.*)<(.*)>").Match(typeAsString);
			if (match.Success)
			{
				return $"{match.Groups[1].Value.Split('.').Last()}<{match.Groups[2].Value.Split('.').Last()}>";
			}
			return typeAsString.Contains(".") ? typeAsString.Split('.').Last() : typeAsString;
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
