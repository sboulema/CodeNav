using CodeNav.ViewModels;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;

namespace CodeNav.Dialogs.FilterDialog;

[DataContract]
public class FilterDialogData : NotifyPropertyChangedObject
{
    public FilterDialogData()
    {
        AddFilterCommand = new(AddFilter);
        DeleteFilterCommand = new(DeleteFilter);
    }

    /// <summary>
    /// List of filter rules
    /// </summary>
    [DataMember]
    public ObservableList<FilterRuleViewModel> FilterRules { get; set; } = [new()];

    /// <summary>
    /// Selected filter rule
    /// </summary>
    [DataMember]
    public FilterRuleViewModel? SelectedFilterRule { get; set; }

    [DataMember]
    public AsyncCommand AddFilterCommand { get; }
    private async Task AddFilter(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
        => FilterRules.Add(new());

    [DataMember]
    public AsyncCommand DeleteFilterCommand { get; }
    private async Task DeleteFilter(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        if (SelectedFilterRule == null)
        {
            return;
        }

        FilterRules.Remove(SelectedFilterRule);
    }
}
