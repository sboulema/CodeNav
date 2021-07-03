using System;
using System.Windows;
using System.Windows.Controls;
using CodeNav.Helpers;
using CodeNav.Models;
using System.Windows.Media;
using System.Threading.Tasks;

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

            var control = FindParent<CodeViewUserControl>(this);
            _ = control.UpdateDocument(forceUpdate: true);
        }

        private void ButtonFilterBookmark_OnClick(object sender, RoutedEventArgs e)
        {
            var control = FindParent<CodeViewUserControl>(this);
            control.CodeDocumentViewModel.FilterOnBookmarks = !control.CodeDocumentViewModel.FilterOnBookmarks;
            control.FilterBookmarks();
        }

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
            if (parentObject is T parent)
            {
                return parent;
            }
            else
            {
                return FindParent<T>(parentObject);
            }
        }
    }
}
