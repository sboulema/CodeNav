using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CodeNav.Helpers;
using CodeNav.Mappers;
using CodeNav.Models;
using EnvDTE;
using Microsoft.VisualStudio.PlatformUI;
using Window = EnvDTE.Window;

namespace CodeNav
{
    /// <summary>
    /// Interaction logic for CodeViewUserControl.xaml
    /// </summary>
    public partial class CodeViewUserControl
    {
        private Window _window;
        private List<CodeItem> _cache;
        private readonly BackgroundWorker _backgroundWorker;
        internal readonly CodeDocumentViewModel _codeDocumentViewModel;

        public CodeViewUserControl(Window window)
        {
            InitializeComponent();

            // Setup viewmodel as datacontext
            _codeDocumentViewModel = new CodeDocumentViewModel();
            DataContext = _codeDocumentViewModel;

            // Setup backgroundworker to update datacontext
            _backgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
            _backgroundWorker.DoWork += BackgroundWorker_DoWork;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            _window = window;
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }

        public void SetWindow(Window window) => _window = window;

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e) => UpdateDocument(true);

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox) sender;
            var selectedItem = (CodeItem) listBox?.SelectedItem;

            if (selectedItem == null)
            {
                LogHelper.Log("ListBox or SelectedItem is null");
                return;
            }

            if (selectedItem.StartPoint == null)
            {
                LogHelper.Log($"{selectedItem.FullName} has no StartPoint");
                return;
            }

            var textSelection = _window.Document.Selection as TextSelection;
            if (textSelection == null)
            {
                LogHelper.Log($"TextSelection is null for {_window.Document.FullName}");
                return;
            }
                  
            textSelection.MoveToPoint(selectedItem.StartPoint);
            listBox.UnselectAll();
            e.Handled = true;
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        public void UpdateDocument(bool forceUpdate = false)
        {
            // Do we have code items in the text document
            var elements = _window.ProjectItem?.FileCodeModel?.CodeElements;
            if (elements == null) return;

            if (forceUpdate)
            {
                _cache = null;
                DataContext = null;
            }

            // Do we have a cached version of this document
            if (_cache != null)
            {
                _codeDocumentViewModel.CodeDocument = _cache;
            }

            // If not show a loading item
            if (!_codeDocumentViewModel.CodeDocument.Any())
            {
                _codeDocumentViewModel.CodeDocument = CreateLoadingItem();
            }

            // Is the backgroundworker already doing something, if so stop it
            if (_backgroundWorker.IsBusy)
            {
                _backgroundWorker.CancelAsync();
            }

            // Start the backgroundworker to update the list of code items
            if (!_backgroundWorker.CancellationPending)
            {
                _backgroundWorker.RunWorkerAsync(new BackgroundWorkerRequest { Elements = elements, ForceUpdate = forceUpdate });
            }
        }

        private static List<CodeItem> CreateLoadingItem()
        {
            return new List<CodeItem>
            {
                new CodeClassItem
                {
                    Name = "Loading...",
                    FullName = "Loading...",
                    Id = "Loading...",
                    Foreground = new SolidColorBrush(Colors.Black),
                    BorderBrush = new SolidColorBrush(Colors.DarkGray),
                    IconPath = "Icons/UI/Refresh_16x.xaml"
                }
            };
        }

        private static List<CodeItem> CreateSelectDocumentItem()
        {
            return new List<CodeItem>
            {
                new CodeClassItem
                {
                    Name = "Waiting for active code document...",
                    FullName = "Waiting for active code document...",
                    Id = "Waiting for active code document...",
                    Foreground = new SolidColorBrush(Colors.Black),
                    BorderBrush = new SolidColorBrush(Colors.DarkGray),
                    IconPath = "Icons/UI/DocumentOutline_16x.xaml"
                }
            };
        }

        /// <summary>
        /// Show an item to indicate that the user has to select an active code document to inspect
        /// </summary>
        public void ShowWaitingForDocument()
        {
            _codeDocumentViewModel.CodeDocument = CreateSelectDocumentItem();
        }

        public void HighlightCurrentItem()
        {
            _codeDocumentViewModel.CodeDocument =
                HighlightHelper.HighlightCurrentItem(_window, _codeDocumentViewModel.CodeDocument);       
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = e.Result as BackgroundWorkerResult;
            result.CodeItems.RemoveAll(item => item == null);

            // Do we need to update the DataContext?
            var areEqual = AreDocumentsEqual(_codeDocumentViewModel.CodeDocument, result.CodeItems);
            if (result.ForceUpdate == false && areEqual)
            {
                stopwatch.Stop();
                LogHelper.Log($"RunWorkerCompleted in {stopwatch.ElapsedMilliseconds} ms, document did not change");
                return;
            }

            // Set the new list of codeitems as DataContext
            _codeDocumentViewModel.CodeDocument = result.CodeItems;
            _cache = result.CodeItems;

            // Are there any items to show, if not hide the control, if being shown as a margin
            VisibilityHelper.SetControlVisibility(null, !_codeDocumentViewModel.CodeDocument.Any());

            // Set currently active codeitem
            HighlightHelper.SetForeground(_codeDocumentViewModel.CodeDocument);

            stopwatch.Stop();
            LogHelper.Log($"RunWorkerCompleted in {stopwatch.ElapsedMilliseconds} ms");
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!_backgroundWorker.CancellationPending)
            {
                var request = e.Argument as BackgroundWorkerRequest;
                var codeItems = CodeItemMapper.MapDocument(request.Elements);
                e.Result = new BackgroundWorkerResult { CodeItems = codeItems, ForceUpdate = request.ForceUpdate };
            }
        }

        private static bool AreDocumentsEqual(List<CodeItem> existingItems, List<CodeItem> newItems)
        {
            if (existingItems == null || newItems == null) return false;
            return existingItems.SequenceEqual(newItems, new CodeItemComparer());
        }

        public void Dispose()
        {
            if (_backgroundWorker.IsBusy && _backgroundWorker.CancellationPending == false)
            {
                _backgroundWorker.CancelAsync();
            }
        }
    }
}
