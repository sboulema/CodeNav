using CodeNav.OutOfProc.Interfaces;
using CodeNav.OutOfProc.Languages.TypeScript.Parsing;
using CodeNav.OutOfProc.Models;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Extensibility;

namespace CodeNav.OutOfProc.Languages.TypeScript.Mappers;

public class DocumentMapper : IDocumentMapper
{
    /// <summary>
    /// Map text document to list of code items.
    /// </summary>
    /// <remarks>
    /// Unlike the C#/VB mappers, this does not need to look at any other files in the solution:
    /// there is no semantic model being built, just a structural scan of the current document.
    /// </remarks>
    /// <param name="text">Text of the code document</param>
    /// <param name="excludeFilePath">Unused for TypeScript, kept to satisfy IDocumentMapper</param>
    /// <param name="codeDocumentViewModel">Current view model connected to the CodeNav tool window</param>
    /// <param name="extensibility">Unused for TypeScript, kept to satisfy IDocumentMapper</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of code items</returns>
    public Task<List<CodeItem>> MapDocument(
        string text,
        string? excludeFilePath,
        CodeDocumentViewModel codeDocumentViewModel,
        VisualStudioExtensibility extensibility,
        CancellationToken cancellationToken)
    {
        var nodes = TypeScriptParser.Parse(text);

        var sourceText = SourceText.From(text);

        var codeItems = CodeItemMapper.MapNodes(
            nodes,
            codeDocumentViewModel,
            sourceText,
            parentFullName: string.Empty,
            isMember: false);

        return Task.FromResult(codeItems);
    }

    public bool CanMapDocument(
        string filePath,
        GlobalSettings settings)
        => settings.EnableTypeScript &&
           !filePath.EndsWith(".d.ts", StringComparison.OrdinalIgnoreCase) &&
           (filePath.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ||
            filePath.EndsWith(".tsx", StringComparison.OrdinalIgnoreCase));
}
