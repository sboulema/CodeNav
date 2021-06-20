using System;
using System.Windows;
using System.Windows.Controls;
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
            for (var i = 0; i < count; i++)
            {
                var c = VisualTreeHelper.GetChild(obj, i);
                if (c is TChild r && (predicate == null || predicate(r)))
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

        public static T GetGridChildByType<T>(this Grid grid) where T : class
        {
            foreach (var child in grid.Children)
            {
                if (child is T)
                {
                    return child as T;
                }
            }

            return default;
        }
    }
}
