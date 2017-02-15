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
        public static List<CodeItem> SetCodeItemVisibility(List<CodeItem> document)
        {
            if (document == null || !document.Any()) return new List<CodeItem>();

            foreach (var item in document)
            {
                bool shouldBeVisibleBasedOnKind;
                switch (item.Kind)
                {
                    case CodeItemKindEnum.Constant:
                        shouldBeVisibleBasedOnKind = Settings.Default.ShowConstants;
                        break;
                    case CodeItemKindEnum.Constructor:
                        shouldBeVisibleBasedOnKind = Settings.Default.ShowConstructors;
                        break;
                    case CodeItemKindEnum.Delegate:
                        shouldBeVisibleBasedOnKind = Settings.Default.ShowConstructors;
                        break;
                    case CodeItemKindEnum.Enum:
                        shouldBeVisibleBasedOnKind = Settings.Default.ShowEnums;
                        break;
                    case CodeItemKindEnum.EnumItem:
                        shouldBeVisibleBasedOnKind = Settings.Default.ShowEnumItems;
                        break;
                    case CodeItemKindEnum.Event:
                        shouldBeVisibleBasedOnKind = Settings.Default.ShowEvents;
                        break;
                    case CodeItemKindEnum.Method:
                        shouldBeVisibleBasedOnKind = Settings.Default.ShowMethods;
                        break;
                    case CodeItemKindEnum.Property:
                        shouldBeVisibleBasedOnKind = Settings.Default.ShowProperties;
                        break;
                    case CodeItemKindEnum.Struct:
                        shouldBeVisibleBasedOnKind = Settings.Default.ShowStructs;
                        break;
                    case CodeItemKindEnum.Variable:
                        shouldBeVisibleBasedOnKind = Settings.Default.ShowVariables;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                bool shouldBeVisibleBasedOnAccess;
                switch (item.Access)
                {
                    case CodeItemAccessEnum.Private:
                        shouldBeVisibleBasedOnAccess = Settings.Default.ShowPrivate;
                        break;
                    case CodeItemAccessEnum.Protected:
                        shouldBeVisibleBasedOnAccess = Settings.Default.ShowProtected;
                        break;
                    case CodeItemAccessEnum.Public:
                        shouldBeVisibleBasedOnAccess = Settings.Default.ShowPublic;
                        break;
                    case CodeItemAccessEnum.Internal:
                        shouldBeVisibleBasedOnAccess = Settings.Default.ShowInternal;
                        break;
                    case CodeItemAccessEnum.Unknown:
                        shouldBeVisibleBasedOnAccess = Settings.Default.ShowEnumItems;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                item.IsVisible = shouldBeVisibleBasedOnKind && shouldBeVisibleBasedOnAccess ? Visibility.Visible : Visibility.Collapsed;

                if (item is CodeClassItem)
                {
                    var classItem = (CodeClassItem)item;
                    if (classItem.Members.Any())
                    {
                        SetCodeItemVisibility(classItem.Members);
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
    }
}
