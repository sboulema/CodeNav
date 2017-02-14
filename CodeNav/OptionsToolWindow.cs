using System;
using System.Windows.Forms;
using CodeNav.Properties;

namespace CodeNav
{
    public partial class OptionsToolWindow : Form
    {
        public OptionsToolWindow()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Settings.Default.UseLeftSide = useLeftSideComboBox.Text.Equals("Left");
            Settings.Default.Save();
            Close();
        }

        private void OptionsToolWindow_Load(object sender, EventArgs e)
        {
            useLeftSideComboBox.SelectedItem = Settings.Default.UseLeftSide ? useLeftSideComboBox.Items[0] : useLeftSideComboBox.Items[1];
        }
    }
}
