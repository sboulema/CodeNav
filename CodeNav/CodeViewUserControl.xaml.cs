using System.Windows.Controls;
using System.Windows.Navigation;
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
    }
}
