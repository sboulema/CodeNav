﻿using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CodeNav.Helpers;
using CodeNav.Models;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace CodeNav.Controls
{
    public partial class MainToolbar
    {
        public MainToolbar()
        {
            InitializeComponent();

            ButtonSortByName.IsChecked = (SortOrderEnum)General.Instance.SortOrder == SortOrderEnum.SortByName;
            ButtonSortByFile.IsChecked = (SortOrderEnum)General.Instance.SortOrder == SortOrderEnum.SortByFile;
        }

        private void ButtonRefresh_OnClick(object sender, RoutedEventArgs e) => FindParent(this).UpdateDocument();

        private void ButtonSortByFileOrder_OnClick(object sender, RoutedEventArgs e) => Sort(SortOrderEnum.SortByFile).FireAndForget();

        private void ButtonSortByName_OnClick(object sender, RoutedEventArgs e) => Sort(SortOrderEnum.SortByName).FireAndForget();

        private void ButtonOptions_OnClick(object sender, RoutedEventArgs e)
        {
            new OptionsWindow().ShowDialog();

            var control = FindParent(this);
            control.UpdateDocument();
        }

        private static ICodeViewUserControl FindParent(DependencyObject child)
            => FindParent<CodeViewUserControl>(child) ??
                (ICodeViewUserControl)FindParent<CodeViewUserControlTop>(child);

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            // Get parent item
            var parentObject = VisualTreeHelper.GetParent(child);

            // We’ve reached the end of the tree
            if (parentObject == null)
            {
                return null;
            }

            // Check if the parent matches the type we’re looking for
            return parentObject is T parent ? parent : FindParent<T>(parentObject);
        }

        private void ButtonRegion_OnClick(object sender, RoutedEventArgs e)
        {
            FindParent<CodeViewUserControl>(this).ToggleAll(!(sender as ToggleButton).IsChecked.Value);
        }

        private async Task Sort(SortOrderEnum sortOrder)
        {
            var control = FindParent(this);
            control.CodeDocumentViewModel.SortOrder = sortOrder;
            control.CodeDocumentViewModel.CodeDocument = SortHelper.Sort(control.CodeDocumentViewModel);

            var general = await General.GetLiveInstanceAsync();
            general.SortOrder = (int)sortOrder;
            await general.SaveAsync();
        }
    }
}
