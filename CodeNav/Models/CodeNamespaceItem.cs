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

        #pragma warning disable CS0067
        public event EventHandler IsExpandedChanged;
        #pragma warning restore

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
