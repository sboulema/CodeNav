using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Services;
using Newtonsoft.Json;

namespace CodeNav.OutOfProc.Helpers;

public static class SettingsHelper
{
    private static readonly string _globalSettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CodeNav");

    private static readonly string _globalSettingsFile = Path.Combine(_globalSettingsFolder, "CodeNav.json");

    /// <summary>
    /// Save global settings to disk.
    /// </summary>
    public static async Task SaveGlobalSettings(CodeDocumentService codeDocumentService)
    {
        try
        {
            if (!Directory.Exists(_globalSettingsFolder))
            {
                Directory.CreateDirectory(_globalSettingsFolder);
            }

            var json = JsonConvert.SerializeObject(codeDocumentService.GlobalSettings, Formatting.Indented);

            using var writer = new StreamWriter(_globalSettingsFile, false);

            await writer.WriteAsync(json);
        }
        catch (Exception e)
        {
            await LogHelper.LogException(codeDocumentService, $"Failed to save global settings", e);
        }
    }


    /// <summary>
    /// Loads saved global settings from disk.
    /// </summary>
    public static async Task<GlobalSettings> LoadGlobalSettings(CodeDocumentService codeDocumentService)
    {
        try
        {
            if (!File.Exists(_globalSettingsFile))
            {
                return new();
            }

            using var reader = new StreamReader(_globalSettingsFile);
            var json = await reader.ReadToEndAsync();
            var settings = JsonConvert.DeserializeObject<GlobalSettings>(json);

            return settings ?? new();
        }
        catch (Exception e)
        {
            await LogHelper.LogException(codeDocumentService, $"Failed to load global settings", e);
            return new();
        }
    }
}

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

public class FilterRule
{
    public CodeItemKindEnum Kind { get; set; }

    public CodeItemAccessEnum Access { get; set; }

    public bool? IsEmpty { get; set; } = false;

    public bool Hide { get; set; }

    public bool Ignore { get; set; }

    public int Opacity { get; set; }
}
