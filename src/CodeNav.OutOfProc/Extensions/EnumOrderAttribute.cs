namespace CodeNav.OutOfProc.Extensions;

[AttributeUsage(AttributeTargets.Field)]
public class EnumOrderAttribute : Attribute
{
    public EnumOrderAttribute(int order)
    {
        Order = order;
    }

    public int Order { get; set; }
}
