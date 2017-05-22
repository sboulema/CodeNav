using System.Windows.Media;

namespace CodeNav.Helpers
{
    public static class ColorHelper
    {
        public static SolidColorBrush CreateSolidColorBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
