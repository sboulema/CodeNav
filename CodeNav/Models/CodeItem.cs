using System.ComponentModel;
using System.Windows.Media;
using Caliburn.Micro;
using EnvDTE;

namespace CodeNav.Models
{
    public class CodeItem : PropertyChangedBase
    {
        public string Name { get; set; }
        public TextPoint StartPoint { get; set; }
        public string IconPath { get; set; }
        public string Id { get; set; }

        private SolidColorBrush _foreground;
        public SolidColorBrush Foreground
        {
            get
            {
                return _foreground;
            }
            set
            {
                _foreground = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Foreground"));
            }
        }
    }
}
