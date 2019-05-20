using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CodeNav.Helpers
{
    public class NavBarOverrider
    {
        private readonly UIElement _control;
        private readonly IWpfTextView _wpfTextView;

        public NavBarOverrider(CodeNavMargin margin)
        {
            _control = margin._control as UIElement;
            _wpfTextView = margin._textView;
            _wpfTextView.VisualElement.Loaded += FindNaviBar;
            _wpfTextView.VisualElement.Unloaded += ViewUnloaded;
        }

        void FindNaviBar(object sender, RoutedEventArgs e)
        {
            var view = sender as FrameworkElement;

            var naviBar = view
                ?.GetParent<Border>(b => b.Name == "PART_ContentPanel")
                ?.GetFirstVisualChild<Border>(b => b.Name == "DropDownBarMargin");

            if (naviBar == null)
            {
                UnloadEvents();
            }

            var dropDown0 = naviBar.GetFirstVisualChild<ComboBox>(c => c.Name == "DropDown0");
            var dropDown1 = naviBar.GetFirstVisualChild<ComboBox>(c => c.Name == "DropDown1");
            var dropDown2 = naviBar.GetFirstVisualChild<ComboBox>(c => c.Name == "DropDown2");

            if (dropDown0 == null || dropDown1 == null || dropDown2 == null)
            {
                UnloadEvents();
            }

            var container = dropDown1.GetParent<Grid>();

            if (container == null)
            {
                UnloadEvents();
            }

            _control.SetCurrentValue(Grid.BackgroundProperty, WpfHelper.ToBrush(EnvironmentColors.ComboBoxBackgroundBrushKey));
            _control.SetCurrentValue(Grid.ColumnSpanProperty, 5);
            container.Children.Add(_control);

            dropDown0.Visibility = Visibility.Collapsed;
            dropDown1.Visibility = Visibility.Collapsed;
            dropDown2.Visibility = Visibility.Collapsed;
        }

        void ViewUnloaded(object sender, EventArgs e)
        {
            UnloadEvents();
        }

        void UnloadEvents()
        {
            _wpfTextView.VisualElement.Loaded -= FindNaviBar;
            _wpfTextView.VisualElement.Unloaded -= ViewUnloaded;
        }
    }
}