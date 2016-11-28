namespace CodeNav.Models
{
    public class CodeItem
    {
        public string Name;
        public int Codepoint;
        public ModifierEnum Modifier;
    }

    public enum ModifierEnum
    {
        Unkown,
        Public,
        Private,
        Protected
    }
}
