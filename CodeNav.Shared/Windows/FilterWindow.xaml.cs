using CodeNav.Helpers;
using CodeNav.Shared.ViewModels;
using Microsoft.VisualStudio.PlatformUI;
using System.Windows;

namespace CodeNav
{
    public partial class FilterWindow : DialogWindow
    {
        public FilterWindow()
        {
            InitializeComponent();

            Loaded += FilterWindow_Loaded;
        }

        private void FilterWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new FilterWindowViewModel
            {
                FilterRules = SettingsHelper.FilterRules
            };
        }

        private FilterWindowViewModel ViewModel => DataContext as FilterWindowViewModel;

        private void OkClick(object sender, RoutedEventArgs e)
        {
            SettingsHelper.SaveFilterRules(ViewModel.FilterRules);

            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}