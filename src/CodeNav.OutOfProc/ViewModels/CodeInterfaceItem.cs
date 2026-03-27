using System.Runtime.Serialization;

namespace CodeNav.OutOfProc.ViewModels;

[DataContract]
public class CodeInterfaceItem : CodeClassItem
{
}

[DataContract]
public class CodeImplementedInterfaceItem : CodeRegionItem, ICloneable
{
    public object Clone() => MemberwiseClone();
}
