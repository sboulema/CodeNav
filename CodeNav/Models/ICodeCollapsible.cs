using System;

namespace CodeNav.Models
{
    public interface ICodeCollapsible
    {
        event EventHandler IsExpandedChanged;
    }
}
