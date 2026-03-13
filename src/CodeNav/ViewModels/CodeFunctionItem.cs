using System.Runtime.Serialization;

namespace CodeNav.ViewModels;

[DataContract]
public class CodeFunctionItem : CodeItem
{
    [DataMember]
    public string Parameters { get; set; } = string.Empty;

    [DataMember]
    public string ReturnType { get; set; } = string.Empty;
}
