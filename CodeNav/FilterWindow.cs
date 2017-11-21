using System;
using System.Windows.Forms;
using CodeNav.Properties;
using CodeNav.Models;
using System.Collections.Generic;

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
            if (Settings.Default.FilterRules == null)
            {
                Settings.Default.FilterRules = new List<FilterRule>();
            }

            accessDataGridViewTextBoxColumn.DataSource = Enum.GetValues(typeof(CodeItemAccessEnum));
            kindDataGridViewTextBoxColumn.DataSource = Enum.GetValues(typeof(CodeItemKindEnum));
            filterRulesDataGridView.DataSource = new BindingSource { DataSource = Settings.Default.FilterRules };
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Settings.Default.Save();
            Close();
        }
    }
}
