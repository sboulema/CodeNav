using System.Runtime.Serialization;

namespace CodeNav.OutOfProc.Dialogs.SettingsDialog;

[DataContract]
public class SettingsDialogData
{
    /// <summary>
    /// Setting if the filter toolbar should be shown
    /// </summary>
    [DataMember]
    public bool ShowFilterToolbar { get; set; } = true;

    /// <summary>
    /// Setting if history/edit indicators should be shown
    /// </summary>
    [DataMember]
    public bool ShowHistoryIndicators { get; set; } = true;

    /// <summary>
    /// Setting if active code item should be highlighted
    /// </summary>
    [DataMember]
    public bool AutoHighlight { get; set; } = true;

    /// <summary>
    /// Setting if CodeNav should be updated while typing
    /// </summary>
    [DataMember]
    public bool UpdateWhileTyping { get; set; } = true;

    /// <summary>
    /// Setting to not update CodeNav for files over a certain line threshold
    /// </summary>
    [DataMember]
    public int AutoLoadLineThreshold { get; set; } = 0;

    /// <summary>
    /// Setting if CodeNav should sent crash analytics to Application Insights
    /// </summary>
    [DataMember]
    public bool EnableCrashAnalytics { get; set; }

    /// <summary>
    /// Setting if CodeNav should be shown or hidden for unsupported files and files without code items
    /// </summary>
    [DataMember]
    public bool ShowToolWindowForUnsupportedFiles { get; set; } = true;
}
