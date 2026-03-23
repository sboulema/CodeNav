using CodeNav.OutOfProc.Models;
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
