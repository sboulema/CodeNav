using CodeNav.Models;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.ObjectModel;

namespace CodeNav.Shared.ViewModels
{
    public class FilterWindowViewModel : ObservableObject
    {
        private ObservableCollection<FilterRule> _filterRules = new ObservableCollection<FilterRule>();

        public ObservableCollection<FilterRule> FilterRules
        {
            get => _filterRules;
            set => SetProperty(ref _filterRules, value);
        }
    }
}
