using System.Runtime.Serialization;
using System.Windows;

namespace CodeNav.OutOfProc.ViewModels;

[DataContract]
public class CodeNamespaceItem : CodeClassItem
{
    public CodeNamespaceItem()
    {
        DataTemplateType = "Namespace";
    }

    private Visibility _ignoreVisibility = Visibility.Visible;

    /// <summary>
    /// Used in showing the namespace as Expander with members or only as a list of members
    /// </summary>
    [DataMember]
    public Visibility IgnoreVisibility
    {
        get => _ignoreVisibility;
        set
        {
            SetProperty(ref _ignoreVisibility, value);
            RaiseNotifyPropertyChangedEvent(nameof(NotIgnoreVisibility));
        }
    }

    /// <summary>
    /// Used in showing the namespace as Expander with members or only as a list of members
    /// </summary>
    [DataMember]
    public Visibility NotIgnoreVisibility
        => IgnoreVisibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
}
