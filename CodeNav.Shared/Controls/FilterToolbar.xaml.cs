#nullable enable

using System;
using System.Windows;
using System.Windows.Controls;
using CodeNav.Helpers;
using CodeNav.Models.ViewModels;
using CodeNav.Windows;

namespace CodeNav.Controls
{
    public partial class FilterToolbar
    {
        public FilterToolbar()
        {
            InitializeComponent();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(DataContext is CodeDocumentViewModel dataContext))
            {
                // Datacontext error while filtering items by name
                return;
            }

            dataContext.FilterText = FilterTextBox.Text;

            try
            {
                VisibilityHelper.SetCodeItemVisibility(dataContext);
            }
            catch (Exception)
            {
                // Error filtering items
            }
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            FilterTextBox.Text = string.Empty;
        }

        private void ButtonFilter_OnClick(object sender, RoutedEventArgs e)
        {
            new FilterWindow().ShowDialog();

            var control = WpfHelper.FindParent<CodeViewUserControl>(this);
            control?.UpdateDocument();
        }

        private void ButtonFilterBookmark_OnClick(object sender, RoutedEventArgs e)
        {
            var control = WpfHelper.FindParent<CodeViewUserControl>(this);

            if (control == null)
            {
                return;
            }

            control.CodeDocumentViewModel.FilterOnBookmarks = !control.CodeDocumentViewModel.FilterOnBookmarks;
            control.FilterBookmarks();
        }
    }
}
