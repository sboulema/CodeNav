using System.Windows;
using System.Windows.Media;
using CodeNav.Helpers;

namespace CodeNav.Controls
{
    /// <summary>
    /// Interaction logic for MainToolbar.xaml
    /// </summary>
    public partial class MainToolbar
    {
        public MainToolbar()
        {
            InitializeComponent();
        }

        private void ButtonRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            var control = FindParent<CodeViewUserControl>(this);
            LogHelper.Log("Refreshing document");
            control.UpdateDocument(true);
        }

        private void ButtonSortByFileOrder_OnClick(object sender, RoutedEventArgs e)
        {
            var control = FindParent<CodeViewUserControl>(this);
            control.CodeDocumentViewModel.CodeDocument = 
                SortHelper.SortByFile(control.CodeDocumentViewModel.CodeDocument);
        }

        private void ButtonSortByName_OnClick(object sender, RoutedEventArgs e)
        {
            var control = FindParent<CodeViewUserControl>(this);
            control.CodeDocumentViewModel.CodeDocument = SortHelper.SortByName(control.CodeDocumentViewModel.CodeDocument);
        }

        private void ButtonOptions_OnClick(object sender, RoutedEventArgs e)
        {
            new OptionsWindow().ShowDialog();
            FindParent<CodeViewUserControl>(this).UpdateDocument(true);
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
