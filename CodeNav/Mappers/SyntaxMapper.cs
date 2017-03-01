using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
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
        private static SemanticModel _semanticModel;

        public static List<CodeItem> MapDocument(string filePath)
        {
            return MapDocument(File.ReadAllText(filePath), null);
        }

        public static List<CodeItem> MapDocument(Document document, CodeViewUserControl control)
        {
            return MapDocument(GetText(document), control);
        }

        private static List<CodeItem> MapDocument(string text, CodeViewUserControl control)
        {
            _control = control;

            _tree = CSharpSyntaxTree.ParseText(text);
            _semanticModel = CreateSemanticModel();
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
                case SyntaxKind.EventFieldDeclaration:
                    return MapEventField(member as EventFieldDeclarationSyntax);
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

        private static CodeItem MapEventField(EventFieldDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = MapBase<CodeItem>(member, member.Declaration.Variables.First().Identifier, member.Modifiers);
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
			if (member == null) return null;

			var item = MapBase<CodeClassItem>(member, member.Identifier, member.Modifiers);
			item.Kind = CodeItemKindEnum.Class;
			item.IconPath = MapIcon(item.Kind, item.Access);
			item.Parameters = MapInheritance(member);
			item.BorderBrush = CreateSolidColorBrush(Colors.DarkGray);
		    item.Tooltip += item.Parameters;

			var regions = MapRegions(member.Span);
			var implementedInterfaces = MapImplementedInterfaces(member);

			foreach (var classMember in member.Members)
			{
				var memberItem = MapMember(classMember);
			    if (memberItem != null && !AddToImplementedInterface(implementedInterfaces, memberItem) 
                    && !AddToRegion(regions, memberItem))
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
                        if (!AddToRegion(regions, interfaceItem))
                        {
                            item.Members.Add(interfaceItem);
                        }
                    }
                }
            }

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

        private static bool AddToImplementedInterface(List<CodeRegionItem> implementedInterfaces, CodeItem item)
        {
            if (item == null) return false;
            foreach (var interfaceItem in implementedInterfaces)
            {
                foreach (var interfaceMember in interfaceItem.Members)
                {
                    if (interfaceMember.ShortId.Equals(item.ShortId))
                    {
                        interfaceItem.Members[interfaceItem.Members.IndexOf(interfaceMember)] = item;

                        // Determing the start/end line of and implemented interface by the start/end line of its members
                        if (item.StartLine <= interfaceItem.StartLine)
                        {
                            interfaceItem.StartLine = item.StartLine;
                        }
                        if (item.StartLine >= interfaceItem.StartLine)
                        {
                            interfaceItem.EndLine = item.StartLine;
                        }

                        return true;
                    }
                }
            }
            return false;
        }

        private static List<CodeRegionItem> MapImplementedInterfaces(ClassDeclarationSyntax member)
        {
            var implementedInterfaces = new List<CodeRegionItem>();

            if (member?.BaseList == null) return implementedInterfaces;

            foreach (var implementedInterface in member.BaseList.Types)
            {
                var item = MapInterface(implementedInterface.ToString(), int.MaxValue);

                var typeSymbol = _semanticModel.GetSymbolInfo(implementedInterface.Type).Symbol as INamedTypeSymbol;
                if (typeSymbol == null || typeSymbol.TypeKind != TypeKind.Interface) continue;

                foreach (var symbolMember in typeSymbol.GetMembers())
                {
                    if (symbolMember.Kind == SymbolKind.Method)
                    {
                        var symbolMethod = symbolMember as IMethodSymbol;

                        if (symbolMethod == null ||  symbolMethod.MethodKind == MethodKind.PropertyGet || 
                            symbolMethod.MethodKind == MethodKind.PropertySet) continue;

                        item.Members.Add(new CodeItem
                        {
                            ShortId = MapShortId(symbolMethod.ToString()),
                            Id = MapId(symbolMember.Name, symbolMethod.Parameters, false, false)
                        });
                    }
                    else
                    {
                        item.Members.Add(new CodeItem
                        {
                            ShortId = symbolMember.Name,
                            Id = symbolMember.Name
                        });
                    }                  
                }

                implementedInterfaces.Add(item);
            }

            return implementedInterfaces;
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

            foreach (var regionDirective in root.DescendantTrivia().Where(i => i.Kind() == SyntaxKind.RegionDirectiveTrivia && i.Span.IntersectsWith(span)))
            {
                regionList.Add(
                    MapRegion("#" + regionDirective.ToString().Replace("#region ", string.Empty), regionDirective.Span)
                );
            }
                
            var index = 0;

            foreach (var endRegionDirective in root.DescendantTrivia().Where(j => j.Kind() == SyntaxKind.EndRegionDirectiveTrivia && j.Span.IntersectsWith(span)))
            {
                regionList[index++].EndLine = GetEndLine(endRegionDirective.Span);
            }

            return regionList;
        }

        private static int GetStartLine(TextSpan span) => 
            _tree.GetLineSpan(span).StartLinePosition.Line + 1;

        private static int GetEndLine(TextSpan span) =>
            _tree.GetLineSpan(span).EndLinePosition.Line + 1;

        private static CodeInterfaceItem MapInterface(string name, int startLine)
        {
            return new CodeInterfaceItem
            {
                Name = name,
                FullName = name,
                Id = name,
                StartLine = startLine,
                Foreground = CreateSolidColorBrush(Colors.Black),
                BorderBrush = CreateSolidColorBrush(Colors.DarkGray),
                FontSize = Settings.Default.Font.SizeInPoints - 2,
                Kind = CodeItemKindEnum.Interface
            };
        }

        private static CodeInterfaceItem MapInterface(InterfaceDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = MapBase<CodeInterfaceItem>(member, member.Identifier, member.Modifiers);
            item.Kind = CodeItemKindEnum.Interface;
            item.BorderBrush = CreateSolidColorBrush(Colors.DarkGray);
            item.IconPath = MapIcon(item.Kind, item.Access);

            foreach (var interfaceMember in member.Members)
            {
                item.Members.Add(MapMember(interfaceMember));
            }

            return item;
        }

        private static CodeRegionItem MapRegion(string name, TextSpan span)
        {
            return new CodeRegionItem
            {
                Name = name,
                FullName = name,
                Id = name,
                Tooltip = name,
                StartLine = GetStartLine(span),
                Foreground = CreateSolidColorBrush(Colors.Black),
                BorderBrush = CreateSolidColorBrush(Colors.DarkGray),
                FontSize = Settings.Default.Font.SizeInPoints - 2,
                Kind = CodeItemKindEnum.Region
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
            element.FullName = GetFullName(source, name);
            element.Id = element.FullName;
            element.ShortId = MapShortId(element.FullName);
            element.Tooltip = name;
            element.StartLine = GetStartLine(source.Span);
            element.EndLine = GetEndLine(source.Span);
            element.Foreground = CreateSolidColorBrush(Colors.Black);
            element.Access = MapAccessToEnum(modifiers);
            element.FontSize = Settings.Default.Font.SizeInPoints;
            element.ParameterFontSize = Settings.Default.Font.SizeInPoints - 1;
            element.FontFamily = new FontFamily(Settings.Default.Font.FontFamily.Name);
            element.FontStyle = FontStyleMapper.Map(Settings.Default.Font.Style);
			element.Control = _control;

            return element;
        }

        private static string GetFullName(MemberDeclarationSyntax source, string name)
        {
            var symbol = _semanticModel.GetDeclaredSymbol(source as SyntaxNode);

            if (symbol != null)
            {
                return symbol.ToString();
            }
            return name;
        }

        private static SemanticModel CreateSemanticModel()
        {
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation", new[] { _tree }, new[] { mscorlib });
            return compilation.GetSemanticModel(_tree);
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
            item.Kind = CodeItemKindEnum.Namespace;
            foreach (var namespaceMember in member.Members)
            {
                item.Members.Add(MapMember(namespaceMember));
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
            item.Id = MapId(item.FullName, member.ParameterList);
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
            return MapId(identifier.Text, parameters);
        }

        public static string MapId(string name, ParameterListSyntax parameters)
        {
            return name + MapParameters(parameters, true, false);
        }

        public static string MapId(string name, ImmutableArray<IParameterSymbol> parameters, bool useLongNames, bool prettyPrint)
        {
            return name + MapParameters(parameters, useLongNames, prettyPrint);
        }

        public static string MapShortId(string fullname)
        {
            var methodMatch = new Regex("(.*)\\((.*)\\)").Match(fullname);
            if (methodMatch.Success)
            {
                return methodMatch.Groups[1].Value.Split('.').Last() +
                       string.Join(string.Empty, methodMatch.Groups[2].Value.Split(',').Select(s => s.Split('.').Last()));
            }

            if (fullname.Contains('.'))
            {
                return fullname.Split('.').Last();
            }

            return fullname;
        }

        /// <summary>
        /// Parse parameters from a method and return a formatted string back
        /// </summary>
        /// <param name="parameters">List of method parameters</param>
        /// <param name="useLongNames">use fullNames for parameter types</param>
        /// <param name="prettyPrint">seperate types with a comma</param>
        /// <returns>string listing all parameter types (eg. (int, string, bool))</returns>
        private static string MapParameters(ParameterListSyntax parameters, bool useLongNames = false, bool prettyPrint = true)
		{
			var paramList = (from ParameterSyntax parameter in parameters.Parameters select MapReturnType(parameter.Type, useLongNames)).ToList();
			return prettyPrint ? $"({string.Join(", ", paramList)})" : string.Join(string.Empty, paramList);
		}

        private static string MapParameters(ImmutableArray<IParameterSymbol> parameters, bool useLongNames = false, bool prettyPrint = true)
        {
            var paramList = (from IParameterSymbol parameter in parameters select MapReturnType(parameter.Type, useLongNames)).ToList();
            return prettyPrint ? $"({string.Join(", ", paramList)})" : string.Join(string.Empty, paramList);
        }

        private static CodePropertyItem MapProperty(PropertyDeclarationSyntax member)
		{
			if (member == null) return null;

			var item = MapBase<CodePropertyItem>(member, member.Identifier, member.Modifiers);
			item.Type = MapReturnType(member.Type);

			if (item.Access == CodeItemAccessEnum.Private) return null;

		    if (member.AccessorList != null)
		    {
                if (member.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.GetAccessorDeclaration))
                {
                    item.Parameters += "get";
                }

                if (member.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.SetAccessorDeclaration))
                {
                    item.Parameters += string.IsNullOrEmpty(item.Parameters) ? "set" : ",set";
                }

		        if (!string.IsNullOrEmpty(item.Parameters))
		        {
		            item.Parameters = $" {{{item.Parameters}}}";
		        }
            }

			item.Parameters = item.Name + item.Parameters;
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

        private static string MapReturnType(ITypeSymbol type, bool useLongNames = false)
        {
            return MapReturnType(type.ToString(), useLongNames);
        }

        private static string MapReturnType(TypeSyntax type, bool useLongNames = false)
        {
            return MapReturnType(type.ToString(), useLongNames);
        }

        private static string MapReturnType(string type, bool useLongNames = false)
		{
            if (useLongNames) return type;

			var match = new Regex("(.*)<(.*)>").Match(type);
			if (match.Success)
			{
				return $"{match.Groups[1].Value.Split('.').Last()}<{match.Groups[2].Value.Split('.').Last()}>";
			}
			return type.Contains(".") ? type.Split('.').Last() : type;
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

        public static void FilterNullItems(List<CodeItem> items)
        {
            items.RemoveAll(item => item == null);
            foreach (var item in items)
            {
                if (item is IMembers)
                {
                    FilterNullItems((item as IMembers).Members);
                }
            }
        }
    }
}
