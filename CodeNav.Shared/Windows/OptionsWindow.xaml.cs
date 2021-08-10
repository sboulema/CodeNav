using CodeNav.Helpers;
using CodeNav.Models;
using CodeNav.Properties;
using CodeNav.Shared.ViewModels;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace CodeNav
{
    public partial class OptionsWindow : DialogWindow
    {
        public OptionsWindow()
        {
            InitializeComponent();

            Loaded += OptionsWindow_Loaded;
        }

        private void OptionsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new OptionsWindowViewModel
            {
                MarginSide = Settings.Default.MarginSide,
                ShowFilterToolbar = Settings.Default.ShowFilterToolbar,
                UseXMLComments = Settings.Default.UseXMLComments,
                ShowHistoryIndicators = Settings.Default.ShowHistoryIndicators,
                DisableHighlight = Settings.Default.DisableHighlight,
                AutoLoadLineThreshold = Settings.Default.AutoLoadLineThreshold,
                Font = Settings.Default.Font,
                BackgroundColor = Settings.Default.WindowBackgroundColor,
                HighlightColor = Settings.Default.HighlightBackgroundColor
            };
        }

        private OptionsWindowViewModel ViewModel => DataContext as OptionsWindowViewModel;

        private void ShowFontDialog(object sender, RoutedEventArgs e)
        {
            var fontDialog = new FontDialog();
            if (fontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.Font = fontDialog.Font;
            }
        }

        private void ShowHighlightColorDialog(object sender, RoutedEventArgs e)
        {
            var colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.HighlightColor = colorDialog.Color;
            }
        }

        private void ShowBackgroundColorDialog(object sender, RoutedEventArgs e)
        {
            var colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.BackgroundColor = colorDialog.Color;
            }
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.MarginSide = ViewModel.MarginSide;
            Settings.Default.Font = ViewModel.Font;
            Settings.Default.ShowFilterToolbar = ViewModel.ShowFilterToolbar;
            Settings.Default.UseXMLComments = ViewModel.UseXMLComments;
            Settings.Default.ShowHistoryIndicators = ViewModel.ShowHistoryIndicators;
            Settings.Default.DisableHighlight = ViewModel.DisableHighlight;
            Settings.Default.AutoLoadLineThreshold = ViewModel.AutoLoadLineThreshold;
            Settings.Default.WindowBackgroundColor = ViewModel.BackgroundColor;
            Settings.Default.HighlightBackgroundColor = ViewModel.HighlightColor;

            Settings.Default.Save();

            SettingsHelper.Refresh();

            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ResetClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.Width = 200;
            Settings.Default.MarginSide = MarginSideEnum.Left;
            Settings.Default.Font = new Font("Segoe UI", (float)11.25);
            Settings.Default.ShowFilterToolbar = true;
            Settings.Default.ShowMargin = true;
            Settings.Default.FilterRules = null;
            Settings.Default.SortOrder = SortOrderEnum.Unknown;
            Settings.Default.NewVersionInstalled = true;
            Settings.Default.HighlightBackgroundColor = ColorHelper.Transparent();
            Settings.Default.UseXMLComments = false;
            Settings.Default.ShowHistoryIndicators = true;
            Settings.Default.DisableHighlight = false;
            Settings.Default.AutoLoadLineThreshold = 0;
            Settings.Default.FilterWindowHeight = 270;
            Settings.Default.FilterWindowWidth = 450;
            Settings.Default.FilterWindowLeft = 0;
            Settings.Default.FilterWindowTop = 0;
            Settings.Default.WindowBackgroundColor = ColorHelper.Transparent();
            Settings.Default.Save();

            SettingsHelper.Refresh();
            Close();
        }
    }
}
