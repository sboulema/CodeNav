using System.Windows;

namespace CodeNav.Mappers
{
    public static class FontStyleMapper
    {
        public static FontStyle Map(System.Drawing.FontStyle fontStyle)
        {
            switch (fontStyle)
            {
                case System.Drawing.FontStyle.Italic:
                    return FontStyles.Italic;
                default:
                    return FontStyles.Normal;
            }
        }
    }
}
