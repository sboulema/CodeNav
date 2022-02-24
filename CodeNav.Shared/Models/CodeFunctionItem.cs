#nullable enable

namespace CodeNav.Models
{
    public class CodeFunctionItem : CodeItem
    {
        public string Parameters { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;
    }
}
