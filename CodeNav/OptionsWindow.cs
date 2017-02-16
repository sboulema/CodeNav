using System;
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
            Settings.Default.MarginSide = useLeftSideComboBox.Text;
            Settings.Default.Save();
            Close();
        }

        private void OptionsToolWindow_Load(object sender, EventArgs e)
        {
            useLeftSideComboBox.SelectedItem = Settings.Default.MarginSide;
        }
    }
}
