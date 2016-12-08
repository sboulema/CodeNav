using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public CodeViewUserControl(DTE dte)
        {
            InitializeComponent();
            _dte = dte;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox) sender).SelectedItem == null) return;           
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
    }
}
