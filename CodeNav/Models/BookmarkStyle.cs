using CodeNav.Helpers;
using System.Windows.Media;

namespace CodeNav.Models
{
    public class BookmarkStyle
    {
        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }

        public SolidColorBrush BackgroundBrush {
            get
            {
                return ColorHelper.ToBrush(BackgroundColor);
            }
        }

        public SolidColorBrush ForegroundBrush
        {
            get
            {
                return ColorHelper.ToBrush(ForegroundColor);
            }
        }

        public BookmarkStyle()
        {

        }

        public BookmarkStyle(Color backgroundColor, Color foregroundColor)
        {
            BackgroundColor = backgroundColor;
            ForegroundColor = foregroundColor;            
        }
    }
}
