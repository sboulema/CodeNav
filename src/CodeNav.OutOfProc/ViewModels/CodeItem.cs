using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Services;
using CodeNav.Services;
using Microsoft;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.UI;
using Microsoft.VisualStudio.RpcContracts.OpenDocument;
using System.Runtime.Serialization;
using System.Windows;
using Range = Microsoft.VisualStudio.RpcContracts.Utilities.Range;

namespace CodeNav.OutOfProc.ViewModels;

[DataContract]
public class CodeItem : NotifyPropertyChangedObject
{
    public CodeItem()
    {
        ClickItemCommand = new(ClickItem);
        DoubleClickItemCommand = new(DoubleClickItem);
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

    /// <summary>
    /// The parent view model of the code item
    /// </summary>
    [DataMember]
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
    /// Gets or sets the zero-based line and column position of the start of <see cref="Span"/>.
    /// </summary>
    public LinePosition SpanStartLinePosition { get; set; }

    /// <summary>
    /// Gets or sets the zero-based line and column position of the end of <see cref="Span"/>.
    /// </summary>
    public LinePosition SpanEndLinePosition { get; set; }

    /// <summary>
    /// Gets or sets the span of text represented by the identifier of this code item.
    /// </summary>
    public TextSpan? IdentifierSpan { get; set; }

    /// <summary>
    /// Gets or sets the zero-based line and column position of the start of <see cref="IdentifierSpan"/>.
    /// </summary>
    public LinePosition? IdentifierSpanStartLinePosition { get; set; }

    /// <summary>
    /// Gets or sets the zero-based line and column position of the end of <see cref="IdentifierSpan"/>.
    /// </summary>
    public LinePosition? IdentifierSpanEndLinePosition { get; set; }

    /// <summary>
    /// Gets or sets the span of text represented by the outline region of this code item.
    /// </summary>
    public TextSpan OutlineSpan { get; set; }

    /// <summary>
    /// Gets or sets the zero-based line and column position of the start of <see cref="OutlineSpan"/>.
    /// </summary>
    public LinePosition? OutlineSpanStartLinePosition { get; set; }

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

    /// <summary>
    /// Path to the file containing the code item
    /// </summary>
    /// <remarks>
    /// Used for opening the file if it's different from the currently active one
    /// </remarks>
    public Uri? FilePath { get; set; }

    /// <summary>
    /// Full name of the code item
    /// </summary>
    /// <remarks>
    /// Used in constructing a unique id 
    /// </remarks>
    internal string FullName = string.Empty;

    /// <summary>
    /// Primary kind of this code item.
    /// </summary>
    /// <remarks>
    /// The primary kind describes what the code item is structurally, such as a
    /// class, method, property, or field.
    /// </remarks>
    public CodeItemKindEnum Kind;

    /// <summary>
    /// Additional kinds associated with this code item.
    /// </summary>
    /// <remarks>
    /// Additional kinds provide contextual classifications without replacing the
    /// primary <see cref="Kind"/>. For example, an interface method can have
    /// <see cref="CodeItemKindEnum.Method"/> as its primary kind and
    /// <see cref="CodeItemKindEnum.InterfaceMember"/> as an additional kind.
    /// </remarks>
    public HashSet<CodeItemKindEnum> AdditionalKinds { get; } = [];

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

    private bool _IsItalic;

    /// <summary>
    /// Indicator if the item should be italic
    /// </summary>
    [DataMember]
    public bool IsItalic
    {
        get => _IsItalic;
        set => SetProperty(ref _IsItalic, value);
    }

    private double _fontSize;

    /// <summary>
    /// Font size of the code item based on the applied filter rules
    /// </summary>
    [DataMember]
    public double FontSize
    {
        get => _fontSize;
        set => SetProperty(ref _fontSize, value);
    }

    #region Commands
    /// <summary>
    /// Handles a single click on a code item, navigating to it in the text editor
    /// without shifting focus away from the CodeNav tool window.
    /// </summary>
    /// <remarks>
    /// Scrolls the active text view so the item's span (its <see cref="IdentifierSpan"/> if
    /// set, otherwise its full <see cref="Span"/>) becomes visible, then places a collapsed
    /// caret at the start of that span. This only sets the selection/caret position — it does
    /// not activate or focus the text editor.
    /// The item is also recorded in the navigation history.
    /// </remarks>
    [DataMember]
    public AsyncCommand ClickItemCommand { get; }
    public async Task ClickItem(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        await LogHelper.LogInfo(this, $"Clicking item '{Name}'");

        var linePosition = IdentifierSpanStartLinePosition != null
            ? IdentifierSpanStartLinePosition.Value
            : SpanStartLinePosition;

        await OpenTextDocument(clientContext, cancellationToken, startLinePosition: linePosition);

        await LogHelper.LogInfo(this, $"Adding item '{Name}' to history");

        HistoryHelper.AddItemToHistory(this);
    }

    [DataMember]
    public AsyncCommand GoToDefinitionCommand { get; }
    private async Task GoToDefinition(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => await OpenTextDocument(clientContext, cancellationToken, startLinePosition: SpanStartLinePosition);

    [DataMember]
    public AsyncCommand ClearHistoryCommand { get; }
    public async Task ClearHistory(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => HistoryHelper.ClearHistory(CodeDocumentViewModel);

    [DataMember]
    public AsyncCommand GoToEndCommand { get; }
    public async Task GoToEnd(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => await OpenTextDocument(clientContext, cancellationToken, startLinePosition: SpanEndLinePosition);

    [DataMember]
    public AsyncCommand SelectInCodeCommand { get; }
    public async Task SelectInCode(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => await OpenTextDocument(clientContext, cancellationToken, startLinePosition: SpanStartLinePosition, endLinePosition: SpanEndLinePosition);

    /// <summary>
    /// Handles a double click on a code item, moving the caret into the code and switching
    /// focus to the text editor.
    /// </summary>
    /// <remarks>
    /// Moves the caret to the start of the item's <see cref="OutlineSpan"/> (rather than its
    /// <see cref="IdentifierSpan"/>/<see cref="Span"/>). This is different than <see cref="ClickItem"/>:
    /// it activates the text editor, so after a double click the user can start typing immediately
    /// without clicking into the document first.
    /// </remarks>
    [DataMember]
    public AsyncCommand DoubleClickItemCommand { get; }
    public async Task DoubleClickItem(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => await OpenTextDocument(clientContext, cancellationToken, activate: true, startLinePosition: OutlineSpanStartLinePosition);

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
        var textViewSnapshot = await clientContext.GetActiveTextViewAsync(cancellationToken);

        if (textViewSnapshot == null)
        {
            return;
        }

        await CodeDocumentViewModel!
            .CodeDocumentService!
            .UpdateCodeDocumentViewModel(
                clientContext.Extensibility,
                textViewSnapshot.FilePath,
                textViewSnapshot.Document.Text.CopyToString(),
                CodeDocumentViewModel,
                cancellationToken);
    }

    [DataMember]
    public AsyncCommand ExpandAllCommand { get; }
    public async Task ExpandAll(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => OutliningService.ExpandAll(CodeDocumentViewModel!);

    [DataMember]
    public AsyncCommand CollapseAllCommand { get; }
    public async Task CollapseAll(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => OutliningService.CollapseAll(CodeDocumentViewModel!);

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

    private Uri? GetFilePath()
    {
        // Return the file path set on this code item,
        // that indicates the source of the code item is in a different file than the rest of the code items.
        if (FilePath != null)
        {
            return FilePath;
        }

        // Return the general file path of the code document view model, if available.
        if (!string.IsNullOrEmpty(CodeDocumentViewModel?.FilePath))
        {
            return new Uri(CodeDocumentViewModel!.FilePath);
        }

        return null;
    }

    /// <summary>
    /// Opens the text document associated with this instance's file path, optionally selecting a
    /// range in the document defined by <paramref name="startLinePosition"/> and <paramref name="endLinePosition"/>.
    /// </summary>
    /// <param name="clientContext">The client context used to access the extensibility services.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="activate">If <see langword="true"/>, activates the document once opened; otherwise, opens it without activating it.</param>
    /// <param name="startLinePosition">The zero-based start of the range to select, or <see langword="null"/> if no selection should be applied.</param>
    /// <param name="endLinePosition">
    /// The zero-based end of the range to select. If <see langword="null"/>, <paramref name="startLinePosition"/> is used as the end as well,
    /// resulting in a collapsed (zero-length) selection.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the opened
    /// <see cref="ITextDocumentSnapshot"/>, or <see langword="null"/> if the file path could not be resolved.
    /// </returns>
    private async Task<ITextDocumentSnapshot?> OpenTextDocument(
        IClientContext clientContext,
        CancellationToken cancellationToken,
        bool activate = false,
        LinePosition? startLinePosition = null,
        LinePosition? endLinePosition = null)
    {
        var filePath = GetFilePath();

        if (filePath == null)
        {
            return null;
        }

        Range? selection = startLinePosition == null
            ? null
            : new Range(
                startLinePosition.Value.Line,
                startLinePosition.Value.Character,
                endLinePosition?.Line ?? startLinePosition.Value.Line,
                endLinePosition?.Character ?? startLinePosition.Value.Character);

        return await clientContext.Extensibility
            .Documents()
            .OpenTextDocumentAsync(
                filePath,
                new(activate: activate, selection: selection),
                cancellationToken);
    }
}
