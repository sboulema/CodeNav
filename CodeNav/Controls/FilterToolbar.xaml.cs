using System.Windows;
using System.Windows.Controls;
using CodeNav.Helpers;
using CodeNav.Models;

namespace CodeNav.Controls
{
    /// <summary>
    /// Interaction logic for FilterToolbar.xaml
    /// </summary>
    public partial class FilterToolbar : UserControl
    {
        public FilterToolbar()
        {
            InitializeComponent();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext == null) return;
            VisibilityHelper.SetCodeItemVisibility((DataContext as CodeDocumentViewModel).CodeDocument, FilterTextBox.Text);
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            FilterTextBox.Text = string.Empty;
        }

        private void ButtonFilter_OnClick(object sender, RoutedEventArgs e)
        {
            new FilterWindow().ShowDialog();
            VisibilityHelper.SetCodeItemVisibility((DataContext as CodeDocumentViewModel).CodeDocument);
        }
    }
}
