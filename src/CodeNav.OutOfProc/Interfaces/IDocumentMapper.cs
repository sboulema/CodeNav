using CodeNav.OutOfProc.Models;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.VisualStudio.Extensibility;

namespace CodeNav.OutOfProc.Interfaces;
    
public interface IDocumentMapper
{
    bool CanMapDocument(
        string filePath,
        GlobalSettings settings);

    Task<List<CodeItem>> MapDocument(
        string text,
        string? excludeFilePath,
        CodeDocumentViewModel codeDocumentViewModel,
        VisualStudioExtensibility extensibility,
        CancellationToken cancellationToken);
}