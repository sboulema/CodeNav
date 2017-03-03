using System;
using System.Collections.Generic;

namespace CodeNav.Models
{
    public class CodeNamespaceItem : CodeItem, IMembers
    {
        public CodeNamespaceItem()
        {
            Members = new List<CodeItem>();
        }

        public List<CodeItem> Members { get; set; }

        public event EventHandler IsExpandedChanged;
        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
