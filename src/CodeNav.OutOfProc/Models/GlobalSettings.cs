using CodeNav.OutOfProc.Constants;

namespace CodeNav.OutOfProc.Models;

public class GlobalSettings
{
    /// <summary>
    /// Settings dialog - Setting if the filter toolbar should be shown
    /// </summary>
    public bool ShowFilterToolbar { get; set; } = true;

    /// <summary>
    /// Settings dialog - Setting if history/edit indicators should be shown
    /// </summary>
    public bool ShowHistoryIndicators { get; set; } = true;

    /// <summary>
    /// Settings dialog - Setting if active code item should be highlighted
    /// </summary>
    public bool AutoHighlight { get; set; } = true;

    /// <summary>
    /// Settings dialog - Setting if CodeNav should be updated while typing
    /// </summary>
    public bool UpdateWhileTyping { get; set; } = true;

    /// <summary>
    /// Settings dialog - Setting to not update CodeNav for files over a certain line threshold
    /// </summary>
    public int AutoLoadLineThreshold { get; set; } = 0;

    /// <summary>
    /// Settings dialog - Setting if CodeNav should sent crash analytics to Application Insights
    /// </summary>
    public bool EnableCrashAnalytics { get; set; }

    /// <summary>
    /// Main toolbar - Setting to store the selected sort order for code items
    /// </summary>
    public SortOrderEnum SortOrder { get; set; } = SortOrderEnum.SortByFile;

    /// <summary>
    /// Filter dialog - Settings to store the list of filter rules for code items
    /// </summary>
    public List<FilterRule> FilterRules { get; set; } = [];
}
