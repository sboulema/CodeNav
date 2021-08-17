using System;
using System.Windows.Forms;
using CodeNav.Models;
using CodeNav.Helpers;

namespace CodeNav
{
    public partial class FilterWindow : Form
    {
        public FilterWindow()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FilterToolWindow_Load(object sender, EventArgs e)
        {
            accessDataGridViewTextBoxColumn.DataSource = Enum.GetValues(typeof(CodeItemAccessEnum));
            kindDataGridViewTextBoxColumn.DataSource = Enum.GetValues(typeof(CodeItemKindEnum));
            filterRulesDataGridView.DataSource = new BindingSource { DataSource = SettingsHelper.FilterRules };

            Height = General.Instance.FilterWindowHeight;
            Width = General.Instance.FilterWindowWidth;
            Left = General.Instance.FilterWindowLeft;
            Top = General.Instance.FilterWindowTop;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            SettingsHelper.SaveFilterRules();

            General.Instance.FilterWindowHeight = Height;
            General.Instance.FilterWindowWidth = Width;
            General.Instance.FilterWindowLeft = Left;
            General.Instance.FilterWindowTop = Top;
            General.Instance.Save();
            Close();
        }
    }
}
