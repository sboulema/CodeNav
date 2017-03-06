using System.ComponentModel;

namespace CodeNav.Models
{
    public enum CodeItemAccessEnum
    {
        [Description("")]
        Unknown,
        [Description("")]
        Public,
        [Description("Private")]
        Private,
        [Description("Protect")]
        Protected,
        [Description("Friend")]
        Internal,
        [Description("Sealed")]
        Sealed
    }
}
