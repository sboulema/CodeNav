using CodeNav.OutOfProc.Constants;

namespace CodeNav.OutOfProc.Mappers;

public static class FilterRuleMapper
{
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
                or CodeItemKindEnum.ImplementedInterface
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

    public static bool MapBasedOnHideIgnore(bool hide, bool ignore)
        => !(hide || ignore);
}
