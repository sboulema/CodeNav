using System;
using System.Windows.Forms;
using CodeNav.Properties;
using CodeNav.Models;
using CodeNav.Helpers;

namespace CodeNav.Windows
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
            Settings.Default.MarginSide = (MarginSideEnum)Enum.Parse(typeof(MarginSideEnum), marginSideComboBox.Text);
            Settings.Default.Font = fontDialog1.Font;
            Settings.Default.ShowFilterToolbar = filterToolbarCheckBox.Checked;
            Settings.Default.UseXMLComments = xmlCommentsCheckBox.Checked;
            Settings.Default.ShowHistoryIndicators = historyIndicatorCheckBox.Checked;
            Settings.Default.DisableHighlight = disableHighlightCheckBox.Checked;

            int.TryParse(autoLoadLineThresholdTextBox.Text, out var autoLoadLineThreshold);
            Settings.Default.AutoLoadLineThreshold = autoLoadLineThreshold;

            Settings.Default.Save();
            SettingsHelper.Refresh();
            Close();
        }

        private void OptionsToolWindow_Load(object sender, EventArgs e)
        {
            marginSideComboBox.SelectedItem = Settings.Default.MarginSide.ToString();
            fontDialog1.Font = Settings.Default.Font;
            filterToolbarCheckBox.Checked = Settings.Default.ShowFilterToolbar;            
            xmlCommentsCheckBox.Checked = Settings.Default.UseXMLComments;
            historyIndicatorCheckBox.Checked = Settings.Default.ShowHistoryIndicators;
            disableHighlightCheckBox.Checked = Settings.Default.DisableHighlight;
            autoLoadLineThresholdTextBox.Text = Settings.Default.AutoLoadLineThreshold.ToString();

            highlightBackgroundButton.BackColor = Settings.Default.HighlightBackgroundColor;
            windowBackgroundButton.BackColor = Settings.Default.WindowBackgroundColor;
        }

        private void fontButton_Click(object sender, EventArgs e)
        {           
            fontDialog1.ShowDialog();
        }

        private void highlightBackgroundButton_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                Settings.Default.HighlightBackgroundColor = colorDialog1.Color;
                highlightBackgroundButton.BackColor = colorDialog1.Color;
            }
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            Settings.Default.Reset();
            SettingsHelper.Refresh();
            Close();
        }

        private void WindowBackgroundButton_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                Settings.Default.WindowBackgroundColor = colorDialog1.Color;
                windowBackgroundButton.BackColor = colorDialog1.Color;
            }
        }
    }
}
