#nullable enable

using CodeNav.Helpers;
using CodeNav.Models;
using Microsoft.VisualStudio.PlatformUI;
using System.Drawing;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace CodeNav.Shared.ViewModels
{
    public class OptionsWindowViewModel : ObservableObject
    {
        private bool _showFilterToolbar = true;

        public bool ShowFilterToolbar
        {
            get => _showFilterToolbar;
            set => SetProperty(ref _showFilterToolbar, value);
        }

        private bool _UseXMLComments = false;

        public bool UseXMLComments
        {
            get => _UseXMLComments;
            set => SetProperty(ref _UseXMLComments, value);
        }

        private bool _showHistoryIndicators = true;

        public bool ShowHistoryIndicators
        {
            get => _showHistoryIndicators;
            set => SetProperty(ref _showHistoryIndicators, value);
        }

        private bool _disableHighlight = false;

        public bool DisableHighlight
        {
            get => _disableHighlight;
            set => SetProperty(ref _disableHighlight, value);
        }

        private bool _updateWhileTyping = false;

        public bool UpdateWhileTyping
        {
            get => _updateWhileTyping;
            set => SetProperty(ref _updateWhileTyping, value);
        }

        private MarginSideEnum _marginSide = MarginSideEnum.Left;

        public MarginSideEnum MarginSide
        {
            get => _marginSide;
            set => SetProperty(ref _marginSide, value);
        }

        private int _autoLoadLineThreshold = 0;

        public int AutoLoadLineThreshold
        {
            get => _autoLoadLineThreshold;
            set => SetProperty(ref _autoLoadLineThreshold, value);
        }

        private Font? _font;

        public Font? Font
        {
            get => _font;
            set => SetProperty(ref _font, value);
        }

        private Color _highlightColor;

        public Color HighlightColor
        {
            get => _highlightColor;
            set => SetProperty(ref _highlightColor, value);
        }

        public SolidColorBrush HighlightBrush => ColorHelper.ToBrush(_highlightColor);

        private Color _backgroundColor;

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }

        public SolidColorBrush BackgroundBrush => ColorHelper.ToBrush(_backgroundColor);
    }
}
