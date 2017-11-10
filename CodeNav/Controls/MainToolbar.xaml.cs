using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CodeNav.Helpers;
using CodeNav.Models;

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
            LogHelper.Log("Refreshing document");
            var control = FindParent<CodeViewUserControl>(this);
            control.UpdateDocument(true);
        }

        private void ButtonSortByFileOrder_OnClick(object sender, RoutedEventArgs e)
        {
            var control = FindParent<CodeViewUserControl>(this);
            control.CodeDocumentViewModel.SortOrder = SortOrderEnum.SortByFile;
            control.CodeDocumentViewModel.CodeDocument = SortHelper.Sort(control.CodeDocumentViewModel);
        }

        private void ButtonSortByName_OnClick(object sender, RoutedEventArgs e)
        {
            var control = FindParent<CodeViewUserControl>(this);
            control.CodeDocumentViewModel.SortOrder = SortOrderEnum.SortByName;
            control.CodeDocumentViewModel.CodeDocument = SortHelper.Sort(control.CodeDocumentViewModel);
        }

        private void ButtonOptions_OnClick(object sender, RoutedEventArgs e)
        {
            var control = FindParent<CodeViewUserControl>(this);
            new OptionsWindow().ShowDialog();
            control.UpdateDocument(true);
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

        private void ButtonRegion_OnClick(object sender, RoutedEventArgs e)
        {
            FindParent<CodeViewUserControl>(this).ToggleAllRegions(!(sender as ToggleButton).IsChecked.Value);
        }
    }
}
