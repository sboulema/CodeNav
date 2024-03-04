using CodeNav.Helpers;
using Community.VisualStudio.Toolkit;
using System.Drawing;
using System.Runtime.InteropServices;

internal partial class OptionsProvider
{
    [ComVisible(true)]
    public class GeneralOptions : BaseOptionPage<General> { }
}

public class General : BaseOptionModel<General>
{
    public bool ShowFilterToolbar { get; set; } = true;

    public bool UseXMLComments { get; set; } = false;

    public bool ShowHistoryIndicators { get; set; } = true;

    public bool DisableHighlight { get; set; } = false;

    public int MarginSide { get; set; } = 0;

    public int AutoLoadLineThreshold { get; set; } = 0;

    public string FontFamilyName { get; set; } = "Segoe UI";

    public float FontSize { get; set; } = 11.25f;

    public FontStyle FontStyle { get; set; } = FontStyle.Regular;

    public Color HighlightColor { get; set; } = ColorHelper.Transparent();

    public Color BackgroundColor { get; set; } = ColorHelper.Transparent();

    public double Width { get; set; } = 200;

    public bool ShowMargin { get; set; } = true;

    public string FilterRules { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;

    public bool UpdateWhileTyping { get; set; } = false;

    public bool ShowRegions { get; set; } = true;
}