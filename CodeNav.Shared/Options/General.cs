using CodeNav.Helpers;
using Community.VisualStudio.Toolkit;
using System.Drawing;

internal partial class OptionsProvider
{
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

    public Font Font { get; set; } = new Font("Segoe UI", 11.25f);

    public Color HighlightColor { get; set; } = ColorHelper.Transparent();

    public Color BackgroundColor { get; set; } = ColorHelper.Transparent();

    public double Width { get; set; } = 200;

    public bool ShowMargin { get; set; } = true;

    public string FilterRules { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;

    public int FilterWindowHeight { get; set; } = 270;

    public int FilterWindowWidth { get; set; } = 450;

    public int FilterWindowLeft { get; set; } = 0;

    public int FilterWindowTop { get; set; } = 0;
}