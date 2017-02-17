using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

            DataContext = new CodeDocumentViewModel
            {
                CodeDocument = VisibilityHelper.SetCodeItemVisibility((DataContext as CodeDocumentViewModel).CodeDocument, FilterTextBox.Text)
            };
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            FilterTextBox.Text = string.Empty;
        }

        private void ButtonFilter_OnClick(object sender, RoutedEventArgs e)
        {
            new FilterWindow().ShowDialog();

            DataContext = new CodeDocumentViewModel
            {
                CodeDocument = VisibilityHelper.SetCodeItemVisibility((DataContext as CodeDocumentViewModel).CodeDocument)
            };
        }
    }
}
