using System;
using System.Collections.Generic;
using System.IO;
using CodeNav.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using CodeNav.Helpers;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using CodeNav.Mappers.JavaScript;
using System.Threading.Tasks;
using CodeNav.Shared.Helpers;

namespace CodeNav.Mappers
{
    public static class SyntaxMapper
    {
        /// <summary>
        /// Map a document from filepath, used for unit testing
        /// </summary>
        /// <param name="filePath">filepath of the input document</param>
        /// <returns>List of found code items</returns>
        public static List<CodeItem> MapDocument(string filePath, ICodeViewUserControl control)
        {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("CodeNavCompilation", new[] { tree }, new[] { mscorlib });
            var semanticModel = compilation.GetSemanticModel(tree);

            var root = (CompilationUnitSyntax)tree.GetRoot(); //

            return root.Members.Select(member => MapMember(member, tree, semanticModel, control)).ToList();
        }

        /// <summary>
        /// Map a document from filepath, used for unit testing
        /// </summary>
        /// <param name="filePath">filepath of the input document</param>
        /// <returns>List of found code items</returns>
        public static List<CodeItem> MapDocumentVB(string filePath, ICodeViewUserControl control)
        {
            var tree = VisualBasic.VisualBasicSyntaxTree.ParseText(File.ReadAllText(filePath));

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = VisualBasic.VisualBasicCompilation.Create("CodeNavCompilation", new[] { tree }, new[] { mscorlib });
            var semanticModel = compilation.GetSemanticModel(tree);

            var root = (VisualBasicSyntax.CompilationUnitSyntax)tree.GetRoot();

            return root.Members.Select(member => MapMember(member, tree, semanticModel, control)).ToList(); //
        }

        /// <summary>
        /// Map the active document in the workspace
        /// </summary>
        /// <param name="control">CodeNav control that will show the result</param>
        /// <returns>List of found code items</returns>
        public static async Task<List<CodeItem>> MapDocument(ICodeViewUserControl control, string filePath = "")
        {
            try
            {
                var codeAnalysisDocument = await DocumentHelper.GetCodeAnalysisDocument(filePath);

                if (codeAnalysisDocument != null)
                {
                    return await MapDocument(codeAnalysisDocument, control);
                }

                return await MapDocument(control);
            }
            catch (Exception e)
            {
                var language = await LanguageHelper.GetActiveDocumentLanguage();
                LogHelper.Log("Error during mapping", e, null, language.ToString());
                return null;
            }
        }

