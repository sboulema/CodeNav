using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Windows;
using System.Windows.Media;

namespace CodeNav.Helpers
{
    public static class WpfHelper
    {
        public static DependencyObject GetParent(this DependencyObject obj)
        {
            return obj is Visual ? VisualTreeHelper.GetParent(obj) : LogicalTreeHelper.GetParent(obj);
        }

        public static TParent GetParent<TParent>(this DependencyObject obj, Predicate<TParent> predicate = null)
            where TParent : DependencyObject
        {
            if (obj == null)
            {
                return null;
            }
            var p = obj;
            TParent r;
            while ((p = p.GetParent()) != null)
            {
                r = p as TParent;
                if (r != null && (predicate == null || predicate(r)))
                {
                    return r;
                }
            }
            return null;
        }

        public static TChild GetFirstVisualChild<TChild>(this DependencyObject obj, Predicate<TChild> predicate = null)
            where TChild : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                var c = VisualTreeHelper.GetChild(obj, i);
                var r = c as TChild;
                if (r != null && (predicate == null || predicate(r)))
                {
                    return r;
                }
                r = GetFirstVisualChild(c, predicate);
                if (r != null)
                {
                    return r;
                }
            }
            return null;
        }

        public static SolidColorBrush ToBrush(ThemeResourceKey key)
        {
            var color = VSColorTheme.GetThemedColor(key);
            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }
    }
}
