using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Settings;

namespace CodeNav.Settings;

#pragma warning disable VSEXTPREVIEW_SETTINGS // The settings API is currently in preview and marked as experimental

internal static class SettingsDefinition
{
    internal const string FilterRuleAccessSettingId = "filterRuleAccess";
    internal const string FilterRuleIsHiddenSettingId = "filterRuleIsHidden";
    internal const string FilterRuleIsEmptySettingId = "filterRuleIsEmpty";
    internal const string FilterRuleIsIgnoredSettingId = "filterRuleIsIgnored";
    internal const string FilterRuleKindSettingId = "filterRuleKind";
    internal const string FilterRuleOpacitySettingId = "filterRuleOpacity";

    [VisualStudioContribution]
    internal static SettingCategory CodeNavSettingsCategory { get; } = new("codeNavSettings", "%CodeNav.Settings.Category.DisplayName%")
    {
        Description = "%CodeNav.Settings.Category.Description%",
        GenerateObserverClass = true,
    };

    [VisualStudioContribution]
    internal static Setting.Boolean ShowFilterToolbarSetting { get; } = new(
        "showFilterToolbar",
        "%CodeNav.Settings.ShowFilterToolbar.DisplayName%",
        CodeNavSettingsCategory,
        defaultValue: true)
    {
        Description = "%CodeNav.Settings.ShowFilterToolbar.Description%",
    };

    [VisualStudioContribution]
    internal static Setting.Boolean ShowHistoryIndicatorsSetting { get; } = new(
        "showHistoryIndicators",
        "%CodeNav.Settings.ShowHistoryIndicators.DisplayName%",
        CodeNavSettingsCategory,
        defaultValue: true)
    {
        Description = "%CodeNav.Settings.ShowHistoryIndicators.Description%",
    };

    [VisualStudioContribution]
    internal static Setting.Boolean AutoHighlightSetting { get; } = new(
        "autoHighlight",
        "%CodeNav.Settings.AutoHighlight.DisplayName%",
        CodeNavSettingsCategory,
        defaultValue: true)
    {
        Description = "%CodeNav.Settings.AutoHighlight.Description%",
    };

    [VisualStudioContribution]
    internal static Setting.Boolean UpdateWhileTypingSetting { get; } = new(
        "updateWhileTyping",
        "%CodeNav.Settings.UpdateWhileTyping.DisplayName%",
        CodeNavSettingsCategory,
        defaultValue: true)
    {
        Description = "%CodeNav.Settings.UpdateWhileTyping.Description%",
    };

    [VisualStudioContribution]
    internal static Setting.Integer AutoLoadLineThresholdSetting { get; } = new(
        "autoLoadLineThreshold",
        "%CodeNav.Settings.AutoLoadLineThreshold.DisplayName%",
        CodeNavSettingsCategory,
        defaultValue: 0)
    {
        Description = "%CodeNav.Settings.AutoLoadLineThreshold.Description%",
    };

    [VisualStudioContribution]
    internal static Setting.Enum SortOrderSetting { get; } = new(
        "sortOrder",
        "%CodeNav.Settings.SortOrder.DisplayName%",
        CodeNavSettingsCategory,
        [
            new("Unknown", "%CodeNav.Settings.SortOrder.Unknown%"),
            new("SortByFile", "%CodeNav.Settings.SortOrder.SortByFile%"),
            new("SortByName", "%CodeNav.Settings.SortOrder.SortByName%"),
        ],
        defaultValue: "SortByFile")
    {
        Description = "%CodeNav.Settings.SortOrder.Description%",
    };

