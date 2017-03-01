using System;
using System.Windows;
using System.Windows.Controls;
using CodeNav.Helpers;
using CodeNav.Models;

namespace CodeNav.Controls
{
    /// <summary>
    /// Interaction logic for FilterToolbar.xaml
    /// </summary>
    public partial class FilterToolbar
    {
        public FilterToolbar()
        {
            InitializeComponent();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var dataContext = DataContext as CodeDocumentViewModel;

            if (dataContext == null)
            {
                LogHelper.Log("Datacontext error while filtering items by name");
                return;
            }

            try
            {                
                VisibilityHelper.SetCodeItemVisibility(dataContext.CodeDocument, FilterTextBox.Text);
            }
            catch (Exception exception)
            {
                LogHelper.Log($"Error filtering items: {exception}");
            }           
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            FilterTextBox.Text = string.Empty;
        }

        private void ButtonFilter_OnClick(object sender, RoutedEventArgs e)
        {
            new FilterWindow().ShowDialog();

            var dataContext = DataContext as CodeDocumentViewModel;

            if (dataContext == null)
            {
                LogHelper.Log("Datacontext error while filtering items by kind");
                return;
            }

            try
            {
                VisibilityHelper.SetCodeItemVisibility(dataContext.CodeDocument);
            }
            catch (Exception exception)
            {
                LogHelper.Log($"Error filtering items: {exception}");
            }            
        }
    }
}
