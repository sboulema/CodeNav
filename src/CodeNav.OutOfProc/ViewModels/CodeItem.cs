using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Helpers;
using CodeNav.Services;
using Microsoft;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;
using System.Windows;

namespace CodeNav.OutOfProc.ViewModels;

[DataContract]
public class CodeItem : NotifyPropertyChangedObject
{
    public CodeItem()
    {
        ClickItemCommand = new(ClickItem);
        GoToDefinitionCommand = new(GoToDefinition);
        GoToEndCommand = new(GoToEnd);
        SelectInCodeCommand = new(SelectInCode);
        CopyNameCommand = new(CopyName);
        RefreshCommand = new(Refresh);
        CollapseAllCommand = new(CollapseAll);
        ExpandAllCommand = new(ExpandAll);
        ClearHistoryCommand = new(ClearHistory);
        AddBookmarkCommand = new(AddBookmark);
        RemoveBookmarkCommand = new(RemoveBookmark);
        ClearBookmarksCommand = new(ClearBookmarks);
    }

    public CodeDocumentViewModel? CodeDocumentViewModel { get; set; }

    /// <summary>
    /// Display name of the code item
    /// </summary>
    [DataMember]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The type name of the data template to use for rendering the associated data.
    /// </summary>
    [DataMember]
    public string DataTemplateType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the span of text represented by this code item.
    /// </summary>
    public TextSpan Span { get; set; }

    /// <summary>
    /// Gets or sets the span of text represented by the identifier of this code item.
    /// </summary>
    public TextSpan IdentifierSpan { get; set; }

    /// <summary>
    /// Icon showing the type (class, namespace, etc.) of the code item
    /// </summary>
    [DataMember]
    public ImageMoniker Moniker { get; set; }

    /// <summary>
    /// Icon showing the access (public, private, etc.) of the code item
    /// </summary>
    [DataMember]
    public ImageMoniker OverlayMoniker { get; set; }

    /// <summary>
    /// Unique id of the code item based on fully qualified name
    /// </summary>
    [DataMember]
    public string Id { get; set; } = string.Empty;

    [DataMember]
    public string Tooltip { get; set; } = string.Empty;

    public Uri? FilePath { get; set; }

    internal string FullName = string.Empty;

    public CodeItemKindEnum Kind;

    public CodeItemAccessEnum Access;

    private bool _isHighlighted;

    /// <summary>
    /// Indicator if the item should be highlighted
    /// </summary>
    [DataMember]
    public bool IsHighlighted
    {
        get => _isHighlighted;
        set => SetProperty(ref _isHighlighted, value);
    }

    private bool _isBookmarked;

    /// <summary>
    /// Indicator if the item is bookmarked
    /// </summary>
    [DataMember]
    public bool IsBookmarked
    {
        get => _isBookmarked;
        set
        {
            SetProperty(ref _isBookmarked, value);
            RaiseNotifyPropertyChangedEvent(nameof(NotIsBookmarked));
            RaiseNotifyPropertyChangedEvent(nameof(BookmarkVisibility));
        }
    }

    /// <summary>
    /// Indicator if the item is not bookmarked
    /// </summary>
    [DataMember]
    public bool NotIsBookmarked => !IsBookmarked;

    #region Status Image
    private Visibility _statusMonikerVisibility = Visibility.Collapsed;

    /// <summary>
    /// Visibility of the history icon
    /// </summary>
    [DataMember]
    public Visibility StatusMonikerVisibility
    {
        get => _statusMonikerVisibility;
        set => SetProperty(ref _statusMonikerVisibility, value);
    }

    private bool _statusGrayscale;

    /// <summary>
    /// Indicator if the history icon should be shown in grayscale
    /// </summary>
    [DataMember]
    public bool StatusGrayscale
    {
        get => _statusGrayscale;
        set => SetProperty(ref _statusGrayscale, value);
    }

    private double _statusOpacity;

    /// <summary>
    /// Level (0 - 1) of opacity of the history icon
    /// </summary>
    [DataMember]
    public double StatusOpacity
    {
        get => _statusOpacity;
        set => SetProperty(ref _statusOpacity, value);
    }

    /// <summary>
    /// Visibility of the bookmark icon
    /// </summary>
    [DataMember]
    public Visibility BookmarkVisibility
        => IsBookmarked
            ? Visibility.Visible
            : Visibility.Collapsed;

    #endregion

    private Visibility _visibility;

    /// <summary>
    /// Visibility of the code item, used in filtering and searching
    /// </summary>
    [DataMember]
    public Visibility Visibility
    {
        get => _visibility;
        set => SetProperty(ref _visibility, value);
    }

    private double _opacity = 1;

