using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CodeNav.Helpers;
using CodeNav.Models;
using CodeNav.Properties;
using CodeNav.Windows;

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

            ButtonSortByName.IsChecked = Settings.Default.SortOrder == SortOrderEnum.SortByName;
            ButtonSortByFile.IsChecked = Settings.Default.SortOrder == SortOrderEnum.SortByFile;
        }

        private void ButtonRefresh_OnClick(object sender, RoutedEventArgs e) => _ = FindParent(this).UpdateDocument(true);

        private void ButtonSortByFileOrder_OnClick(object sender, RoutedEventArgs e) => Sort(SortOrderEnum.SortByFile);

        private void ButtonSortByName_OnClick(object sender, RoutedEventArgs e) => Sort(SortOrderEnum.SortByName);

        private void ButtonOptions_OnClick(object sender, RoutedEventArgs e)
        {
            var control = FindParent(this);
            _ = new OptionsWindow().ShowDialog();
            _ = control.UpdateDocument(true);
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

        private void Sort(SortOrderEnum sortOrder)
        {
            var control = FindParent(this);
            control.CodeDocumentViewModel.SortOrder = sortOrder;
            control.CodeDocumentViewModel.CodeDocument = SortHelper.Sort(control.CodeDocumentViewModel);
            Settings.Default.SortOrder = sortOrder;
            Settings.Default.Save();
        }
    }
}
