using System.Runtime.Serialization;

namespace CodeNav.ViewModels;

[DataContract]
public class CodeRegionItem : CodeClassItem
{
    public CodeRegionItem()
    {
        DataTemplateType = "Region";
    }
}