        /// <summary>
        /// Map a CodeAnalysis document, used for files in the current solution and workspace
        /// </summary>
        /// <param name="document">a CodeAnalysis document</param>
        /// <returns>List of found code items</returns>
        public static async Task<List<CodeItem>> MapDocument(Document codeAnalysisDocument, ICodeViewUserControl control)
        {
            if (codeAnalysisDocument == null)
            {
                return null;
            }

            if (Path.GetExtension(codeAnalysisDocument.FilePath).Equals(".js"))
            {
                return SyntaxMapperJS.Map(codeAnalysisDocument, control);
            }

            var tree = await codeAnalysisDocument.GetSyntaxTreeAsync();

            if (tree == null)
            {
                return null;
            }

            var semanticModel = await codeAnalysisDocument.GetSemanticModelAsync();
            var root = await tree.GetRootAsync();

            switch (LanguageHelper.GetLanguage(root.Language))
            {
                case LanguageEnum.CSharp:
                    return (root as CompilationUnitSyntax)
                        .Members.Select(member => MapMember(member, tree, semanticModel, control)).ToList();
                case LanguageEnum.VisualBasic:
                    return (root as VisualBasicSyntax.CompilationUnitSyntax)
                        .Members.Select(member => MapMember(member, tree, semanticModel, control)).ToList();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Map the active document without workspace
        /// </summary>
        /// <returns>List of found code items</returns>
        public static async Task<List<CodeItem>> MapDocument(ICodeViewUserControl control)
        {
            var filePath = await DocumentHelper.GetFilePath();

            if (string.IsNullOrEmpty(filePath))
            {
                return new List<CodeItem>();
            }

            var fileExtension = Path.GetExtension(filePath);

            var text = await DocumentHelper.GetText();

            if (string.IsNullOrEmpty(text))
            {
                return new List<CodeItem>();
            }

            if (fileExtension == ".js")
            {
                return SyntaxMapperJS.Map(filePath, control);
            }
            else if (fileExtension == ".cs")
            {
                var tree = CSharpSyntaxTree.ParseText(text);
                var semanticModel = SyntaxHelper.GetCSharpSemanticModel(tree);
                var root = (CompilationUnitSyntax)await tree.GetRootAsync();

                return root.Members.Select(member => MapMember(member, tree, semanticModel, control)).ToList();
            }
            else if (fileExtension == ".vb")
            {
                var tree = VisualBasic.VisualBasicSyntaxTree.ParseText(text);
                var semanticModel = SyntaxHelper.GetVBSemanticModel(tree);
                var root = (VisualBasicSyntax.CompilationUnitSyntax)await tree.GetRootAsync();

                return root.Members.Select(member => MapMember(member, tree, semanticModel, control)).ToList();
            }

            return new List<CodeItem>();
        }

        public static CodeItem MapMember(SyntaxNode member,
            SyntaxTree tree, SemanticModel semanticModel, ICodeViewUserControl control,
            bool mapBaseClass = true)
        {
            if (member == null)
            {
                return null;
            }

            switch (member.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    return MethodMapper.MapMethod(member as MethodDeclarationSyntax, control, semanticModel);
                case SyntaxKind.EnumDeclaration:
                    return EnumMapper.MapEnum(member as EnumDeclarationSyntax, control, semanticModel, tree);
                case SyntaxKind.EnumMemberDeclaration:
                    return EnumMapper.MapEnumMember(member as EnumMemberDeclarationSyntax, control, semanticModel);
                case SyntaxKind.InterfaceDeclaration:
                    return InterfaceMapper.MapInterface(member as InterfaceDeclarationSyntax, control, semanticModel, tree);
                case SyntaxKind.FieldDeclaration:
                    return FieldMapper.MapField(member as FieldDeclarationSyntax, control, semanticModel);
                case SyntaxKind.PropertyDeclaration:
                    return PropertyMapper.MapProperty(member as PropertyDeclarationSyntax, control, semanticModel);
                case SyntaxKind.StructDeclaration:
                    return StructMapper.MapStruct(member as StructDeclarationSyntax, control, semanticModel, tree);
                case SyntaxKind.ClassDeclaration:
                    return ClassMapper.MapClass(member as ClassDeclarationSyntax, control, semanticModel, tree, mapBaseClass);
                case SyntaxKind.EventFieldDeclaration:
                    return DelegateEventMapper.MapEvent(member as EventFieldDeclarationSyntax, control, semanticModel);
                case SyntaxKind.DelegateDeclaration:
                    return DelegateEventMapper.MapDelegate(member as DelegateDeclarationSyntax, control, semanticModel);
                #if VS2022
                case SyntaxKind.FileScopedNamespaceDeclaration:
                case SyntaxKind.NamespaceDeclaration:
                    return NamespaceMapper.MapNamespace(member as BaseNamespaceDeclarationSyntax, control, semanticModel, tree);
                #else
                case SyntaxKind.NamespaceDeclaration:
                return NamespaceMapper.MapNamespace(member as NamespaceDeclarationSyntax, control, semanticModel, tree);
                #endif
                case SyntaxKind.ConstructorDeclaration:
                    return MethodMapper.MapConstructor(member as ConstructorDeclarationSyntax, control, semanticModel);
                case SyntaxKind.IndexerDeclaration:
                    return IndexerMapper.MapIndexer(member as IndexerDeclarationSyntax, control, semanticModel);
                case SyntaxKind.VariableDeclarator:
                    var bla = member as VariableDeclaratorSyntax;
                    return null;
                default:
                    return null;
            }
        }

        public static CodeItem MapMember(VisualBasicSyntax.StatementSyntax member,
            SyntaxTree tree, SemanticModel semanticModel, ICodeViewUserControl control)
        {
            if (member == null) return null;

            switch (member.Kind())
            {
                case VisualBasic.SyntaxKind.FunctionBlock:
                case VisualBasic.SyntaxKind.SubBlock:
                    return MethodMapper.MapMethod(member as VisualBasicSyntax.MethodBlockSyntax, control, semanticModel);
                case VisualBasic.SyntaxKind.SubStatement:
                    return MethodMapper.MapMethod(member as VisualBasicSyntax.MethodStatementSyntax, control, semanticModel);
                case VisualBasic.SyntaxKind.EnumBlock:
                    return EnumMapper.MapEnum(member as VisualBasicSyntax.EnumBlockSyntax, control, semanticModel, tree);
                case VisualBasic.SyntaxKind.EnumMemberDeclaration:
                    return EnumMapper.MapEnumMember(member as VisualBasicSyntax.EnumMemberDeclarationSyntax, control, semanticModel);
                case VisualBasic.SyntaxKind.InterfaceBlock:
                    return InterfaceMapper.MapInterface(member as VisualBasicSyntax.InterfaceBlockSyntax, control, semanticModel, tree);
                case VisualBasic.SyntaxKind.FieldDeclaration:
                    return FieldMapper.MapField(member as VisualBasicSyntax.FieldDeclarationSyntax, control, semanticModel);
                case VisualBasic.SyntaxKind.PropertyBlock:
                    return PropertyMapper.MapProperty(member as VisualBasicSyntax.PropertyBlockSyntax, control, semanticModel);
                case VisualBasic.SyntaxKind.StructureBlock:
                    return StructMapper.MapStruct(member as VisualBasicSyntax.StructureBlockSyntax, control, semanticModel, tree);
                case VisualBasic.SyntaxKind.ClassBlock:
                case VisualBasic.SyntaxKind.ModuleBlock:
                    return ClassMapper.MapClass(member as VisualBasicSyntax.TypeBlockSyntax, control, semanticModel, tree);
                case VisualBasic.SyntaxKind.EventBlock:
                    return DelegateEventMapper.MapEvent(member as VisualBasicSyntax.EventBlockSyntax, control, semanticModel);
                case VisualBasic.SyntaxKind.DelegateFunctionStatement:
                    return DelegateEventMapper.MapDelegate(member as VisualBasicSyntax.DelegateStatementSyntax, control, semanticModel);
                case VisualBasic.SyntaxKind.NamespaceBlock:
                    return NamespaceMapper.MapNamespace(member as VisualBasicSyntax.NamespaceBlockSyntax, control, semanticModel, tree);
                case VisualBasic.SyntaxKind.ConstructorBlock:
                    return MethodMapper.MapConstructor(member as VisualBasicSyntax.ConstructorBlockSyntax, control, semanticModel);
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
