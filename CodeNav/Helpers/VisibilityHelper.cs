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
        public static void SetCodeItemVisibility(List<CodeItem> document)
        {
            foreach (var item in document)
            {
                bool shouldBeVisible;
                switch (item.Kind)
                {
                    case CodeItemKindEnum.Constant:
                        shouldBeVisible = Settings.Default.ShowConstants;
                        break;
                    case CodeItemKindEnum.Constructor:
                        shouldBeVisible = Settings.Default.ShowConstructors;
                        break;
                    case CodeItemKindEnum.Delegate:
                        shouldBeVisible = Settings.Default.ShowConstructors;
                        break;
                    case CodeItemKindEnum.Enum:
                        shouldBeVisible = Settings.Default.ShowEnums;
                        break;
                    case CodeItemKindEnum.EnumItem:
                        shouldBeVisible = Settings.Default.ShowEnumItems;
                        break;
                    case CodeItemKindEnum.Event:
                        shouldBeVisible = Settings.Default.ShowEvents;
                        break;
                    case CodeItemKindEnum.Method:
                        shouldBeVisible = Settings.Default.ShowMethods;
                        break;
                    case CodeItemKindEnum.Property:
                        shouldBeVisible = Settings.Default.ShowProperties;
                        break;
                    case CodeItemKindEnum.Struct:
                        shouldBeVisible = Settings.Default.ShowStructs;
                        break;
                    case CodeItemKindEnum.Variable:
                        shouldBeVisible = Settings.Default.ShowVariables;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                item.IsVisible = shouldBeVisible ? Visibility.Visible : Visibility.Collapsed;

                if (item is CodeClassItem)
                {
                    var classItem = (CodeClassItem)item;
                    if (classItem.Members.Any())
                    {
                        SetCodeItemVisibility(classItem.Members);
                    }
                }
            }
        }

        /// <summary>
        /// Toggle visibility of the CodeNav control
        /// </summary>
        /// <param name="column">the grid column of which the visibility will be toggled</param>
        /// <param name="condition">if condition is True visibility will be set to hidden</param>
        public static void SetControlVisibility(ColumnDefinition column, bool condition)
        {
            column.Width = condition ? new GridLength(0) : new GridLength(Settings.Default.Width);
        }
    }
}
