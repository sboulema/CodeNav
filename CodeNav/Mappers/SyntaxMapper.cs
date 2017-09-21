using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using CodeNav.Models;
using CodeNav.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using CodeNav.Helpers;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.CodeAnalysis.Text;

namespace CodeNav.Mappers
{
    public static class SyntaxMapper
    {
        private static CodeViewUserControl _control;
        private static SyntaxTree _tree;
        private static SemanticModel _semanticModel;

        /// <summary>
        /// Map a document from filepath, used for unit testing
        /// </summary>
        /// <param name="filePath">filepath of the input document</param>
        /// <returns>List of found code items</returns>
        public static List<CodeItem> MapDocument(string filePath)
        {
            _tree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("CodeNavCompilation", new[] { _tree }, new[] { mscorlib });
            _semanticModel = compilation.GetSemanticModel(_tree);

            var root = (CompilationUnitSyntax)_tree.GetRoot();

            return root.Members.Select(MapMember).ToList();
        }

        /// <summary>
        /// Map the active document in the workspace
        /// </summary>
        /// <param name="activeDocument">active EnvDTE.document</param>
        /// <param name="control">CodeNav control that will show the result</param>
        /// <param name="workspace">Current Visual Studio workspace</param>
        /// <returns>List of found code items</returns>
        public static List<CodeItem> MapDocument(EnvDTE.Document activeDocument, CodeViewUserControl control, 
            VisualStudioWorkspace workspace)
        {
            _control = control;

            if (workspace == null)
            {
                LogHelper.Log("Error during mapping: Workspace is null");
                return null;
            }

            try
            {
                var id = workspace.CurrentSolution.GetDocumentIdsWithFilePath(activeDocument.FullName).FirstOrDefault();

                // We can not find the requested document in the current solution,
                // Try and map it in a different way
                if (id == null)
                {
                    return MapDocument(activeDocument);
                }

                var document = workspace.CurrentSolution.GetDocument(id);

                return MapDocument(document);
            }
            catch (Exception e)
            {
                LogHelper.Log($"Error during mapping: {e}");
                LogHelper.Log("Error during mapping", e);
                return null;
            }        
        }

        /// <summary>
        /// Map a CodeAnalysis document, used for files in the current solution and workspace
        /// </summary>
        /// <param name="document">a CodeAnalysis document</param>
        /// <returns>List of found code items</returns>
        public static List<CodeItem> MapDocument(Document document)
        {
            if (document == null)
            {
                LogHelper.Log("Error during mapping: Document is null");
                return null;
            }

            _tree = document.GetSyntaxTreeAsync().Result;

            if (_tree == null)
            {
                LogHelper.Log("Error during mapping: Tree is null");
                return null;
            }

            _semanticModel = document.GetSemanticModelAsync().Result;
            var root = _tree.GetRoot();

            if (root is CompilationUnitSyntax)
            {
                return (root as CompilationUnitSyntax).Members.Select(MapMember).ToList();
            }
            else
            {
                LogHelper.Log("Error during mapping: root is not CSharp");
                return null;
            }        
        }

        /// <summary>
        /// Map an EnvDTE.Document, used for files outside of the current solution eg. [from metadata]
        /// </summary>
        /// <param name="document">An EnvDTE.Document</param>
        /// <returns>List of found code items</returns>
        public static List<CodeItem> MapDocument(EnvDTE.Document document)
        {
            var doc = (EnvDTE.TextDocument)document.Object("TextDocument");
            EnvDTE.EditPoint startPoint = null;

            try
            {
                startPoint = doc?.StartPoint?.CreateEditPoint();
            }
            catch (Exception)
            {
                LogHelper.Log("Error during mapping: Unable to find TextDocument StartPoint");
            }

            if (startPoint == null)
            {
                LogHelper.Log("Error during mapping: Unable to find TextDocument StartPoint");
                return null;
            };

            var text = startPoint.GetText(doc.EndPoint);

            _tree = CSharpSyntaxTree.ParseText(text);

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("CodeNavCompilation", new[] { _tree }, new[] { mscorlib });
            _semanticModel = compilation.GetSemanticModel(_tree);

            var root = (CompilationUnitSyntax)_tree.GetRoot();

            return root.Members.Select(MapMember).ToList();
        }

