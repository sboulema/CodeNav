namespace CodeNav.Models;

public sealed class DocumentView
{
    public string FilePath { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public bool IsDirty { get; set; }

    public bool IsFrameChange { get; set; }

    public bool IsDocumentFrame { get; set; }
}
