using CodeNav.ViewModels;

namespace CodeNav.Interfaces;

public interface IMembers
{
    List<CodeItem> Members { get; set; }

    bool IsExpanded { get; set; }
}
