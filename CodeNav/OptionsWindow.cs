using System;
using System.Drawing;
using System.Windows.Forms;
using CodeNav.Properties;

namespace CodeNav
{
    public partial class OptionsWindow : Form
    {
        public OptionsWindow()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Settings.Default.MarginSide = marginSideComboBox.Text;
            Settings.Default.Font = fontDialog1.Font;
            Settings.Default.ShowFilterToolbar = filterToolbarCheckBox.Checked;
            Settings.Default.Save();
            Close();
        }

        private void OptionsToolWindow_Load(object sender, EventArgs e)
        {
            marginSideComboBox.SelectedItem = Settings.Default.MarginSide;
            fontDialog1.Font = Settings.Default.Font;
            filterToolbarCheckBox.Checked = Settings.Default.ShowFilterToolbar;
        }

        private void fontButton_Click(object sender, EventArgs e)
        {           
            fontDialog1.ShowDialog();
        }
    }
}
