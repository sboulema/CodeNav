using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CodeNav.Models;
using CodeNav.Properties;

namespace CodeNav.Helpers
{
    public static class VisibilityHelper
    {
        /// <summary>
        /// Loop through all codeItems and look into Settings to see if the item should be visible or not.
        /// </summary>
        /// <param name="document">List of codeItems</param>
        /// <param name="name">Filters items by name</param>
        public static List<CodeItem> SetCodeItemVisibility(List<CodeItem> document, string name = "")
        {
            try
            {
                if (document == null || !document.Any())
                {
                    LogHelper.Log($"No code items have been found to filter on by {name}");
                    return new List<CodeItem>();
                }

                foreach (var item in document)
                {
                    item.IsVisible = ShouldBeVisible(item, name) ? Visibility.Visible : Visibility.Collapsed;

                    if (item is IMembers)
                    {
                        var hasMembersItem = (IMembers)item;
                        if (hasMembersItem.Members.Any())
                        {
                            SetCodeItemVisibility(hasMembersItem.Members, name);
                        }
                        item.IsVisible = hasMembersItem.Members.Any(m => m.IsVisible == Visibility.Visible)
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Log("Error during setting visibility", e);
            }

            return document;
        }

        /// <summary>
        /// Toggle visibility of the CodeNav margin
        /// </summary>
        /// <param name="column">the grid column of which the visibility will be toggled</param>
        /// <param name="condition">if condition is True visibility will be set to hidden</param>
        public static void SetMarginWidth(ColumnDefinition column, bool condition)
        {
            if (column == null) return;
            column.Width = condition ? new GridLength(0) : new GridLength(Settings.Default.Width);
        }

        /// <summary>
        /// Toggle visibility of the CodeNav margin
        /// </summary>
        /// <param name="column">the grid column of which the visibility will be toggled</param>
        /// <param name="document">the list of codeitems to determine if there is anything to show at all</param>
        public static void SetMarginWidth(ColumnDefinition column, List<CodeItem> document)
        {
            if (column == null) return;
            if (!Settings.Default.ShowMargin)
            {
                column.Width = new GridLength(0);
            }
            else
            {
                column.Width = IsEmpty(document) ? new GridLength(0) : new GridLength(Settings.Default.Width);
            }       
        }

        public static bool IsEmpty(List<CodeItem> document)
        {
            if (!document.Any()) return true;

            var isEmpty = true;
            foreach (var item in document)
            {
                if (item is IMembers)
                {
                    isEmpty = !(item as IMembers).Members.Any();
                }
            }
            return isEmpty;
        }

        private static bool ShouldBeVisible(CodeItem item, string name = "")
        {
            var visible = true;

            if (Settings.Default.FilterRules != null)
            {
                var filterRule = Settings.Default.FilterRules.LastOrDefault(f =>
                    (f.Access == item.Access || f.Access == CodeItemAccessEnum.All) &&
                    (f.Kind == item.Kind || f.Kind == CodeItemKindEnum.All));

                if (filterRule != null)
                {
                    visible = filterRule.Visible;
                }
            }
            
            return visible && item.Name.Contains(name, StringComparison.OrdinalIgnoreCase);
        }

        public static bool ShouldBeVisible(CodeItemKindEnum kind)
        {
            var visible = true;

            if (Settings.Default.FilterRules != null)
            {
                var filterRule = Settings.Default.FilterRules.LastOrDefault(f =>
                    (f.Kind == kind || f.Kind == CodeItemKindEnum.All));

                if (filterRule != null)
                {
                    visible = filterRule.Visible;
                }
            }

            return visible;
        } 

        private static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }
    }
}
