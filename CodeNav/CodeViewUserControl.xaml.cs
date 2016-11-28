using System.Windows.Controls;
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
            (_dte.ActiveDocument.Selection as TextSelection).MoveToPoint(((CodeItem)((ListBox)sender).SelectedItem).StartPoint);
        }
    }
}
