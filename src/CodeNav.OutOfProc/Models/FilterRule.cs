using CodeNav.OutOfProc.Constants;

namespace CodeNav.OutOfProc.Models;

public class FilterRule
{
    public CodeItemKindEnum Kind { get; set; }

    public CodeItemAccessEnum Access { get; set; }

    public bool? IsEmpty { get; set; } = false;

    public bool Hide { get; set; }

    public bool Ignore { get; set; }

    public int Opacity { get; set; }

    public bool Italic { get; set; }

    public int FontScale { get; set; }
}
