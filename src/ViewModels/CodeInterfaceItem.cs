using System.Runtime.Serialization;

namespace CodeNav.ViewModels;

[DataContract]
public class CodeInterfaceItem : CodeClassItem
{
}

[DataContract]
public class CodeImplementedInterfaceItem : CodeRegionItem
{
}
