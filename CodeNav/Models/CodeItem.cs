using EnvDTE;

namespace CodeNav.Models
{
    public class CodeItem
    {
        public string Name { get; set; }
        public TextPoint StartPoint { get; set; }
        public ModifierEnum Modifier { get; set; }
    }

    public enum ModifierEnum
    {
        Unkown,
        Public,
        Private,
        Protected
    }
}
