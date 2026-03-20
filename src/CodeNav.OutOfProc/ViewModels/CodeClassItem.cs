using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Interfaces;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;
using System.Windows;

namespace CodeNav.OutOfProc.ViewModels;

[DataContract]
public class CodeClassItem : CodeItem, IMembers, ICodeCollapsible
{
    public CodeClassItem()
    {
        DataTemplateType = "Class";
        ToggleExpandCollapseCommand = new(ToggleExpandCollapse);
    }

    [DataMember]
    public List<CodeItem> Members { get; set; } = [];

    [DataMember]
    public string Parameters { get; set; } = string.Empty;

    public event EventHandler? IsExpandedChanged;
    private bool _isExpanded;

    /// <summary>
    /// Gets or sets a value indicating whether the item is expanded in the tool window.
    /// </summary>
    [DataMember]
    public bool IsExpanded
    {
        get { return _isExpanded; }
        set
        {
            if (_isExpanded != value)
            {
                SetProperty(ref _isExpanded, value);

                IsExpandedChanged?.Invoke(this, EventArgs.Empty);
                
                if (value)
                {
                    _ = OutliningHelper.ExpandOutlineRegion(this);
                }
                else
                {
                    _ = OutliningHelper.CollapseOutlineRegion(this);
                }
            }
        }
    }

    /// <summary>
    /// Do we have any members that are not null and should be visible?
    /// If we don't hide the expander +/- symbol and the header border
    /// </summary>
    [DataMember]
    public Visibility HasMembersVisibility
        => Members.Any(m => m.Visibility == Visibility.Visible)
            ? Visibility.Visible
            : Visibility.Collapsed;

    /// <summary>
    /// Command use to collapse and expand class/region/namespace code items
    /// when double-clicking on the expander header
    /// </summary>
    [DataMember]
    public AsyncCommand ToggleExpandCollapseCommand { get; }
    public async Task ToggleExpandCollapse(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        IsExpanded = !IsExpanded;
    }
}
