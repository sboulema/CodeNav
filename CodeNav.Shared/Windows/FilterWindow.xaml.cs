#nullable enable

using CodeNav.Helpers;
using CodeNav.Models;
using CodeNav.Shared.ViewModels;
using Microsoft.VisualStudio.PlatformUI;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CodeNav.Windows
{
    public partial class FilterWindow : DialogWindow
    {
        private DataGridCell? _cell;

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

        private FilterWindowViewModel? ViewModel => DataContext as FilterWindowViewModel;

        private void OkClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            SettingsHelper.SaveFilterRules(ViewModel.FilterRules);

            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            ViewModel.FilterRules.Add(new FilterRule());
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            if (_cell == null ||
                ViewModel == null ||
                !(_cell.DataContext is FilterRule filterRule))
            {
                return;
            }

            ViewModel.FilterRules.Remove(filterRule);
        }

        private void DataGrid_Selected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource.GetType() != typeof(DataGridCell) ||
                !(sender is DataGrid grid))
            {
                return;
            }

            grid.BeginEdit(e);

            var cell = e.OriginalSource as DataGridCell;

            _cell = cell;

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

        private void DataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is DataGrid grid))
            {
                return;
            }

            grid.CancelEdit(DataGridEditingUnit.Row);
        }
    }
}