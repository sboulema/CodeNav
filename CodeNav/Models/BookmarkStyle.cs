using System.Windows.Media;

namespace CodeNav.Models
{
    public class BookmarkStyle
    {
        public SolidColorBrush Background { get; set; }
        public SolidColorBrush Foreground { get; set; }

        public BookmarkStyle()
        {

        }

        public BookmarkStyle(SolidColorBrush background, SolidColorBrush foreground)
        {
            Background = background;
            Foreground = foreground;            
        }

        public BookmarkStyle(Color backgroundColor, Color foregroundColor)
        {
            Background = new SolidColorBrush(backgroundColor);
            Foreground = new SolidColorBrush(foregroundColor);
        }
    }
}
