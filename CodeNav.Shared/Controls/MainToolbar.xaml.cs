#nullable enable

using System.Windows;
using System.Windows.Controls.Primitives;
using CodeNav.Helpers;
using CodeNav.Models;
using CodeNav.Windows;
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

        private void ButtonRefresh_OnClick(object sender, RoutedEventArgs e)
            => WpfHelper.FindParent(this)?.UpdateDocument(force: true);

        private void ButtonSortByFileOrder_OnClick(object sender, RoutedEventArgs e)
            => Sort(SortOrderEnum.SortByFile).FireAndForget();

        private void ButtonSortByName_OnClick(object sender, RoutedEventArgs e)
            => Sort(SortOrderEnum.SortByName).FireAndForget();

        private void ButtonOptions_OnClick(object sender, RoutedEventArgs e)
        {
            new OptionsWindow().ShowDialog();

            var control = WpfHelper.FindParent(this);
            control?.UpdateDocument();
            control?.RegisterDocumentEvents().FireAndForget();
        }

        private void ButtonRegion_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleButton toggleButton))
            {
                return;
            }

            WpfHelper.FindParent<CodeViewUserControl>(this)?.ToggleAll(toggleButton.IsChecked == false);
        }

        private async Task Sort(SortOrderEnum sortOrder)
        {
            var control = WpfHelper.FindParent(this);

            if (control == null)
            {
                return;
            }

            control.CodeDocumentViewModel.SortOrder = sortOrder;
            control.CodeDocumentViewModel.CodeDocument = SortHelper.Sort(control.CodeDocumentViewModel);

            var general = await General.GetLiveInstanceAsync();
            general.SortOrder = (int)sortOrder;
            await general.SaveAsync();
        }
    }
}
