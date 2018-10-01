using System;
using System.Windows;
using System.Windows.Controls;
using CodeNav.Helpers;
using CodeNav.Models;
using System.Windows.Media;
using System.Linq;

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

            var control = FindParent<CodeViewUserControl>(this);
            control.UpdateDocument(true);         
        }

        private void ButtonFilterBookmark_OnClick(object sender, RoutedEventArgs e)
        {
            var control = FindParent<CodeViewUserControl>(this);
            control.CodeDocumentViewModel.FilterOnBookmarks = !control.CodeDocumentViewModel.FilterOnBookmarks;
            control.FilterBookmarks();
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);    //we’ve reached the end of the tree
            if (parentObject == null) return null;
            //check if the parent matches the type we’re looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }
    }
}
