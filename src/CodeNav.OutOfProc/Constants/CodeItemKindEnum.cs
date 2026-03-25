using CodeNav.OutOfProc.Extensions;

namespace CodeNav.OutOfProc.Constants;

public enum CodeItemKindEnum
{
    Unknown,

    All,

    [EnumOrder(2)]
    BaseClass,

    [EnumOrder(3)]
    Class,

    [EnumOrder(13)]
    Constant,

    [EnumOrder(7)]
    Constructor,

    [EnumOrder(21)]
    Delegate,

    [EnumOrder(8)]
    Enum,

    [EnumOrder(9)]
    EnumMember,

    [EnumOrder(16)]
    Event,

    [EnumOrder(10)]
    ImplementedInterface,

    [EnumOrder(20)]
    Indexer,

    [EnumOrder(11)]
    Interface,

    [EnumOrder(4)]
    LocalFunction,

    [EnumOrder(5)]
    Method,

    [EnumOrder(1)]
    Namespace,

    [EnumOrder(6)]
    Property,

    [EnumOrder(14)]
    Record,

    [EnumOrder(19)]
    Region,

    [EnumOrder(15)]
    Struct,

    [EnumOrder(17)]
    Switch,

    [EnumOrder(18)]
    SwitchSection,

    [EnumOrder(12)]
    Variable,
}
