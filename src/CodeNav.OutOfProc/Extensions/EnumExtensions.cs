using System.ComponentModel;

namespace CodeNav.OutOfProc.Extensions;

public static class EnumExtensions
{
    public static string GetEnumDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        if (field == null)
        {
            return string.Empty;
        }

        return Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is not DescriptionAttribute attribute
            ? value.ToString() : attribute.Description;
    }

    public static int GetEnumOrder(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        if (field == null)
        {
            return 0;
        }

        return Attribute.GetCustomAttribute(field, typeof(EnumOrderAttribute)) is not EnumOrderAttribute attribute
            ? 0 : attribute.Order;
    }
}
