namespace CodeNav.Models
{
    public class FilterRule
    {
        public CodeItemAccessEnum Access { get; set; }

        public CodeItemKindEnum Kind { get; set; }

        public bool Visible { get; set; }

        public string Opacity { get; set; }

        public bool HideIfEmpty { get; set; }

        public bool Ignore { get; set; }
    }
}
