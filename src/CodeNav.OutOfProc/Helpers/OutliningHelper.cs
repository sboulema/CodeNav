using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Interfaces;
using CodeNav.OutOfProc.ViewModels;

namespace CodeNav.OutOfProc.Helpers;

public static class OutliningHelper
{
    public static void CollapseAll(CodeDocumentViewModel? codeDocumentViewModel)
        => SetIsExpanded(codeDocumentViewModel, isExpanded: false);

    public static void ExpandAll(CodeDocumentViewModel? codeDocumentViewModel)
        => SetIsExpanded(codeDocumentViewModel, isExpanded: true);

    private static void SetIsExpanded(CodeDocumentViewModel? codeDocumentViewModel, bool isExpanded)
    {
        codeDocumentViewModel?
            .CodeItems
            .Flatten()
            .FilterNull()
            .Where(item => item is IMembers)
            .Cast<IMembers>()
            .ToList()
            .ForEach(item => item.IsExpanded = isExpanded);
    }
}
