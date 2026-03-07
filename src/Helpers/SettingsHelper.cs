using CodeNav.Constants;
using CodeNav.Dialogs.FilterDialog;
using CodeNav.Dialogs.SettingsDialog;
using CodeNav.Mappers;
using CodeNav.Settings;
using CodeNav.Settings.Settings;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Settings;
using System.Windows;

namespace CodeNav.Helpers;

#pragma warning disable VSEXTPREVIEW_SETTINGS // Type is for evaluation purposes only and is subject to change or removal in future updates.

public static class SettingsHelper
{
    public static async Task<SettingsDialogData> GetSettings(
        VisualStudioExtensibility extensibility,
        CancellationToken cancellationToken)
    {
        var settingCategory = await extensibility
            .Settings()
            .ReadEffectiveValuesAsync(
            [
                SettingsDefinition.CodeNavSettingsCategory,
            ],
            cancellationToken);

        return new()
        {
            AutoHighlight = GetSettingValueOrDefault(settingCategory, SettingsDefinition.AutoHighlightSetting),
            AutoLoadLineThreshold = GetSettingValueOrDefault(settingCategory, SettingsDefinition.AutoLoadLineThresholdSetting),
            ShowFilterToolbar = GetSettingValueOrDefault(settingCategory, SettingsDefinition.ShowFilterToolbarSetting),
            ShowHistoryIndicators = GetSettingValueOrDefault(settingCategory, SettingsDefinition.ShowHistoryIndicatorsSetting),
            UpdateWhileTyping = GetSettingValueOrDefault(settingCategory, SettingsDefinition.UpdateWhileTypingSetting),
        };
    }

    public static SettingsDialogData GetSettings(CodeNavSettingsCategorySnapshot settingsSnapshot)
        => new()
        {
            AutoHighlight = settingsSnapshot.AutoHighlightSetting.ValueOrDefault(SettingsDefinition.AutoHighlightSetting.DefaultValue),
            AutoLoadLineThreshold = settingsSnapshot.AutoLoadLineThresholdSetting.ValueOrDefault(SettingsDefinition.AutoLoadLineThresholdSetting.DefaultValue),
            ShowFilterToolbar = settingsSnapshot.ShowFilterToolbarSetting.ValueOrDefault(SettingsDefinition.ShowFilterToolbarSetting.DefaultValue),
            ShowHistoryIndicators = settingsSnapshot.ShowHistoryIndicatorsSetting.ValueOrDefault(SettingsDefinition.ShowHistoryIndicatorsSetting.DefaultValue),
            UpdateWhileTyping = settingsSnapshot.UpdateWhileTypingSetting.ValueOrDefault(SettingsDefinition.UpdateWhileTypingSetting.DefaultValue),
        };

    public static async Task<FilterDialogData> GetFilterRules(
        VisualStudioExtensibility extensibility,
        CancellationToken cancellationToken)
    {
        var settingValues = await extensibility
            .Settings()
            .ReadEffectiveValuesAsync([SettingsDefinition.FilterRulesSetting], cancellationToken);

        var settingItems = GetSettingValueOrDefault(settingValues, SettingsDefinition.FilterRulesSetting);

        var filterRules = settingItems
            .Select(FilterRuleMapper.Map)
            .ToList();

        return new()
        {
            FilterRules = new(filterRules),
        };
    }

    public static FilterDialogData GetFilterRules(CodeNavSettingsCategorySnapshot settingsSnapshot)
    {
        var settingItems = settingsSnapshot.FilterRulesSetting.ValueOrDefault(SettingsDefinition.FilterRulesSetting.DefaultValue);

        var filterRules = settingItems
            .Select(FilterRuleMapper.Map)
            .ToList();

        return new()
        {
            FilterRules = new(filterRules),
        };
    }

