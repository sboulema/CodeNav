using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using Microsoft.VisualStudio.LanguageServices;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

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
        /// Map a document from filepath, used for unit testing
        /// </summary>
        /// <param name="filePath">filepath of the input document</param>
        /// <returns>List of found code items</returns>
        public static List<CodeItem> MapDocumentVB(string filePath)
        {
            _tree = VisualBasic.VisualBasicSyntaxTree.ParseText(File.ReadAllText(filePath));

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = VisualBasic.VisualBasicCompilation.Create("CodeNavCompilation", new[] { _tree }, new[] { mscorlib });
            _semanticModel = compilation.GetSemanticModel(_tree);

            var root = (VisualBasicSyntax.CompilationUnitSyntax)_tree.GetRoot();

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
                var filePath = DocumentHelper.GetFullName(activeDocument);

                if (string.IsNullOrEmpty(filePath))
                {
                    return MapDocument(activeDocument);
                }

                var id = workspace.CurrentSolution.GetDocumentIdsWithFilePath(filePath).FirstOrDefault();

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
                LogHelper.Log("Error during mapping", e, DocumentHelper.GetText(activeDocument));
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

            if (root.Language.Equals("Visual Basic"))
            {
                return (root as VisualBasicSyntax.CompilationUnitSyntax).Members.Select(MapMember).ToList();
            }
            else if (root.Language.Equals("C Sharp"))
            {
                return (root as CompilationUnitSyntax).Members.Select(MapMember).ToList();
            }
            else
            {
                LogHelper.Log("Error during mapping: root is not CSharp or VisualBasic");
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
            var text = DocumentHelper.GetText(document);

            if (string.IsNullOrEmpty(text)) return new List<CodeItem>();

            _tree = CSharpSyntaxTree.ParseText(text);

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("CodeNavCompilation", new[] { _tree }, new[] { mscorlib });
            _semanticModel = compilation.GetSemanticModel(_tree);

            var root = (CompilationUnitSyntax)_tree.GetRoot();

            return root.Members.Select(MapMember).ToList();
        }

        public static CodeItem MapMember(MemberDeclarationSyntax member)
        {
            if (member == null) return null;

            switch (member.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    return MethodMapper.MapMethod(member as MethodDeclarationSyntax, _control, _semanticModel);
                case SyntaxKind.EnumDeclaration:
                    return MapEnum(member as EnumDeclarationSyntax);
                case SyntaxKind.EnumMemberDeclaration:
                    return MapEnumMember(member as EnumMemberDeclarationSyntax);
                case SyntaxKind.InterfaceDeclaration:
                    return InterfaceMapper.MapInterface(member as InterfaceDeclarationSyntax, _control, _semanticModel);
                case SyntaxKind.FieldDeclaration:
                    return FieldMapper.MapField(member as FieldDeclarationSyntax, _control, _semanticModel);
                case SyntaxKind.PropertyDeclaration:
                    return PropertyMapper.MapProperty(member as PropertyDeclarationSyntax, _control, _semanticModel);
                case SyntaxKind.StructDeclaration:
                    return MapStruct(member as StructDeclarationSyntax);
                case SyntaxKind.ClassDeclaration:
                    return ClassMapper.MapClass(member as ClassDeclarationSyntax, _control, _semanticModel, _tree);
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

        public static CodeItem MapMember(VisualBasicSyntax.StatementSyntax member)
        {
            if (member == null) return null;

            switch (member.Kind())
            {
                case VisualBasic.SyntaxKind.FunctionBlock:
                    return MethodMapper.MapMethod(member as VisualBasicSyntax.MethodBlockSyntax, _control, _semanticModel);
                //case VisualBasic.SyntaxKind.EnumStatement:
                //    return MapEnum(member as VisualBasicSyntax.EnumBlockSyntax);
                //case VisualBasic.SyntaxKind.EnumMemberDeclaration:
                //    return MapEnumMember(member as VisualBasicSyntax.EnumMemberDeclarationSyntax);
                //case VisualBasic.SyntaxKind.InterfaceBlock:
                //    return MapInterface(member as InterfaceDeclarationSyntax);
                case VisualBasic.SyntaxKind.FieldDeclaration:
                    return FieldMapper.MapField(member as VisualBasicSyntax.FieldDeclarationSyntax, _control, _semanticModel);
                case VisualBasic.SyntaxKind.PropertyBlock:
                    return PropertyMapper.MapProperty(member as VisualBasicSyntax.PropertyBlockSyntax, _control, _semanticModel);
                //case VisualBasic.SyntaxKind.StructureBlock:
                //    return MapStruct(member as StructDeclarationSyntax);
                case VisualBasic.SyntaxKind.ClassBlock:
                    return ClassMapper.MapClass(member as VisualBasicSyntax.ClassBlockSyntax, _control, _semanticModel, _tree);
                //case VisualBasic.SyntaxKind.EventBlock:
                //    return MapEventField(member as EventFieldDeclarationSyntax);
                //case VisualBasic.SyntaxKind.DelegateFunctionStatement:
                //    return MapDelegate(member as DelegateDeclarationSyntax);
                //case VisualBasic.SyntaxKind.NamespaceBlock:
                //    return MapNamespace(member as NamespaceDeclarationSyntax);
                //case VisualBasic.SyntaxKind.ConstructorBlock:
                //    return MapConstructor(member as ConstructorDeclarationSyntax);
                default:
                    return null;
            }
        }

        private static CodeItem MapEnumMember(EnumMemberDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeItem>(member, member.Identifier, _control, _semanticModel);
            item.Kind = CodeItemKindEnum.EnumMember;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

            return item;
        }

        private static CodeItem MapDelegate(DelegateDeclarationSyntax member)
        {
            var item = BaseMapper.MapBase<CodeItem>(member, member.Identifier, member.Modifiers, _control, _semanticModel);
            item.Kind = CodeItemKindEnum.Delegate;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            return item;
        }

        private static CodeItem MapEventField(EventFieldDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeItem>(member, member.Declaration.Variables.First().Identifier, member.Modifiers, _control, _semanticModel);
            item.Kind = CodeItemKindEnum.Event;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            return item;
        }

        private static CodeClassItem MapStruct(StructDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeClassItem>(member, member.Identifier, member.Modifiers, _control, _semanticModel);
            item.Kind = CodeItemKindEnum.Struct;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
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

            var item = BaseMapper.MapBase<CodeClassItem>(member, member.Identifier, member.Modifiers, _control, _semanticModel);
            item.Kind = CodeItemKindEnum.Enum;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.Parameters = MapMembersToString(member.Members);
            item.BorderBrush = ColorHelper.CreateSolidColorBrush(Colors.DarkGray);          

            foreach (var enumMember in member.Members)
            {
                item.Members.Add(MapMember(enumMember));
            }

            return item;
        }

        #region Helpers

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

        private static string MapMembersToString(SeparatedSyntaxList<EnumMemberDeclarationSyntax> members)
        {
            var memberList = (from EnumMemberDeclarationSyntax member in members select member.Identifier.Text).ToList();
            return $"{string.Join(", ", memberList)}";
        }

        #endregion

        private static CodeNamespaceItem MapNamespace(NamespaceDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeNamespaceItem>(member, member.Name, _control, _semanticModel);
            item.Kind = CodeItemKindEnum.Namespace;
            foreach (var namespaceMember in member.Members)
            {
                item.Members.Add(MapMember(namespaceMember));
            }
            return item;
        }

        private static CodeItem MapConstructor(ConstructorDeclarationSyntax member)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeFunctionItem>(member, member.Identifier, member.Modifiers, _control, _semanticModel);
            item.Parameters = ParameterMapper.MapParameters(member.ParameterList);
            item.Tooltip = TooltipMapper.Map(item.Access, item.Type, item.Name, member.ParameterList);
            item.Id = IdMapper.MapId(member.Identifier, member.ParameterList);
            item.Kind = CodeItemKindEnum.Constructor;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.OverlayMoniker = KnownMonikers.Add;

            return item;
        }
    }
}
