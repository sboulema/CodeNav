using System;
using System.Collections.Generic;

namespace CodeNav.Models
{
    public interface IMembers
    {
        List<CodeItem> Members { get; set; }
        event EventHandler IsExpandedChanged;
        bool IsExpanded { get; set; }
    }
}
