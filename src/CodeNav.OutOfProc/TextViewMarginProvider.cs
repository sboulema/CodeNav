using CodeNav.OutOfProc.Services;
using CodeNav.OutOfProc.ToolWindows;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;

namespace CodeNav.OutOfProc;

[VisualStudioContribution]
internal class TextViewMarginProvider(CodeDocumentService codeDocumentService) : ExtensionPart, ITextViewMarginProvider
{
    /// <inheritdoc />
    public TextViewExtensionConfiguration TextViewExtensionConfiguration => new()
    {
        AppliesTo =
        [
            DocumentFilter.FromDocumentType("CSharp"),
            DocumentFilter.FromGlobPattern("**/*.cs", true),
        ],
    };

    /// <inheritdoc />
    public TextViewMarginProviderConfiguration TextViewMarginProviderConfiguration =>
        new(marginContainer: ContainerMarginPlacement.KnownValues.Left)
        {
            Before = [MarginPlacement.KnownValues.Glyph],
        };

    /// <inheritdoc />
    public Task<IRemoteUserControl> CreateVisualElementAsync(ITextViewSnapshot textView, CancellationToken cancellationToken)
    {
        return Task.FromResult<IRemoteUserControl>(new CodeNavToolWindowControl(codeDocumentService.CodeDocumentViewModel));
    }
}