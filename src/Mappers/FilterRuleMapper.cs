using CodeNav.Constants;
using CodeNav.Dialogs.FilterDialog;
using CodeNav.Settings;
using CodeNav.ViewModels;
using Microsoft.VisualStudio.Extensibility.Settings;

namespace CodeNav.Mappers;

#pragma warning disable VSEXTPREVIEW_SETTINGS // Type is for evaluation purposes only and is subject to change or removal in future updates.

public static class FilterRuleMapper
{
    public static FilterRuleViewModel Map(ArraySettingItem arraySettingItem)
        => new()
        {
            Access = Enum.Parse<CodeItemAccessEnum>(arraySettingItem.GetValue<string>(SettingsDefinition.FilterRuleAccessSettingId)),
            Hide = GetValueOrDefault<bool>(arraySettingItem, SettingsDefinition.FilterRuleIsHiddenSettingId),
            IsEmpty = GetValueOrDefault<bool>(arraySettingItem, SettingsDefinition.FilterRuleIsEmptySettingId),
            Ignore = GetValueOrDefault<bool>(arraySettingItem, SettingsDefinition.FilterRuleIsIgnoredSettingId),
            Kind = Enum.Parse<CodeItemKindEnum>(arraySettingItem.GetValue<string>(SettingsDefinition.FilterRuleKindSettingId)),
            Opacity = GetValueOrDefault<int>(arraySettingItem, SettingsDefinition.FilterRuleOpacitySettingId),
        };

    public static ArraySettingItem[] Map(FilterDialogData filterDialogData)
        => [.. filterDialogData.FilterRules.Select(filterRule => new ArraySettingItem(new Dictionary<string, object>
        {
            { SettingsDefinition.FilterRuleAccessSettingId, filterRule.Access },
            { SettingsDefinition.FilterRuleIsHiddenSettingId, filterRule.Hide },
            { SettingsDefinition.FilterRuleIsEmptySettingId, filterRule.IsEmpty },
            { SettingsDefinition.FilterRuleIsIgnoredSettingId, filterRule.Ignore },
            { SettingsDefinition.FilterRuleKindSettingId, filterRule.Kind },
            { SettingsDefinition.FilterRuleOpacitySettingId, filterRule.Opacity },
        }))];

    public static CodeItemAccessEnum[] MapAccess(CodeItemKindEnum kind)
        => kind switch
        {
            CodeItemKindEnum.Namespace
                or CodeItemKindEnum.Switch
                or CodeItemKindEnum.SwitchSection 
                or CodeItemKindEnum.EnumMember
                or CodeItemKindEnum.Region
                or CodeItemKindEnum.ImplementedInterface
                or CodeItemKindEnum.Constructor 
                or CodeItemKindEnum.LocalFunction
                or CodeItemKindEnum.BaseClass => [CodeItemAccessEnum.All],
            CodeItemKindEnum.Struct => [CodeItemAccessEnum.Public, CodeItemAccessEnum.Internal, CodeItemAccessEnum.Private, CodeItemAccessEnum.All],
            _ => [.. Enum.GetValues<CodeItemAccessEnum>().Except([CodeItemAccessEnum.Unknown])],
        };

    public static bool MapEmpty(CodeItemKindEnum kind)
        => kind switch
        {
            CodeItemKindEnum.Namespace
                or CodeItemKindEnum.Class
                or CodeItemKindEnum.Region
                or CodeItemKindEnum.Interface
                or CodeItemKindEnum.Method
                or CodeItemKindEnum.BaseClass => true,
            _ => false,
        };

    public static bool MapIgnore(CodeItemKindEnum kind)
        => kind switch
        {
            CodeItemKindEnum.Namespace
                or CodeItemKindEnum.Region
                or CodeItemKindEnum.ImplementedInterface => true,
            _ => false,
        };

    public static bool MapHide(CodeItemKindEnum kind)
        => kind switch
        {
            CodeItemKindEnum.Unknown => false,
            _ => true,
        };

    public static bool MapOpacity(bool hide, bool ignore)
        => !(hide || ignore);

    private static T? GetValueOrDefault<T>(ArraySettingItem arraySettingItem, string propertyId)
    {
        var success = arraySettingItem.TryGetValue(propertyId, out T? value);

        if (!success)
        {
            return default;
        }

        return value;
    }
}
