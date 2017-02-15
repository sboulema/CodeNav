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

        public CodeViewUserControl(Window window)
        {
            InitializeComponent();
            _window = window;

            _backgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
            _backgroundWorker.DoWork += BackgroundWorker_DoWork;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        public void SetWindow(Window window)
        {
            _window = window;
        }

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

        private void ButtonRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateDocument();
        }

        private void ButtonSortByFileOrder_OnClick(object sender, RoutedEventArgs e)
        {
            DataContext = new CodeDocumentViewModel
            {
                CodeDocument = SortHelper.SortByFile((DataContext as CodeDocumentViewModel).CodeDocument)
            };
        }

        private void ButtonSortByName_OnClick(object sender, RoutedEventArgs e)
        {
            DataContext = new CodeDocumentViewModel
            {
                CodeDocument = SortHelper.SortByName((DataContext as CodeDocumentViewModel).CodeDocument)
            };
        }

        private void ButtonFilter_OnClick(object sender, RoutedEventArgs e)
        {
            new FilterToolWindow().ShowDialog();

            DataContext = new CodeDocumentViewModel
            {
                CodeDocument = VisibilityHelper.SetCodeItemVisibility((DataContext as CodeDocumentViewModel).CodeDocument)
            };
        }

        private void ButtonOptions_OnClick(object sender, RoutedEventArgs e)
        {
            new OptionsToolWindow().ShowDialog();
        }

        public void UpdateDocument()
        {
            // Do we have code items in the text document
            var elements = _window.ProjectItem?.FileCodeModel?.CodeElements;
            if (elements == null) return;

            // Do we have a cached version of this document
            if (_cache != null)
            {
                DataContext = new CodeDocumentViewModel { CodeDocument = _cache };
            }

            // If not show a loading item
            if ((DataContext as CodeDocumentViewModel)?.CodeDocument == null)
            {
                DataContext = new CodeDocumentViewModel { CodeDocument = CreateLoadingItem() };
            }

            // Is the backgroundworker already doing something, if so stop it
            if (_backgroundWorker.IsBusy)
            {
                _backgroundWorker.CancelAsync();
            }

            // Start the backgroundworker to update the list of code items
            if (!_backgroundWorker.CancellationPending)
            {
                _backgroundWorker.RunWorkerAsync(elements);
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
                    IconPath = "Icons/Refresh/Refresh_16x.xaml"
                }
            };
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var areEqual = (DataContext as CodeDocumentViewModel)?.CodeDocument.SequenceEqual((List<CodeItem>)e.Result, new CodeItemComparer());
            if (areEqual ?? false)
            {
                stopwatch.Stop();
                LogHelper.Log($"RunWorkerCompleted in {stopwatch.ElapsedMilliseconds} ms, document did not change");
                return;
            }

            var codeItems = (List<CodeItem>)e.Result;
            codeItems.RemoveAll(item => item == null);
            DataContext = new CodeDocumentViewModel { CodeDocument = codeItems };
            _cache = (List<CodeItem>)e.Result;

            VisibilityHelper.SetControlVisibility(null, !(DataContext as CodeDocumentViewModel).CodeDocument.Any());
            HighlightHelper.SetForeground((DataContext as CodeDocumentViewModel).CodeDocument);

            stopwatch.Stop();
            LogHelper.Log($"RunWorkerCompleted in {stopwatch.ElapsedMilliseconds} ms");
        }

        private static void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = CodeItemMapper.MapDocument((CodeElements)e.Argument);
        }
    }
}
