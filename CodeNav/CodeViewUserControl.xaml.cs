using System.Collections.Generic;
using System.Linq;
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
            if (((CodeItem)((ListBox)sender)?.SelectedItem)?.StartPoint == null) return;           
            (_dte.ActiveDocument.Selection as TextSelection).MoveToPoint(((CodeItem)((ListBox)sender).SelectedItem).StartPoint);
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
    }
}
