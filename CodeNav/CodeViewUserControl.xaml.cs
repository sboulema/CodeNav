using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CodeNav.Helpers;
using CodeNav.Models;
using EnvDTE;

namespace CodeNav
{
    /// <summary>
    /// Interaction logic for CodeViewUserControl.xaml
    /// </summary>
    public partial class CodeViewUserControl
    {
        private readonly DTE _dte;
        private readonly CodeNav _codeNav;

        public CodeViewUserControl(DTE dte, CodeNav codeNav)
        {
            InitializeComponent();
            _dte = dte;
            _codeNav = codeNav;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox) sender;
            var selectedItem = (CodeItem) listBox?.SelectedItem;

            if (selectedItem == null) return;

            if (selectedItem.StartPoint == null)
            {
                LogHelper.Log($"{selectedItem.FullName} has no StartPoint");
                return;
            }

            var textSelection = _dte.ActiveDocument.Selection as TextSelection;
            if (textSelection == null)
            {
                LogHelper.Log($"TextSelection is null for {_dte.ActiveDocument.FullName}");
                return;
            }
                  
            textSelection.MoveToPoint(selectedItem.StartPoint);
            listBox.UnselectAll();
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
            _codeNav.RegisterEvents();
            _codeNav.UpdateDocument(_dte?.ActiveDocument?.ActiveWindow);
        }

        private void ButtonSortByFileOrder_OnClick(object sender, RoutedEventArgs e)
        {
            _codeNav._codeDocumentVm.CodeDocument = SortHelper.SortByFile(_codeNav._codeDocumentVm.CodeDocument);
        }

        private void ButtonSortByName_OnClick(object sender, RoutedEventArgs e)
        {
            _codeNav._codeDocumentVm.CodeDocument = SortHelper.SortByName(_codeNav._codeDocumentVm.CodeDocument);
        }

        private void ButtonFilter_OnClick(object sender, RoutedEventArgs e)
        {
            new FilterToolWindow().ShowDialog();
        }
    }
}
