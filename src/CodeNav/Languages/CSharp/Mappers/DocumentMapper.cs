using CodeNav.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.ProjectSystem.Query;
using System.IO;

namespace CodeNav.Languages.CSharp.Mappers;

public class DocumentMapper(
    ClassMapper classMapper,
    DelegateEventMapper delegateEventMapper,
    EnumMapper enumMapper,
    FieldMapper fieldMapper,
    IndexerMapper indexerMapper,
    InterfaceMapper interfaceMapper,
    MethodMapper methodMapper,
    NamespaceMapper namespaceMapper,
    PropertyMapper propertyMapper,
    RecordMapper recordMapper,
    StatementMapper statementMapper,
    StructMapper structMapper)
{
    /// <summary>
    /// Map text document to list of code items.
    /// </summary>
    /// <param name="documentSnapshot">Document snapshot with latest version of text</param>
    /// <param name="excludeFilePath">File path of the document snaphot, used to exclude the saved version of the text</param>
    /// <param name="codeDocumentViewModel">Current view model connected to the CodeNav tool window</param>
    /// <param name="extensibility">Visual Studio extensibility used to retrieve all solution files for compilation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of code items</returns>
    public async Task<List<CodeItem>> MapDocument(
        ITextDocumentSnapshot documentSnapshot,
        string? excludeFilePath,
        CodeDocumentViewModel codeDocumentViewModel,
        VisualStudioExtensibility extensibility,
        CancellationToken cancellationToken)
        => await MapDocument(documentSnapshot.Text.CopyToString(), excludeFilePath, codeDocumentViewModel, extensibility, cancellationToken);

    /// <summary>
    /// Map text document to list of code items.
    /// </summary>
    /// <param name="text">Text of the code document</param>
    /// <param name="excludeFilePath">File path of the document snaphot, used to exclude the saved version of the text</param>
    /// <param name="codeDocumentViewModel">Current view model connected to the CodeNav tool window</param>
    /// <param name="extensibility">Visual Studio extensibility used to retrieve all solution files for compilation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of code items</returns>
    public async Task<List<CodeItem>> MapDocument(
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
                        .Where(file => file.Extension == ".cs")
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
    public async Task<List<CodeItem>> MapDocument(
        string text,
        CodeDocumentViewModel codeDocumentViewModel,
        IEnumerable<string> filePaths,
        CancellationToken cancellationToken)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(text, cancellationToken: cancellationToken);
        var semanticModel = await GetCSharpSemanticModel(filePaths, syntaxTree, cancellationToken);
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

    public static async Task<SemanticModel?> GetCSharpSemanticModel(
        IEnumerable<string> filePaths,
        SyntaxTree syntaxTree,
        CancellationToken cancellationToken)
    {
        var syntaxTrees = await Task.WhenAll(filePaths
            .Select(async filePath =>
            {
                var fileText = File.ReadAllText(filePath);
                var syntaxTree = CSharpSyntaxTree.ParseText(fileText, cancellationToken: cancellationToken).WithFilePath(filePath);
                return syntaxTree;
            }));

        var compilation = CSharpCompilation
            .Create("CodeNavCompilation")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(syntaxTrees)
            .AddSyntaxTrees(syntaxTree);

        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        return semanticModel;
    }

    public CodeItem? MapMember(
        SyntaxNode member, SyntaxTree tree, SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel,
        bool mapBaseClass = true)
        => member.Kind() switch
            {
                SyntaxKind.ClassDeclaration when member is ClassDeclarationSyntax classSyntax
                    => classMapper.MapClass(classSyntax, semanticModel, tree, codeDocumentViewModel, this, mapBaseClass),
                SyntaxKind.ConstructorDeclaration when member is ConstructorDeclarationSyntax constructorSyntax
                    => methodMapper.MapConstructor(constructorSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.DelegateDeclaration when member is DelegateDeclarationSyntax delegateSyntax
                    => delegateEventMapper.MapDelegate(delegateSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.EnumDeclaration when member is EnumDeclarationSyntax enumSyntax
                    => enumMapper.MapEnum(enumSyntax, semanticModel, tree, codeDocumentViewModel, this),
                SyntaxKind.EnumMemberDeclaration when member is EnumMemberDeclarationSyntax enumMemberSyntax
                    => enumMapper.MapEnumMember(enumMemberSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.EventFieldDeclaration when member is EventFieldDeclarationSyntax eventFieldSyntax
                    => delegateEventMapper.MapEvent(eventFieldSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.FieldDeclaration when member is FieldDeclarationSyntax fieldSyntax
                    => fieldMapper.MapField(fieldSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.FileScopedNamespaceDeclaration when member is BaseNamespaceDeclarationSyntax namespaceSyntax
                    => namespaceMapper.MapNamespace(namespaceSyntax, semanticModel, tree, codeDocumentViewModel, this),
                SyntaxKind.GlobalStatement when member is GlobalStatementSyntax globalStatementSyntax
                    => statementMapper.MapGlobalStatement(globalStatementSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.IndexerDeclaration when member is IndexerDeclarationSyntax indexerSyntax
                    => indexerMapper.MapIndexer(indexerSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.InterfaceDeclaration when member is InterfaceDeclarationSyntax interfaceSyntax
                    => interfaceMapper.MapInterface(interfaceSyntax, semanticModel, tree, codeDocumentViewModel, this),
                SyntaxKind.MethodDeclaration when member is MethodDeclarationSyntax memberSyntax
                    => methodMapper.MapMethod(memberSyntax, semanticModel, codeDocumentViewModel, statementMapper),
                SyntaxKind.NamespaceDeclaration when member is BaseNamespaceDeclarationSyntax namespaceSyntax
                    => namespaceMapper.MapNamespace(namespaceSyntax, semanticModel, tree, codeDocumentViewModel, this),
                SyntaxKind.PropertyDeclaration when member is PropertyDeclarationSyntax propertySyntax
                    => propertyMapper.MapProperty(propertySyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.RecordDeclaration when member is RecordDeclarationSyntax recordSyntax
                    => recordMapper.MapRecord(recordSyntax, semanticModel, codeDocumentViewModel),
                SyntaxKind.StructDeclaration when member is StructDeclarationSyntax structSyntax
                    => structMapper.MapStruct(structSyntax, semanticModel, tree, codeDocumentViewModel, this),
                _ => null,
            };
}