    [VisualStudioContribution]
    internal static Setting.ObjectArray FilterRulesSetting { get; } = new(
        "filterRules",
        "%CodeNav.Settings.FilterRules.DisplayName%",
        CodeNavSettingsCategory,
        [
            new ArraySettingItemProperty.Enum(
                FilterRuleAccessSettingId,
                "%CodeNav.Settings.FilterRules.Access.DisplayName%",
                [
                    new("Unknown", "%CodeNav.Settings.FilterRules.Access.Unknown%"),
                    new("All", "%CodeNav.Settings.FilterRules.Access.All%"),
                    new("Public", "%CodeNav.Settings.FilterRules.Access.Public%"),
                    new("Private", "%CodeNav.Settings.FilterRules.Access.Private%"),
                    new("Protected", "%CodeNav.Settings.FilterRules.Access.Protected%"),
                    new("Internal", "%CodeNav.Settings.FilterRules.Access.Internal%"),
                    new("Sealed", "%CodeNav.Settings.FilterRules.Access.Sealed%"),
                ],
                "All"),
            new ArraySettingItemProperty.Boolean(FilterRuleIsHiddenSettingId, "%CodeNav.Settings.FilterRules.IsHidden.DisplayName%", false),
            new ArraySettingItemProperty.Boolean(FilterRuleIsEmptySettingId, "%CodeNav.Settings.FilterRules.IsEmpty.DisplayName%", false),
            new ArraySettingItemProperty.Boolean(FilterRuleIsIgnoredSettingId, "%CodeNav.Settings.FilterRules.IsIgnored.DisplayName%", false),
            new ArraySettingItemProperty.Integer(FilterRuleOpacitySettingId, "%CodeNav.Settings.FilterRules.Opacity.DisplayName%", 100),
            new ArraySettingItemProperty.Enum(
                FilterRuleKindSettingId,
                "%CodeNav.Settings.FilterRules.Kind.DisplayName%",
                [
                    new("Unknown", "%CodeNav.Settings.FilterRules.Kind.Unknown%"),
                    new("All", "%CodeNav.Settings.FilterRules.Kind.All%"),
                    new("BaseClass", "%CodeNav.Settings.FilterRules.Kind.BaseClass%"),
                    new("Class", "%CodeNav.Settings.FilterRules.Kind.Class%"),
                    new("Constant", "%CodeNav.Settings.FilterRules.Kind.Constant%"),
                    new("Constructor", "%CodeNav.Settings.FilterRules.Kind.Constructor%"),
                    new("Delegate", "%CodeNav.Settings.FilterRules.Kind.Delegate%"),
                    new("Enum", "%CodeNav.Settings.FilterRules.Kind.Enum%"),
                    new("EnumMember", "%CodeNav.Settings.FilterRules.Kind.EnumMember%"),
                    new("Event", "%CodeNav.Settings.FilterRules.Kind.Event%"),
                    new("ImplementedInterface", "%CodeNav.Settings.FilterRules.Kind.ImplementedInterface%"),
                    new("Indexer", "%CodeNav.Settings.FilterRules.Kind.Indexer%"),
                    new("Interface", "%CodeNav.Settings.FilterRules.Kind.Interface%"),
                    new("LocalFunction", "%CodeNav.Settings.FilterRules.Kind.LocalFunction%"),
                    new("Method", "%CodeNav.Settings.FilterRules.Kind.Method%"),
                    new("Namespace", "%CodeNav.Settings.FilterRules.Kind.Namespace%"),
                    new("Property", "%CodeNav.Settings.FilterRules.Kind.Property%"),
                    new("Record", "%CodeNav.Settings.FilterRules.Kind.Record%"),
                    new("Region", "%CodeNav.Settings.FilterRules.Kind.Region%"),
                    new("Struct", "%CodeNav.Settings.FilterRules.Kind.Struct%"),
                    new("Switch", "%CodeNav.Settings.FilterRules.Kind.Switch%"),
                    new("SwitchSection", "%CodeNav.Settings.FilterRules.Kind.SwitchSection%"),
                    new("Variable", "%CodeNav.Settings.FilterRules.Kind.Variable%"),
                ],
                "All"),
        ],
        defaultValue: [])
    {
        Description = "%CodeNav.Settings.FilterRules.Description%",
    };
}
