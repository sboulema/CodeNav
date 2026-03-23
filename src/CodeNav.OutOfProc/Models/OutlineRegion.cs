namespace CodeNav.OutOfProc.Models;

public sealed class OutlineRegion
{
    public int SpanStart { get; set; }

    public int SpanEnd { get; set; }

    public bool IsExpanded { get; set; }
}
