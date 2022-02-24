#nullable enable

using CodeNav.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CodeNav.Models
{
    public class CodeClassItem : CodeItem, IMembers, ICodeCollapsible
    {
        public CodeClassItem()
        {
            Members = new List<CodeItem>();
        }

        public List<CodeItem> Members { get; set; }

        public string Parameters { get; set; } = string.Empty;

        private Color _borderColor;
        public Color BorderColor
        {
            get
            {
                return _borderColor;
            }
            set
            {
                SetProperty(ref _borderColor, value);
                NotifyPropertyChanged("BorderBrush");
            }
        }
        public SolidColorBrush BorderBrush
        {
            get
            {
                return ColorHelper.ToBrush(_borderColor);
            }
        }

        public event EventHandler? IsExpandedChanged;
        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    SetProperty(ref _isExpanded, value);   
                    IsExpandedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

		/// <summary>
		/// Do we have any members that are not null and should be visible?
		/// If we don't hide the expander +/- symbol and the header border
		/// </summary>
		public Visibility HasMembersVisibility
        {
            get
            {
                return Members.Any(m => m != null && m.IsVisible == Visibility.Visible) 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }
        }
    }
}
