using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CodeNav.Models
{
    public class CodeClassItem : CodeItem, IMembers
    {
        public CodeClassItem()
        {
            Members = new List<CodeItem>();
        }

        public List<CodeItem> Members { get; set; }
        public string Parameters { get; set; }

        private SolidColorBrush _borderBrush;
        public SolidColorBrush BorderBrush
        {
            get
            {
                return _borderBrush;
            }
            set
            {
                _borderBrush = value;
                NotifyOfPropertyChange();
            }
        }

        public Visibility HasMembersVisibility
        {
            get
            {
                return Members.Any(m => m.IsVisible == Visibility.Visible) 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }
        }
    }
}
