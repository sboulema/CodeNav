#nullable enable

using CodeNav.Helpers;
using CodeNav.Models;
using CodeNav.Shared.ViewModels;
using Microsoft.VisualStudio.PlatformUI;
using System.Windows;
using System.Windows.Forms;

namespace CodeNav.Windows
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
                UpdateWhileTyping = General.Instance.UpdateWhileTyping,
                Font = SettingsHelper.Font,
                BackgroundColor = General.Instance.BackgroundColor,
                HighlightColor = General.Instance.HighlightColor
            };
        }

        private OptionsWindowViewModel? ViewModel => DataContext as OptionsWindowViewModel;

        private void ShowFontDialog(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            var fontDialog = new FontDialog
            {
                Font = ViewModel.Font
            };

            if (fontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.Font = fontDialog.Font;
            }
        }

        private void ShowHighlightColorDialog(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            var colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.HighlightColor = colorDialog.Color;
            }
        }

        private void ShowBackgroundColorDialog(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            var colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.BackgroundColor = colorDialog.Color;
            }
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null ||
                ViewModel.Font == null)
            {
                return;
            }

            General.Instance.MarginSide = (int)ViewModel.MarginSide;
            General.Instance.FontFamilyName = ViewModel.Font.FontFamily.Name;
            General.Instance.FontSize = ViewModel.Font.Size;
            General.Instance.FontStyle = ViewModel.Font.Style;
            General.Instance.ShowFilterToolbar = ViewModel.ShowFilterToolbar;
            General.Instance.UseXMLComments = ViewModel.UseXMLComments;
            General.Instance.ShowHistoryIndicators = ViewModel.ShowHistoryIndicators;
            General.Instance.DisableHighlight = ViewModel.DisableHighlight;
            General.Instance.UpdateWhileTyping = ViewModel.UpdateWhileTyping;
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
            General.Instance.FontFamilyName = "Segoe UI";
            General.Instance.FontSize = 11.25f;
            General.Instance.FontStyle = System.Drawing.FontStyle.Regular;
            General.Instance.ShowFilterToolbar = true;
            General.Instance.ShowMargin = true;
            General.Instance.FilterRules = string.Empty;
            General.Instance.SortOrder = (int)SortOrderEnum.Unknown;
            General.Instance.HighlightColor = ColorHelper.Transparent();
            General.Instance.UseXMLComments = false;
            General.Instance.ShowHistoryIndicators = true;
            General.Instance.DisableHighlight = false;
            General.Instance.UpdateWhileTyping = false;
            General.Instance.AutoLoadLineThreshold = 0;
            General.Instance.BackgroundColor = ColorHelper.Transparent();
            General.Instance.Save();

            SettingsHelper.Refresh();
            Close();
        }
    }
}
