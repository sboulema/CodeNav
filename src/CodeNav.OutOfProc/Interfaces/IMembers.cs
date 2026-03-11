using CodeNav.OutOfProc.ViewModels;

namespace CodeNav.OutOfProc.Interfaces;

public interface IMembers
{
    List<CodeItem> Members { get; set; }

    bool IsExpanded { get; set; }
}
