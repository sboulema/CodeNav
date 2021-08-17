using CodeNav.Helpers;
using CodeNav.Models;
using CodeNav.Shared.ViewModels;
using Microsoft.VisualStudio.PlatformUI;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

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
                MarginSide = (MarginSideEnum)General.Instance.MarginSide,
                ShowFilterToolbar = General.Instance.ShowFilterToolbar,
                UseXMLComments = General.Instance.UseXMLComments,
                ShowHistoryIndicators = General.Instance.ShowHistoryIndicators,
                DisableHighlight = General.Instance.DisableHighlight,
                AutoLoadLineThreshold = General.Instance.AutoLoadLineThreshold,
                Font = General.Instance.Font,
                BackgroundColor = General.Instance.BackgroundColor,
                HighlightColor = General.Instance.HighlightColor
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
            General.Instance.MarginSide = (int)ViewModel.MarginSide;
            General.Instance.Font = ViewModel.Font;
            General.Instance.ShowFilterToolbar = ViewModel.ShowFilterToolbar;
            General.Instance.UseXMLComments = ViewModel.UseXMLComments;
            General.Instance.ShowHistoryIndicators = ViewModel.ShowHistoryIndicators;
            General.Instance.DisableHighlight = ViewModel.DisableHighlight;
            General.Instance.AutoLoadLineThreshold = ViewModel.AutoLoadLineThreshold;
            General.Instance.BackgroundColor = ViewModel.BackgroundColor;
            General.Instance.HighlightColor = ViewModel.HighlightColor;

            General.Instance.Save();

            SettingsHelper.Refresh();

            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ResetClick(object sender, RoutedEventArgs e)
        {
            General.Instance.Width = 200;
            General.Instance.MarginSide = (int)MarginSideEnum.Left;
            General.Instance.Font = new Font("Segoe UI", (float)11.25);
            General.Instance.ShowFilterToolbar = true;
            General.Instance.ShowMargin = true;
            General.Instance.FilterRules = string.Empty;
            General.Instance.SortOrder = (int)SortOrderEnum.Unknown;
            General.Instance.HighlightColor = ColorHelper.Transparent();
            General.Instance.UseXMLComments = false;
            General.Instance.ShowHistoryIndicators = true;
            General.Instance.DisableHighlight = false;
            General.Instance.AutoLoadLineThreshold = 0;
            General.Instance.BackgroundColor = ColorHelper.Transparent();
            General.Instance.Save();

            SettingsHelper.Refresh();
            Close();
        }
    }
}
