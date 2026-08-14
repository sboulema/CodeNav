using CodeNav.OutOfProc.Interfaces;
using CodeNav.OutOfProc.Models;
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
                        .Where(file => file.Extension == ".vb")
                        .Where(file => file.Path != excludeFilePath)
                        .With(file => file.Path)), cancellationToken);

        var filePaths = projects
            .SelectMany(project => project.Files)
            .Select(file => file.Path);

        return await MapDocument(
            text,
            codeDocumentViewModel,
            filePaths,
            cancellationToken);
    }

    /// <summary>
    /// Map text document to list of code items.
    /// </summary>
    public static async Task<List<CodeItem>> MapDocument(
        string text,
        CodeDocumentViewModel codeDocumentViewModel,
        IEnumerable<string> filePaths,
        CancellationToken cancellationToken)
    {
        var syntaxTree = VisualBasicSyntaxTree.ParseText(
            text,
            cancellationToken: cancellationToken);

        var semanticModel = await GetVisualBasicSemanticModel(
            filePaths,
            syntaxTree,
            cancellationToken);

        var root =
            (CompilationUnitSyntax)
                await syntaxTree.GetRootAsync(cancellationToken);

        if (semanticModel == null)
        {
            return [];
        }

        return [.. root.Members
            .Where(member => member != null)
            .SelectMany(member =>
                MapMembers(
                    member,
                    syntaxTree,
                    semanticModel,
                    codeDocumentViewModel))
            ];
    }

    public static async Task<SemanticModel?> GetVisualBasicSemanticModel(
        IEnumerable<string> filePaths,
        SyntaxTree syntaxTree,
        CancellationToken cancellationToken)
    {
        var syntaxTrees = await Task.WhenAll(
            filePaths.Select(async filePath =>
            {
                var fileText = await File.ReadAllTextAsync(
                    filePath,
                    cancellationToken);

                return VisualBasicSyntaxTree
                    .ParseText(
                        fileText,
                        cancellationToken: cancellationToken)
                    .WithFilePath(filePath);
            }));

        var compilation = VisualBasicCompilation
            .Create("CodeNavCompilation")
            .AddReferences(
                MetadataReference.CreateFromFile(
                    typeof(object).Assembly.Location))
            .AddSyntaxTrees(syntaxTrees)
            .AddSyntaxTrees(syntaxTree);

        return compilation.GetSemanticModel(syntaxTree);
    }

    public static CodeItem? MapMember(
        SyntaxNode member,
        SyntaxTree tree,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
        => member switch
        {
            NamespaceBlockSyntax namespaceSyntax
                => NamespaceMapper.MapNamespace(
                    namespaceSyntax,
                    semanticModel,
                    tree,
                    codeDocumentViewModel),

            ClassBlockSyntax classSyntax
                => ClassMapper.MapClass(
                    classSyntax,
                    semanticModel,
                    tree,
                    codeDocumentViewModel),

            StructureBlockSyntax structureSyntax
                => StructureMapper.MapStructure(
                    structureSyntax,
                    semanticModel,
                    tree,
                    codeDocumentViewModel),

            InterfaceBlockSyntax interfaceSyntax
                => InterfaceMapper.MapInterface(
                    interfaceSyntax,
                    semanticModel,
                    tree,
                    codeDocumentViewModel),

            MethodBlockSyntax methodSyntax
                => MethodMapper.MapMethod(
                    methodSyntax,
                    semanticModel,
                    codeDocumentViewModel),

            MethodStatementSyntax methodSyntax
                => MethodMapper.MapMethod(
                    methodSyntax,
                    semanticModel,
                    codeDocumentViewModel),

            ConstructorBlockSyntax constructorSyntax
                => MethodMapper.MapConstructor(
                    constructorSyntax,
                    semanticModel,
                    codeDocumentViewModel),

            PropertyBlockSyntax propertySyntax
                => PropertyMapper.MapProperty(
                    propertySyntax,
                    semanticModel,
                    codeDocumentViewModel),

            PropertyStatementSyntax propertySyntax
                => PropertyMapper.MapProperty(
                    propertySyntax,
                    semanticModel,
                    codeDocumentViewModel),

            FieldDeclarationSyntax fieldSyntax
                => FieldMapper.MapFields(
                    fieldSyntax,
                    semanticModel,
                    codeDocumentViewModel)
                    .FirstOrDefault(),

            EventBlockSyntax eventSyntax
                => EventMapper.MapEvent(
                    eventSyntax,
                    semanticModel,
                    codeDocumentViewModel),

            EventStatementSyntax eventSyntax
                => EventMapper.MapEvent(
                    eventSyntax,
                    semanticModel,
                    codeDocumentViewModel),

            EnumBlockSyntax enumSyntax
                => EnumMapper.MapEnum(
                    enumSyntax,
                    semanticModel,
                    tree,
                    codeDocumentViewModel),

            DelegateStatementSyntax delegateSyntax
                => DelegateMapper.MapDelegate(
                    delegateSyntax,
                    semanticModel,
                    codeDocumentViewModel),

            ModuleBlockSyntax moduleSyntax
                => ModuleMapper.MapModule(
                    moduleSyntax,
                    semanticModel,
                    tree,
                    codeDocumentViewModel),

            OperatorStatementSyntax operatorSyntax
                => OperatorMapper.MapOperator(
                    operatorSyntax,
                    semanticModel,
                    codeDocumentViewModel),

            OperatorBlockSyntax operatorSyntax
                => OperatorMapper.MapOperator(
                    operatorSyntax,
                    semanticModel,
                    codeDocumentViewModel),

            DeclareStatementSyntax declareSyntax
                => DeclareMapper.MapDeclare(
                    declareSyntax,
                    semanticModel,
                    codeDocumentViewModel),

            _ => null,
        };

    public static IEnumerable<CodeItem> MapMembers(
        SyntaxNode member,
        SyntaxTree tree,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        if (member is FieldDeclarationSyntax fieldSyntax)
        {
            return FieldMapper.MapFields(
                fieldSyntax,
                semanticModel,
                codeDocumentViewModel);
        }

        var codeItem = MapMember(
            member,
            tree,
            semanticModel,
            codeDocumentViewModel);

        return codeItem == null ? [] : [codeItem];
    }

    public bool CanMapDocument(
        string filePath,
        GlobalSettings settings)
        => settings.EnableVisualBasic &&
           filePath.EndsWith(".vb", StringComparison.OrdinalIgnoreCase);
}
