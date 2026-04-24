using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.ProjectSystem.Query;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public class DocumentMapper : IDocumentMapper
{
    /// <summary>
    /// Map text document to list of code items.
    /// </summary>
    /// <param name="text">Text of the code document</param>
    /// <param name="excludeFilePath">File path of the document snaphot, used to exclude the saved version of the text</param>
    /// <param name="codeDocumentViewModel">Current view model connected to the CodeNav tool window</param>
    /// <param name="extensibility">Visual Studio extensibility used to retrieve all solution files for compilation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of code items</returns>
    public static async Task<List<CodeItem>> MapDocument(
        string text,
        string? excludeFilePath,
        CodeDocumentViewModel codeDocumentViewModel,
        VisualStudioExtensibility extensibility,
        CancellationToken cancellationToken)
    {
        var projects = await extensibility
            .Workspaces()
            .QueryProjectsAsync(project
                => project.With(project
                    => project.Files
                        .Where(file => file.Extension == ".vb")
                        .Where(file => file.Path != excludeFilePath)
                        .With(file => file.Path)), cancellationToken);

        var filePaths = projects
            .SelectMany(project => project.Files)
            .Select(file => file.Path);

        return await MapDocument(text, codeDocumentViewModel, filePaths, cancellationToken);
    }

    /// <summary>
    /// Map text document to list of code items.
    /// </summary>
    /// <param name="text">Text of the code document</param>
    /// <param name="codeDocumentViewModel">Current view model connected to the CodeNav tool window</param>
    /// <param name="filePaths">File paths to include in the compilation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of code items</returns>
    public static async Task<List<CodeItem>> MapDocument(
        string text,
        CodeDocumentViewModel codeDocumentViewModel,
        IEnumerable<string> filePaths,
        CancellationToken cancellationToken)
    {
        var syntaxTree = VisualBasicSyntaxTree.ParseText(text, cancellationToken: cancellationToken);
        var semanticModel = await GetVisualBasicSemanticModel(filePaths, syntaxTree, cancellationToken);
        var root = (CompilationUnitSyntax)await syntaxTree.GetRootAsync(cancellationToken);

        if (semanticModel == null)
        {
            return [];
        }

        return [.. root.Members
            .Where(member => member != null)
            .Select(member => MapMember(member, syntaxTree, semanticModel, codeDocumentViewModel))
            .Where(codeItem => codeItem != null)
            .Cast<CodeItem>()];
    }

    public static async Task<SemanticModel?> GetVisualBasicSemanticModel(
        IEnumerable<string> filePaths,
        SyntaxTree syntaxTree,
        CancellationToken cancellationToken)
    {
        var syntaxTrees = await Task.WhenAll(filePaths
            .Select(async filePath =>
            {
                var fileText = await File.ReadAllTextAsync(filePath, cancellationToken);
                var syntaxTree = VisualBasicSyntaxTree.ParseText(fileText, cancellationToken: cancellationToken).WithFilePath(filePath);
                return syntaxTree;
            }));

        var compilation = VisualBasicCompilation
            .Create("CodeNavCompilation")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(syntaxTrees)
            .AddSyntaxTrees(syntaxTree);

        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        return semanticModel;
    }

    public static CodeItem? MapMember(
        SyntaxNode member, SyntaxTree tree, SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel,
        bool mapBaseClass = true)
        => member.Kind() switch
            {
                SyntaxKind.ClassBlock when member is TypeBlockSyntax classSyntax
                    => ClassMapper.MapClass(classSyntax, semanticModel, tree, codeDocumentViewModel, mapBaseClass),
                SyntaxKind.ConstructorBlock when member is ConstructorBlockSyntax constructorSyntax
                    => MethodMapper.MapConstructor(constructorSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.DelegateFunctionStatement when member is DelegateStatementSyntax delegateSyntax
                    => DelegateEventMapper.MapDelegate(delegateSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.EnumBlock when member is EnumBlockSyntax enumSyntax
                    => EnumMapper.MapEnum(enumSyntax, semanticModel, tree, codeDocumentViewModel),
                SyntaxKind.EnumMemberDeclaration when member is EnumMemberDeclarationSyntax enumMemberSyntax
                    => EnumMapper.MapEnumMember(enumMemberSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.EventBlock when member is EventBlockSyntax eventFieldSyntax
                    => DelegateEventMapper.MapEvent(eventFieldSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.FieldDeclaration when member is FieldDeclarationSyntax fieldSyntax
                    => FieldMapper.MapField(fieldSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.InterfaceBlock when member is InterfaceBlockSyntax interfaceSyntax
                    => InterfaceMapper.MapInterface(interfaceSyntax, semanticModel, tree, codeDocumentViewModel),
                SyntaxKind.FunctionBlock when member is MethodBlockSyntax memberSyntax
                    => MethodMapper.MapMethod(memberSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.SubBlock when member is MethodBlockSyntax memberSyntax
                    => MethodMapper.MapMethod(memberSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.SubStatement when member is MethodStatementSyntax memberSyntax
                    => MethodMapper.MapMethod(memberSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.NamespaceBlock when member is NamespaceBlockSyntax namespaceSyntax
                    => NamespaceMapper.MapNamespace(namespaceSyntax, semanticModel, tree, codeDocumentViewModel),
                SyntaxKind.PropertyBlock when member is PropertyBlockSyntax propertySyntax
                    => PropertyMapper.MapProperty(propertySyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.StructureBlock when member is StructureBlockSyntax structSyntax
                    => StructMapper.MapStruct(structSyntax, semanticModel, tree, codeDocumentViewModel),
                _ => null,
            };

    public bool CanMapDocument(string filePath) 
        => filePath.EndsWith(".vb", StringComparison.OrdinalIgnoreCase);
}