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
        public static List<CodeItem> SetCodeItemVisibility(List<CodeItem> document, string name = "")
        {
            if (document == null || !document.Any()) return new List<CodeItem>();

            foreach (var item in document)
            {
                var shouldBeVisibleBasedOnKind = ShouldBeVisibleBasedOnKind(item.Kind);
                var shouldBeVisibleBasedOnAccess = ShouldBeVisibleBasedOnAccess(item.Access);
                var shouldBeVisibleBasedOnName = item.Name.Contains(name, StringComparison.OrdinalIgnoreCase);

                item.IsVisible = shouldBeVisibleBasedOnKind && 
                                 shouldBeVisibleBasedOnAccess &&
                                 shouldBeVisibleBasedOnName ? Visibility.Visible : Visibility.Collapsed;

                if (item is CodeClassItem)
                {
                    var classItem = (CodeClassItem)item;
                    if (classItem.Members.Any())
                    {
                        SetCodeItemVisibility(classItem.Members, name);
                    }
                }
            }

            return document;
        }

        /// <summary>
        /// Toggle visibility of the CodeNav control
        /// </summary>
        /// <param name="column">the grid column of which the visibility will be toggled</param>
        /// <param name="condition">if condition is True visibility will be set to hidden</param>
        public static void SetControlVisibility(ColumnDefinition column, bool condition)
        {
            if (column == null) return;
            column.Width = condition ? new GridLength(0) : new GridLength(Settings.Default.Width);
        }

        private static bool ShouldBeVisibleBasedOnAccess(CodeItemAccessEnum access)
        {
            switch (access)
            {
                case CodeItemAccessEnum.Private:
                    return Settings.Default.ShowPrivate;
                case CodeItemAccessEnum.Protected:
                    return Settings.Default.ShowProtected;
                case CodeItemAccessEnum.Public:
                    return Settings.Default.ShowPublic;
                case CodeItemAccessEnum.Internal:
                    return Settings.Default.ShowInternal;
                case CodeItemAccessEnum.Unknown:
                    return Settings.Default.ShowEnumItems;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool ShouldBeVisibleBasedOnKind(CodeItemKindEnum kind)
        {
            switch (kind)
            {
                case CodeItemKindEnum.Constant:
                    return Settings.Default.ShowConstants;
                case CodeItemKindEnum.Constructor:
                    return Settings.Default.ShowConstructors;
                case CodeItemKindEnum.Delegate:
                    return Settings.Default.ShowConstructors;
                case CodeItemKindEnum.Enum:
                    return Settings.Default.ShowEnums;
                case CodeItemKindEnum.EnumItem:
                    return Settings.Default.ShowEnumItems;
                case CodeItemKindEnum.Event:
                    return Settings.Default.ShowEvents;
                case CodeItemKindEnum.Method:
                    return Settings.Default.ShowMethods;
                case CodeItemKindEnum.Property:
                    return Settings.Default.ShowProperties;
                case CodeItemKindEnum.Struct:
                    return Settings.Default.ShowStructs;
                case CodeItemKindEnum.Variable:
                    return Settings.Default.ShowVariables;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }
    }
}