        private static CodeItem MapMember(MemberDeclarationSyntax member)
        {
            if (member == null) return null;

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
            item.Moniker = MapMoniker(item.Kind, item.Access);

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
            item.Moniker = MapMoniker(item.Kind, item.Access);

            return item;
        }

        private static CodeItem MapDelegate(DelegateDeclarationSyntax member)
        {
            var item = MapBase<CodeItem>(member, member.Identifier, member.Modifiers);
            if (item.Access == CodeItemAccessEnum.Private) return null;
            item.Kind = CodeItemKindEnum.Delegate;
            item.Moniker = MapMoniker(item.Kind, item.Access);
            return item;
        }

        private static CodeItem MapEventField(EventFieldDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = MapBase<CodeItem>(member, member.Declaration.Variables.First().Identifier, member.Modifiers);
            if (item.Access == CodeItemAccessEnum.Private) return null;
            item.Kind = CodeItemKindEnum.Event;
            item.Moniker = MapMoniker(item.Kind, item.Access);
            return item;
        }

        private static CodeClassItem MapStruct(StructDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = MapBase<CodeClassItem>(member, member.Identifier, member.Modifiers);
            item.Kind = CodeItemKindEnum.Struct;
            item.Moniker = MapMoniker(item.Kind, item.Access);
            item.BorderBrush = ColorHelper.CreateSolidColorBrush(Colors.DarkGray);
            
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
            item.Moniker = MapMoniker(item.Kind, item.Access);
            item.Parameters = MapMembersToString(member.Members);
            item.BorderBrush = ColorHelper.CreateSolidColorBrush(Colors.DarkGray);          

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
            item.Moniker = MapMoniker(item.Kind, item.Access);
            item.Parameters = MapInheritance(member);
			item.BorderBrush = ColorHelper.CreateSolidColorBrush(Colors.DarkGray);
		    item.Tooltip = TooltipMapper.Map(item.Access, string.Empty, item.Name, item.Parameters);

			var regions = RegionMapper.MapRegions(_tree, member.Span);
			var implementedInterfaces = MapImplementedInterfaces(member);

			foreach (var classMember in member.Members)
			{
				var memberItem = MapMember(classMember);
			    if (memberItem != null && !IsPartOfImplementedInterface(implementedInterfaces, memberItem) 
                    && !RegionMapper.AddToRegion(regions, memberItem))
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
                        if (!RegionMapper.AddToRegion(regions, interfaceItem))
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

        #region Interfaces

        private static bool IsPartOfImplementedInterface(IEnumerable<CodeImplementedInterfaceItem> implementedInterfaces, CodeItem item)
        {
            return item != null && implementedInterfaces.SelectMany(i => i.Members.Select(m => m.Id)).Contains(item.Id);
        }

        private static List<CodeImplementedInterfaceItem> MapImplementedInterfaces(ClassDeclarationSyntax member)
        {
            var implementedInterfaces = new List<CodeImplementedInterfaceItem>();

            INamedTypeSymbol classSymbol = null;
            try
            {
                classSymbol = _semanticModel.GetDeclaredSymbol(member);
            }
            catch (Exception e)
            {
                LogHelper.Log($"Error during mapping: MapImplementedInterface: {e.Message}");
                return implementedInterfaces;
            }

            var interfacesList = new List<INamedTypeSymbol>();
            GetInterfaces(interfacesList, classSymbol.Interfaces);

            foreach (var implementedInterface in interfacesList.Distinct())
            {
                implementedInterfaces.Add(MapImplementedInterface(implementedInterface.Name, implementedInterface.GetMembers(), classSymbol));
            }

            return implementedInterfaces;
        }

        /// <summary>
        /// Recursively get the interfaces implemented by the class.
        /// This ignores interfaces implemented by any base class, contrary to the .Allinterfaces behaviour
        /// </summary>
        /// <param name="interfacesFound">List of all interfaces found</param>
        /// <param name="source">Implemented interfaces</param>
        private static void GetInterfaces(List<INamedTypeSymbol> interfacesFound, ImmutableArray<INamedTypeSymbol> source)
        {
            interfacesFound.AddRange(source);
            foreach (var interfaceItem in source)
            {
                GetInterfaces(interfacesFound, interfaceItem.Interfaces);
            }
        }

        private static CodeImplementedInterfaceItem MapImplementedInterface(string name, 
            ImmutableArray<ISymbol> members, INamedTypeSymbol implementingClass)
        {
            var item = new CodeImplementedInterfaceItem
            {
                Name = name,
                FullName = name,
                Id = name,
                Foreground = ColorHelper.CreateSolidColorBrush(Colors.Black),
                BorderBrush = ColorHelper.CreateSolidColorBrush(Colors.DarkGray),
                FontSize = Settings.Default.Font.SizeInPoints - 2,
                Kind = CodeItemKindEnum.ImplementedInterface,
                IsExpanded = true
            };

            foreach (var member in members)
            {
                var implementation = implementingClass.FindImplementationForInterfaceMember(member);
                if (implementation == null || !implementation.DeclaringSyntaxReferences.Any()) continue;
                var reference = implementation.DeclaringSyntaxReferences.First();
                var declarationSyntax = reference.GetSyntax();

                var interfaceMember = MapMember(declarationSyntax as MemberDeclarationSyntax);
                if (interfaceMember == null) continue;

                interfaceMember.OverlayMoniker = KnownMonikers.InterfacePublic;
                item.Members.Add(interfaceMember);
            }

            if (item.Members.Any())
            {
                item.StartLine = item.Members.Min(i => i.StartLine);
                item.EndLine = item.Members.Max(i => i.EndLine);
            }

            return item;
        }

        private static CodeInterfaceItem MapInterface(InterfaceDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = MapBase<CodeInterfaceItem>(member, member.Identifier, member.Modifiers);
            item.Kind = CodeItemKindEnum.Interface;
            item.BorderBrush = ColorHelper.CreateSolidColorBrush(Colors.DarkGray);
            item.Moniker = MapMoniker(item.Kind, item.Access);

            foreach (var interfaceMember in member.Members)
            {
                item.Members.Add(MapMember(interfaceMember));
            }

            return item;
        }

        #endregion

        private static string MapInheritance(ClassDeclarationSyntax member)
		{
			if (member?.BaseList == null) return string.Empty;

		    var inheritanceList = (from BaseTypeSyntax bases in member.BaseList.Types select bases.Type.ToString()).ToList();

		    return !inheritanceList.Any() ? string.Empty : $" : {string.Join(", ", inheritanceList)}";
		}

		private static T MapBase<T>(SyntaxNode source, SyntaxToken identifier, SyntaxTokenList modifiers) where T : CodeItem
		{
			return MapBase<T>(source, identifier.Text, modifiers);
		}

		public static T MapBase<T>(SyntaxNode source, NameSyntax name) where T : CodeItem
		{
			return MapBase<T>(source, name.ToString(), new SyntaxTokenList());
		}

        public static T MapBase<T>(SyntaxNode source, string name) where T : CodeItem
        {
            return MapBase<T>(source, name, new SyntaxTokenList());
        }

        public static T MapBase<T>(SyntaxNode source, SyntaxToken identifier) where T : CodeItem
        {
            return MapBase<T>(source, identifier.Text, new SyntaxTokenList());
        }

        private static T MapBase<T>(SyntaxNode source, string name, SyntaxTokenList modifiers) where T : CodeItem
        {
            var element = Activator.CreateInstance<T>();

            element.Name = name;
            element.FullName = GetFullName(source, name);
            element.Id = element.FullName;
            element.Tooltip = name;
            element.StartLine = GetStartLine(source);
            element.StartLinePosition = GetStartLinePosition(source);
            element.EndLine = GetEndLine(source);
            element.Foreground = ColorHelper.CreateSolidColorBrush(Colors.Black);
            element.Access = MapAccess(modifiers);
            element.FontSize = Settings.Default.Font.SizeInPoints;
            element.ParameterFontSize = Settings.Default.Font.SizeInPoints - 1;
            element.FontFamily = new FontFamily(Settings.Default.Font.FontFamily.Name);
            element.FontStyle = FontStyleMapper.Map(Settings.Default.Font.Style);
			element.Control = _control;

            return element;
        }

        #region Helpers

        private static string GetEnumDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        private static bool IsConstant(SyntaxTokenList modifiers)
        {
            return modifiers.Any(m => m.Kind() == SyntaxKind.ConstKeyword);
        }

        private static string GetFullName(SyntaxNode source, string name)
        {
            try
            {
                var symbol = _semanticModel.GetDeclaredSymbol(source);
                return symbol?.ToString() ?? name;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static void FilterNullItems(List<CodeItem> items)
        {
            if (items == null) return;
            items.RemoveAll(item => item == null);
            foreach (var item in items)
            {
                if (item is IMembers)
                {
                    FilterNullItems((item as IMembers).Members);
                }
            }
        }

        private static LinePosition GetStartLinePosition(SyntaxNode source) =>
            source.SyntaxTree.GetLineSpan(source.Span).StartLinePosition;

        private static int GetStartLine(SyntaxNode source) =>
            source.SyntaxTree.GetLineSpan(source.Span).StartLinePosition.Line + 1;

        private static int GetEndLine(SyntaxNode source) =>
            source.SyntaxTree.GetLineSpan(source.Span).EndLinePosition.Line + 1;

        private static string MapMembersToString(SeparatedSyntaxList<EnumMemberDeclarationSyntax> members)
        {
            var memberList = (from EnumMemberDeclarationSyntax member in members select member.Identifier.Text).ToList();
            return $"{string.Join(", ", memberList)}";
        }

        private static CodeItemAccessEnum MapAccess(SyntaxTokenList modifiers)
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

            return CodeItemAccessEnum.Public;
        }

        #endregion

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

            CodeItem item; 

            var statements = member.Body?.Statements.Select(StatementMapper.MapStatement).ToList();
            FilterNullItems(statements);

            if (Settings.Default.ShowSwitch && statements != null && statements.Any())
            {
                item = MapBase<CodeClassItem>(member, member.Identifier, member.Modifiers);
                ((CodeClassItem)item).Members.AddRange(statements);
                ((CodeClassItem)item).BorderBrush = ColorHelper.CreateSolidColorBrush(Colors.DarkGray);
            }
            else
            {
                item = MapBase<CodeFunctionItem>(member, member.Identifier, member.Modifiers);
                ((CodeFunctionItem)item).Type = TypeMapper.Map(member.ReturnType);
                ((CodeFunctionItem)item).Parameters = MapParameters(member.ParameterList);
                item.Tooltip = TooltipMapper.Map(item.Access, ((CodeFunctionItem) item).Type, item.Name, member.ParameterList);
            }

            item.Id = MapId(item.FullName, member.ParameterList);
            item.Kind = CodeItemKindEnum.Method;
            item.Moniker = MapMoniker(item.Kind, item.Access);

            return item;
        }

        private static CodeItem MapConstructor(ConstructorDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = MapBase<CodeFunctionItem>(member, member.Identifier, member.Modifiers);
            item.Parameters = MapParameters(member.ParameterList);
            item.Tooltip = TooltipMapper.Map(item.Access, item.Type, item.Name, member.ParameterList);
            item.Id = MapId(member.Identifier, member.ParameterList);
            item.Kind = CodeItemKindEnum.Constructor;
            item.Moniker = MapMoniker(item.Kind, item.Access);
            item.OverlayMoniker = KnownMonikers.Add;

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

        /// <summary>
        /// Parse parameters from a method and return a formatted string back
        /// </summary>
        /// <param name="parameters">List of method parameters</param>
        /// <param name="useLongNames">use fullNames for parameter types</param>
        /// <param name="prettyPrint">seperate types with a comma</param>
        /// <returns>string listing all parameter types (eg. (int, string, bool))</returns>
        public static string MapParameters(ParameterListSyntax parameters, bool useLongNames = false, bool prettyPrint = true)
        {
            if (parameters == null) return string.Empty;
			var paramList = (from ParameterSyntax parameter in parameters.Parameters select TypeMapper.Map(parameter.Type, useLongNames)).ToList();
			return prettyPrint ? $"({string.Join(", ", paramList)})" : string.Join(string.Empty, paramList);
		}

        private static string MapParameters(ImmutableArray<IParameterSymbol> parameters, bool useLongNames = false, bool prettyPrint = true)
        {
            var paramList = (from IParameterSymbol parameter in parameters select TypeMapper.Map(parameter.Type, useLongNames)).ToList();
            return prettyPrint ? $"({string.Join(", ", paramList)})" : string.Join(string.Empty, paramList);
        }

        private static CodePropertyItem MapProperty(PropertyDeclarationSyntax member)
		{
			if (member == null) return null;

			var item = MapBase<CodePropertyItem>(member, member.Identifier, member.Modifiers);
			item.Type = TypeMapper.Map(member.Type);

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
		    item.Tooltip = TooltipMapper.Map(item.Access, item.Type, item.Name, item.Parameters);
			item.Kind = CodeItemKindEnum.Property;
            item.Moniker = MapMoniker(item.Kind, item.Access);
            return item;
		}

        public static ImageMoniker MapMoniker(CodeItemKindEnum kind, CodeItemAccessEnum access)
        {
            string monikerString;
            var accessString = GetEnumDescription(access);

            switch (kind)
            {
                case CodeItemKindEnum.Class:
                    monikerString = $"Class{accessString}";
                    break;
                case CodeItemKindEnum.Constant:
                    monikerString = $"Constant{accessString}";
                    break;
                case CodeItemKindEnum.Delegate:
                    monikerString = $"Delegate{accessString}";
                    break;
                case CodeItemKindEnum.Enum:
                    monikerString = $"Enumeration{accessString}";
                    break;
                case CodeItemKindEnum.EnumMember:
                    monikerString = $"EnumerationItem{accessString}";
                    break;
                case CodeItemKindEnum.Event:
                    monikerString = $"Event{accessString}";
                    break;
                case CodeItemKindEnum.Interface:
                    monikerString = $"Interface{accessString}";
                    break;
                case CodeItemKindEnum.Constructor:
                case CodeItemKindEnum.Method:
                    monikerString = $"Method{accessString}";
                    break;
                case CodeItemKindEnum.Property:
                    monikerString = $"Property{accessString}";
                    break;
                case CodeItemKindEnum.Struct:
                    monikerString = $"Structure{accessString}";
                    break;
                case CodeItemKindEnum.Variable:
                    monikerString = $"Field{accessString}";
                    break;
                case CodeItemKindEnum.Switch:
                    monikerString = "FlowSwitch";
                    break;
                case CodeItemKindEnum.SwitchSection:
                    monikerString = "FlowDecision";
                    break;
                default:
                    monikerString = $"Property{accessString}";
                    break;
            }

            var monikers = typeof(KnownMonikers).GetProperties();

            var imageMoniker = monikers.FirstOrDefault(m => monikerString.Equals(m.Name))?.GetValue(null, null);
            if (imageMoniker != null)
            {
                return (ImageMoniker)imageMoniker;
            }

            return KnownMonikers.QuestionMark;
        }
    }
}
