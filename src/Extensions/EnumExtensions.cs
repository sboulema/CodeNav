using System.ComponentModel;

namespace CodeNav.Extensions;

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
}
