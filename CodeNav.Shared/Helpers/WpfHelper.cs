﻿#nullable enable

using System;
using System.Collections.Generic;
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

        public static TParent? GetParent<TParent>(this DependencyObject obj, Predicate<TParent>? predicate = null)
            where TParent : DependencyObject
        {
            if (obj == null)
            {
                return null;
            }
            var p = obj;
            TParent? r;
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

        public static TChild? GetFirstVisualChild<TChild>(this DependencyObject obj, Predicate<TChild>? predicate = null)
            where TChild : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(obj);

            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);

                if (child is TChild r && (predicate == null || predicate(r)))
                {
                    return r;
                }

                var visualChild = GetFirstVisualChild(child, predicate);
                if (visualChild != null)
                {
                    return visualChild;
                }
            }

            return null;
        }

        public static T? GetGridChildByType<T>(this Grid grid) where T : class
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

        public static ICodeViewUserControl? FindParent(DependencyObject child)
            => FindParent<CodeViewUserControl>(child) ??
                (ICodeViewUserControl?)FindParent<CodeViewUserControlTop>(child);

        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            // Get parent item
            var parentObject = VisualTreeHelper.GetParent(child);

            // We’ve reached the end of the tree
            if (parentObject == null)
            {
                return null;
            }

            // Check if the parent matches the type we’re looking for
            return parentObject is T parent ? parent : FindParent<T>(parentObject);
        }

        public static List<T> FindChildrenByType<T>(DependencyObject? depObj) where T : DependencyObject
        {
            var children = new List<T>();

            if (depObj == null)
            {
                return children;
            }

            for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(depObj) - 1; i++)
            {
                var childObject = VisualTreeHelper.GetChild(depObj, i);

                if (childObject != null && childObject is T child)
                {
                    children.Add(child);
                }

                children.AddRange(FindChildrenByType<T>(childObject));
            }

            return children;
        }
    }
}
