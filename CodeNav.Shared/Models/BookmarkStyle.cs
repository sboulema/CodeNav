using CodeNav.Helpers;
using Microsoft.VisualStudio.PlatformUI;
using System.Windows.Media;

namespace CodeNav.Models
{
    public class BookmarkStyle : ObservableObject
    {
        private Color _backgroundColor;

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                SetProperty(ref _backgroundColor, value);
                NotifyPropertyChanged("BackgroundBrush");
            }
        }

        private Color _foregroundColor;

        public Color ForegroundColor
        {
            get => _foregroundColor;
            set
            {
                SetProperty(ref _foregroundColor, value);
                NotifyPropertyChanged("ForegroundBrush");
            }
        }

        public SolidColorBrush BackgroundBrush => ColorHelper.ToBrush(BackgroundColor);

        public SolidColorBrush ForegroundBrush => ColorHelper.ToBrush(ForegroundColor);

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
