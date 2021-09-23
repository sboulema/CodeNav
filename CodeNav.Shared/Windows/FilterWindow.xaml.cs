using CodeNav.Helpers;
using CodeNav.Shared.ViewModels;
using Microsoft.VisualStudio.PlatformUI;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CodeNav.Windows
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

        private void DataGrid_Selected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(DataGridCell))
            {
                var grid = sender as DataGrid;
                grid.BeginEdit(e);

                var cell = e.OriginalSource as DataGridCell;

                var comboBox = WpfHelper
                    .FindChildrenByType<ComboBox>(cell)
                    .FirstOrDefault();

                if (comboBox != null)
                {
                    comboBox.IsDropDownOpen = true;
                }

                var checkBox = WpfHelper
                    .FindChildrenByType<CheckBox>(cell)
                    .FirstOrDefault();

                if (checkBox != null)
                {
                    checkBox.IsChecked = !checkBox.IsChecked;
                }

                var textBox = WpfHelper
                    .FindChildrenByType<TextBox>(cell)
                    .FirstOrDefault();

                if (textBox != null)
                {
                    textBox.SelectAll();
                }
            }
        }
    }
}