    /// <summary>
    /// Level (0 - 1) of opacity of the code item based on the applied filter rules
    /// </summary>
    /// <remarks>
    /// 0 is transparent and 1 is opaque
    /// </remarks>
    [DataMember]
    public double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }

    #region Commands
    [DataMember]
    public AsyncCommand ClickItemCommand { get; }
    public async Task ClickItem(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        await LogHelper.LogInfo(this, $"Clicking item '{Name}'");

        var span = IdentifierSpan.Start != 0
            ? IdentifierSpan
            : Span;

        await LogHelper.LogInfo(this, $"Scrolling to span '{span}'");

        var inProcService = await clientContext.Extensibility
            .ServiceBroker
            .GetProxyAsync<IInProcService>(IInProcService.Configuration.ServiceDescriptor, cancellationToken: cancellationToken);

        try
        {
            Assumes.NotNull(inProcService);
            await inProcService.TextViewScrollToSpan(span.Start, span.Length);
        }
        finally
        {
            (inProcService as IDisposable)?.Dispose();
        }

        await LogHelper.LogInfo(this, $"Moving caret to position '{span.Start}'");

        await MoveCaretToPosition(span.Start, clientContext, cancellationToken);

        await LogHelper.LogInfo(this, $"Adding item '{Name}' to history");

        HistoryHelper.AddItemToHistory(this);
    }

    [DataMember]
    public AsyncCommand GoToDefinitionCommand { get; }
    private async Task GoToDefinition(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => await MoveCaretToPosition(Span.Start, clientContext, cancellationToken);

    [DataMember]
    public AsyncCommand ClearHistoryCommand { get; }
    public async Task ClearHistory(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => HistoryHelper.ClearHistory(CodeDocumentViewModel);

    [DataMember]
    public AsyncCommand GoToEndCommand { get; }
    public async Task GoToEnd(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => await MoveCaretToPosition(Span.End, clientContext, cancellationToken);

    [DataMember]
    public AsyncCommand SelectInCodeCommand { get; }
    public async Task SelectInCode(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => await SelectLines(clientContext, cancellationToken);

    [DataMember]
    public AsyncCommand CopyNameCommand { get; }
    public async Task CopyName(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        TaskCompletionSource<bool> taskCompletionSource = new();
        var thread = new Thread(() =>
        {
            try
            {
                Clipboard.SetText(Name);
                taskCompletionSource.SetResult(false);
            }
            catch (Exception e)
            {
                taskCompletionSource.SetException(e);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    [DataMember]
    public AsyncCommand RefreshCommand { get; }
    public async Task Refresh(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        var textView = await clientContext.GetActiveTextViewAsync(cancellationToken);
        await CodeDocumentViewModel!
            .CodeDocumentService!
            .UpdateCodeDocumentViewModel(clientContext.Extensibility, textView, cancellationToken);
    }

    [DataMember]
    public AsyncCommand ExpandAllCommand { get; }
    public async Task ExpandAll(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => OutliningHelper.ExpandAll(CodeDocumentViewModel!);

    [DataMember]
    public AsyncCommand CollapseAllCommand { get; }
    public async Task CollapseAll(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => OutliningHelper.CollapseAll(CodeDocumentViewModel!);

    [DataMember]
    public AsyncCommand AddBookmarkCommand { get; }
    public async Task AddBookmark(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => BookmarkHelper.AddItemToBookmarks(CodeDocumentViewModel!, this);

    [DataMember]
    public AsyncCommand RemoveBookmarkCommand { get; }
    public async Task RemoveBookmark(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => BookmarkHelper.RemoveItemFromBookmarks(CodeDocumentViewModel!, this);

    [DataMember]
    public AsyncCommand ClearBookmarksCommand { get; }
    public async Task ClearBookmarks(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => BookmarkHelper.ClearBookmarks(CodeDocumentViewModel);

    #endregion

    private async Task MoveCaretToPosition(
        int position,
        IClientContext clientContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var textViewSnapshot = await clientContext.GetActiveTextViewAsync(cancellationToken);

            if (textViewSnapshot == null)
            {
                return;
            }

            var textDocumentSnapshot = textViewSnapshot?.Document;

            // If the code item has a different file path, open that document
            if (FilePath != null &&
                textViewSnapshot?.Uri != FilePath)
            {
                textDocumentSnapshot = await clientContext.Extensibility
                    .Documents()
                    .OpenTextDocumentAsync(FilePath, cancellationToken);
            }

            if (textDocumentSnapshot == null)
            {
                return;
            }

            // Select the requested line
            await clientContext.Extensibility.Editor().EditAsync(batch =>
            {
                var caret = new TextPosition(textDocumentSnapshot, position);
                textViewSnapshot!.AsEditable(batch).SetSelections(
                [
                    new Selection(activePosition: caret, anchorPosition: caret, insertionPosition: caret)
                ]);
            },
            cancellationToken);
        }
        catch (Exception)
        {
            // Ignore
        }
    }

    private async Task SelectLines(IClientContext clientContext, CancellationToken cancellationToken)
    {
        try
        {
            var textViewSnapshot = await clientContext.GetActiveTextViewAsync(cancellationToken);

            if (textViewSnapshot == null)
            {
                return;
            }

            var textDocumentSnapshot = textViewSnapshot?.Document;

            // If the code item has a different file path, open that document
            if (FilePath != null &&
                textViewSnapshot?.Uri != FilePath)
            {
                textDocumentSnapshot = await clientContext.Extensibility
                    .Documents()
                    .OpenTextDocumentAsync(FilePath, cancellationToken);
            }

            if (textDocumentSnapshot == null)
            {
                return;
            }

            // Select all lines corresponding to the code item
            await clientContext.Extensibility.Editor().EditAsync(batch =>
            {
                textViewSnapshot!.AsEditable(batch).SetSelections(
                [
                    new Selection(
                        new TextRange(
                            new TextPosition(textDocumentSnapshot, Span.Start),
                            new TextPosition(textDocumentSnapshot, Span.End)))
                ]);
            },
            cancellationToken);
        }
        catch (Exception)
        {
            // Ignore
        }
    }
}
