using System.Runtime.Serialization;

namespace CodeNav.OutOfProc.ViewModels;

[DataContract]
public class CodeRegionItem : CodeClassItem
{
    public CodeRegionItem()
    {
        DataTemplateType = "Region";
    }
}
