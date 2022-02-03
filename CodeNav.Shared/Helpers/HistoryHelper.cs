using CodeNav.Extensions;
using CodeNav.Models;
using CodeNav.Models.ViewModels;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CodeNav.Helpers
{
    public static class HistoryHelper
    {
        private const int MaxHistoryItems = 5;

        /// <summary>
        /// Add codeitem to history items based on its span
        /// </summary>
        /// <remarks>Used when adding item to history based on text changes</remarks>
        /// <param name="model">Document holding all codeitems</param>
        /// <param name="span">Span containing the text changes</param>
        public static void AddItemToHistory(CodeDocumentViewModel model, Span span)
        {
            try
            {
                var item = FindCodeItem(model.CodeDocument, span);
                AddItemToHistory(item);
            }
            catch (Exception e)
            {
                LogHelper.Log("Error adding item to history", e);
            }
        }

        public static void AddItemToHistory(CodeItem item)
        {
            if (item?.Control?.CodeDocumentViewModel == null)
            {
                return;
            }

            var model = item.Control.CodeDocumentViewModel;

            // Clear current indicators
            foreach (var historyItem in model.HistoryItems)
            {
                if (historyItem == null)
                {
                    continue;
                }

                historyItem.StatusMonikerVisibility = Visibility.Collapsed;
            }

            // Add new indicator, only keep the five latest history items
            model.HistoryItems.Remove(item);
            model.HistoryItems.Insert(0, item);
            model.HistoryItems = new SynchronizedCollection<CodeItem>(model.HistoryItems.SyncRoot, model.HistoryItems.Take(MaxHistoryItems));

            ApplyHistoryIndicator(model);
        }

        /// <summary>
        /// Apply history indicator to all codeitems in the history list
        /// </summary>
        /// <remarks>Uses Flatten() to do this recursively for child codeitems</remarks>
        /// <param name="model">Document holding all codeitems</param>
        public static void ApplyHistoryIndicator(CodeDocumentViewModel model)
        {
            var historyItems = model.HistoryItems
                .Where(i => i != null)
                .Select((historyItem, i) => (historyItem, i));

            foreach (var (historyItem, i) in historyItems)
            {
                var codeItem = model.CodeDocument
                    .Flatten()
                    .Where(item => item != null)
                    .FirstOrDefault(item => item.Id == historyItem.Id);

                if (codeItem == null)
                {
                    continue;
                }

                ApplyHistoryIndicator(codeItem, i);
            }
        }

        /// <summary>
        /// Apply history indicator to a single codeitem
        /// </summary>
        /// <remarks>Index determines color and opacity of the history indicator</remarks>
        /// <param name="item">Codeitem in history list</param>
        /// <param name="index">Index in history list</param>
        private static void ApplyHistoryIndicator(CodeItem item, int index = 0)
        {
            item.StatusMoniker = KnownMonikers.History;
            item.StatusMonikerVisibility = Visibility.Visible;
            item.StatusGrayscale = index > 0;
            item.StatusOpacity = GetOpacity(index);
        }

        /// <summary>
        /// Get opacity based on history list index
        /// </summary>
        /// <param name="index">Index in history list</param>
        /// <remarks>
        /// 0: latest history item => 100%
        /// 1-2: => 90%
        /// 3-4: => 70%
        /// </remarks>
        /// <returns>Double between 0 and 1</returns>
        private static double GetOpacity(int index)
        {
            switch (index)
            {
                case 0:
                    return 1;
                case 1:
                case 2:
                    return 0.9;
                case 3:
                case 4:
                    return 0.7;
                default:
                    return 1;
            }
        }

        /// <summary>
        /// Delete all history item indicators
        /// </summary>
        /// <param name="item">Codeitem on which the context menu was invoked</param>
        public static void ClearHistory(CodeItem item)
        {
            item.Control.CodeDocumentViewModel.HistoryItems.Clear();
            SolutionStorageHelper.SaveToSolutionStorage(item.Control.CodeDocumentViewModel).FireAndForget();
            item.Control.UpdateDocument();
        }

        private static CodeItem FindCodeItem(IEnumerable<CodeItem> items, Span span)
        {
            foreach (var item in items)
            {
                if (item.Span.Contains(span.Start) && !(item is IMembers))
                {
                    return item;
                }

                if (item is IMembers hasMembersItem)
                {
                    return FindCodeItem(hasMembersItem.Members, span);
                }
            }

            return null;
        }

        public static async Task<SynchronizedCollection<CodeItem>> LoadHistoryItemsFromStorage(string filePath)
        {
            var storageItem = await SolutionStorageHelper.GetStorageItem(filePath);

            if (storageItem == null)
            {
                return new SynchronizedCollection<CodeItem>();
            }

            return storageItem.HistoryItems ?? new SynchronizedCollection<CodeItem>();
        }
    }
}