using System;
using System.Collections.Generic;
using System.IO;
using CodeNav.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using CodeNav.Helpers;
using Microsoft.VisualStudio.LanguageServices;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Threading.Tasks;

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

            var root = (CompilationUnitSyntax)_tree.GetRoot(); //

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

            return root.Members.Select(MapMember).ToList(); //
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
                LogHelper.Log("Error during mapping", e, DocumentHelper.GetText(activeDocument), activeDocument.Language);
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
                return null;
            }

            _tree = document.GetSyntaxTreeAsync().Result;

            if (_tree == null)
            {
                return null;
            }

            _semanticModel = document.GetSemanticModelAsync().Result;
            var root = _tree.GetRoot();

            switch (LanguageHelper.GetLanguage(root.Language))
            {
                case LanguageEnum.CSharp:
                    return (root as CompilationUnitSyntax).Members.Select(MapMember).ToList();
                case LanguageEnum.VisualBasic:
                    return (root as VisualBasicSyntax.CompilationUnitSyntax).Members.Select(MapMember).ToList();
                default:
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

            switch (LanguageHelper.GetLanguage(document.Language))
            {
                case LanguageEnum.CSharp:
                    _tree = CSharpSyntaxTree.ParseText(text);

                    var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                    var compilation = CSharpCompilation.Create("CodeNavCompilation", new[] { _tree }, new[] { mscorlib });
                    _semanticModel = compilation.GetSemanticModel(_tree);

                    var root = (CompilationUnitSyntax)_tree.GetRoot();

                    return root.Members.Select(MapMember).ToList();
                case LanguageEnum.VisualBasic:
                    _tree = VisualBasic.VisualBasicSyntaxTree.ParseText(text);

                    var mscorlibVB = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                    var compilationVB = VisualBasic.VisualBasicCompilation.Create("CodeNavCompilation", new[] { _tree }, new[] { mscorlibVB });
                    _semanticModel = compilationVB.GetSemanticModel(_tree);

                    var rootVB = (VisualBasicSyntax.CompilationUnitSyntax)_tree.GetRoot();

                    return rootVB.Members.Select(MapMember).ToList();
                default:
                    return new List<CodeItem>();
            }           
        }

        public static CodeItem MapMember(MemberDeclarationSyntax member)
        {
            if (member == null) return null;

            switch (member.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    return MethodMapper.MapMethod(member as MethodDeclarationSyntax, _control, _semanticModel);
                case SyntaxKind.EnumDeclaration:
                    return EnumMapper.MapEnum(member as EnumDeclarationSyntax, _control, _semanticModel);
                case SyntaxKind.EnumMemberDeclaration:
                    return EnumMapper.MapEnumMember(member as EnumMemberDeclarationSyntax, _control, _semanticModel);
                case SyntaxKind.InterfaceDeclaration:
                    return InterfaceMapper.MapInterface(member as InterfaceDeclarationSyntax, _control, _semanticModel);
                case SyntaxKind.FieldDeclaration:
                    return FieldMapper.MapField(member as FieldDeclarationSyntax, _control, _semanticModel);
                case SyntaxKind.PropertyDeclaration:
                    return PropertyMapper.MapProperty(member as PropertyDeclarationSyntax, _control, _semanticModel);
                case SyntaxKind.StructDeclaration:
                    return StructMapper.MapStruct(member as StructDeclarationSyntax, _control, _semanticModel);
                case SyntaxKind.ClassDeclaration:
                    return ClassMapper.MapClass(member as ClassDeclarationSyntax, _control, _semanticModel, _tree);
                case SyntaxKind.EventFieldDeclaration:
                    return DelegateEventMapper.MapEvent(member as EventFieldDeclarationSyntax, _control, _semanticModel);
                case SyntaxKind.DelegateDeclaration:
                    return DelegateEventMapper.MapDelegate(member as DelegateDeclarationSyntax, _control, _semanticModel);
                case SyntaxKind.NamespaceDeclaration:
                    return NamespaceMapper.MapNamespace(member as NamespaceDeclarationSyntax, _control, _semanticModel);
                case SyntaxKind.ConstructorDeclaration:
                    return MethodMapper.MapConstructor(member as ConstructorDeclarationSyntax, _control, _semanticModel);
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
                case VisualBasic.SyntaxKind.SubBlock:
                    return MethodMapper.MapMethod(member as VisualBasicSyntax.MethodBlockSyntax, _control, _semanticModel);
                case VisualBasic.SyntaxKind.EnumBlock:
                    return EnumMapper.MapEnum(member as VisualBasicSyntax.EnumBlockSyntax, _control, _semanticModel);
                case VisualBasic.SyntaxKind.EnumMemberDeclaration:
                    return EnumMapper.MapEnumMember(member as VisualBasicSyntax.EnumMemberDeclarationSyntax, _control, _semanticModel);
                case VisualBasic.SyntaxKind.InterfaceBlock:
                    return InterfaceMapper.MapInterface(member as VisualBasicSyntax.InterfaceBlockSyntax, _control, _semanticModel);
                case VisualBasic.SyntaxKind.FieldDeclaration:
                    return FieldMapper.MapField(member as VisualBasicSyntax.FieldDeclarationSyntax, _control, _semanticModel);
                case VisualBasic.SyntaxKind.PropertyBlock:
                    return PropertyMapper.MapProperty(member as VisualBasicSyntax.PropertyBlockSyntax, _control, _semanticModel);
                case VisualBasic.SyntaxKind.StructureBlock:
                    return StructMapper.MapStruct(member as VisualBasicSyntax.StructureBlockSyntax, _control, _semanticModel);
                case VisualBasic.SyntaxKind.ClassBlock:
                case VisualBasic.SyntaxKind.ModuleBlock:
                    return ClassMapper.MapClass(member as VisualBasicSyntax.TypeBlockSyntax, _control, _semanticModel, _tree);
                case VisualBasic.SyntaxKind.EventBlock:
                    return DelegateEventMapper.MapEvent(member as VisualBasicSyntax.EventBlockSyntax, _control, _semanticModel);
                case VisualBasic.SyntaxKind.DelegateFunctionStatement:
                    return DelegateEventMapper.MapDelegate(member as VisualBasicSyntax.DelegateStatementSyntax, _control, _semanticModel);
                case VisualBasic.SyntaxKind.NamespaceBlock:
                    return NamespaceMapper.MapNamespace(member as VisualBasicSyntax.NamespaceBlockSyntax, _control, _semanticModel);
                case VisualBasic.SyntaxKind.ConstructorBlock:
                    return MethodMapper.MapConstructor(member as VisualBasicSyntax.ConstructorBlockSyntax, _control, _semanticModel);
                default:
                    return null;
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
    }
}
