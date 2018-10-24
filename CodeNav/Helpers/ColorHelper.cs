using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System.Windows.Media;

namespace CodeNav.Helpers
{
    public static class ColorHelper
    {
        public static SolidColorBrush ToBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        /// <summary>
        /// Convert from VSTheme EnvironmentColor to a XAML SolidColorBrush
        /// </summary>
        /// <param name="key">VSTheme EnvironmentColor key</param>
        /// <returns>XAML SolidColorBrush</returns>
        public static SolidColorBrush ToBrush(ThemeResourceKey key) 
            => new SolidColorBrush(ToMediaColor(VSColorTheme.GetThemedColor(key)));

        public static SolidColorBrush ToBrush(System.Drawing.Color color) 
            => new SolidColorBrush(ToMediaColor(color));

        public static Color ToMediaColor(System.Drawing.Color drawingColor)
            => Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);

        public static Color ToMediaColor(ThemeResourceKey key)
            => ToMediaColor(VSColorTheme.GetThemedColor(key));

        public static System.Drawing.Color ToDrawingColor(Color color)
            => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}
