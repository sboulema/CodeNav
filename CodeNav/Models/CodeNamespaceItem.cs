using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CodeNav.Models
{
    public class CodeNamespaceItem : CodeClassItem
    {
        public CodeNamespaceItem()
        {
            Members = new List<CodeItem>();
        }

        public Orientation Orientation { get; set; }

        public Visibility IgnoreVisibility { get; set; }

        public Visibility NotIgnoreVisibility
        {
            get
            {
                return IgnoreVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}