    public static async Task<SettingsWriteResponse> SaveSettings(
        VisualStudioExtensibility extensibility,
        SettingsDialogData settings,
        CancellationToken cancellationToken)
        => await extensibility
            .Settings()
            .WriteAsync(
                batch =>
                {
                    batch.WriteSetting(SettingsDefinition.AutoHighlightSetting, settings.AutoHighlight);
                    batch.WriteSetting(SettingsDefinition.AutoLoadLineThresholdSetting, settings.AutoLoadLineThreshold);
                    batch.WriteSetting(SettingsDefinition.ShowFilterToolbarSetting, settings.ShowFilterToolbar);
                    batch.WriteSetting(SettingsDefinition.ShowHistoryIndicatorsSetting, settings.ShowHistoryIndicators);
                    batch.WriteSetting(SettingsDefinition.UpdateWhileTypingSetting, settings.UpdateWhileTyping);
                },
                description: "Settings saved via CodeNav dialog",
                cancellationToken);

    public static async Task<SettingsWriteResponse> SaveSettings(
        VisualStudioExtensibility extensibility,
        FilterDialogData filterRules,
        CancellationToken cancellationToken)
        => await extensibility
            .Settings()
            .WriteAsync(
                batch =>
                {
                    batch.WriteSetting(SettingsDefinition.FilterRulesSetting, FilterRuleMapper.Map(filterRules));
                },
                description: "Filter rules saved via CodeNav dialog",
                cancellationToken);

    public static async Task<SettingsWriteResponse> SaveSortOrder(
        VisualStudioExtensibility extensibility,
        SortOrderEnum sortOrder,
        CancellationToken cancellationToken)
        => await extensibility
            .Settings()
            .WriteAsync(
                batch =>
                {
                    batch.WriteSetting(SettingsDefinition.SortOrderSetting, sortOrder.ToString());
                },
                description: "Sort order saved via toolbar button",
                cancellationToken);

    public static Visibility GetShowFilterToolbarVisibility(CodeNavSettingsCategorySnapshot settingsSnapshot)
    {
        if (settingsSnapshot.ShowFilterToolbarSetting == null)
        {
            return BoolToVisibility(SettingsDefinition.ShowFilterToolbarSetting.DefaultValue);
        }

        return BoolToVisibility(settingsSnapshot.ShowFilterToolbarSetting.ValueOrDefault(SettingsDefinition.ShowFilterToolbarSetting.DefaultValue));

        static Visibility BoolToVisibility(bool value)
            => value ? Visibility.Visible : Visibility.Collapsed;
    }

    public static SortOrderEnum GetSortOrder(CodeNavSettingsCategorySnapshot settingsSnapshot)
    {
        if (settingsSnapshot.SortOrderSetting == null)
        {
            return Enum.Parse<SortOrderEnum>(SettingsDefinition.SortOrderSetting.DefaultValue);
        }

        return Enum.Parse<SortOrderEnum>(settingsSnapshot.SortOrderSetting.ValueOrDefault(SettingsDefinition.SortOrderSetting.DefaultValue));
    }

    /// <summary>
    /// Get the value of a setting based on its full id and a list of current setting values. 
    /// </summary>
    /// <param name="settingValues">Dictionary of setting key value pairs</param>
    /// <param name="setting">Boolean setting definition</param>
    /// <returns></returns>
    private static bool GetSettingValueOrDefault(SettingValues? settingValues, Setting.Boolean setting)
    {
        if (settingValues == null)
        {
            return setting.DefaultValue;
        }

        return settingValues[setting.FullId].ValueOrDefault(setting.DefaultValue);
    }

    /// <summary>
    /// Get the value of a setting based on its full id and a list of current setting values. 
    /// </summary>
    /// <param name="settingValues">Dictionary of setting key value pairs</param>
    /// <param name="setting">Int setting definition</param>
    /// <returns></returns>
    private static int GetSettingValueOrDefault(SettingValues? settingValues, Setting.Integer setting)
    {
        if (settingValues == null)
        {
            return setting.DefaultValue;
        }

        return settingValues[setting.FullId].ValueOrDefault(setting.DefaultValue);
    }

    /// <summary>
    /// Get the value of a setting based on its full id and a list of current setting values. 
    /// </summary>
    /// <param name="settingValues">Dictionary of setting key value pairs</param>
    /// <param name="setting">Int setting definition</param>
    /// <returns></returns>
    private static ArraySettingItem[] GetSettingValueOrDefault(SettingValues? settingValues, Setting.ObjectArray setting)
    {
        if (settingValues == null)
        {
            return setting.DefaultValue;
        }

        return settingValues[setting.FullId].ValueOrDefault(setting.DefaultValue);
    }
}
