using CodeNav.Helpers;
using CodeNav.Models;
using CodeNav.Shared.ViewModels;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System.Windows;
using System.Windows.Forms;

namespace CodeNav.Windows
{
    public partial class BookmarkStylesWindow : DialogWindow
    {
        private readonly CodeDocumentViewModel _codeDocumentViewModel;

        public BookmarkStylesWindow(CodeDocumentViewModel codeDocumentViewModel)
        {
            _codeDocumentViewModel = codeDocumentViewModel;

            InitializeComponent();

            Loaded += BookmarkStylesWindow_Loaded;
        }

        private async void BookmarkStylesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new BookmarkStylesWindowViewModel
            {
                BookmarkStyles = await BookmarkHelper.GetBookmarkStyles(_codeDocumentViewModel)
            };
        }

        private BookmarkStylesWindowViewModel ViewModel => DataContext as BookmarkStylesWindowViewModel;

        private void BackgroundClick(object sender, RoutedEventArgs e)
        {
            var colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.SelectedBookmarkStyle.BackgroundColor = ColorHelper.ToMediaColor(colorDialog.Color);
            }
        }

        private void ForegroundClick(object sender, RoutedEventArgs e)
        {
            var colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.SelectedBookmarkStyle.ForegroundColor = ColorHelper.ToMediaColor(colorDialog.Color);
            }
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            BookmarkHelper.SetBookmarkStyles(_codeDocumentViewModel, ViewModel.BookmarkStyles).FireAndForget();

            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}