using System.Windows.Media;

namespace CodeNav.Models
{
    public class BookmarkStyle
    {
        public SolidColorBrush Background { get; }
        public SolidColorBrush Foreground { get; }

        public BookmarkStyle(SolidColorBrush background, SolidColorBrush foreground)
        {
            Background = background;
            Foreground = foreground;            
        }
    }
}
