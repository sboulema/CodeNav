using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeNav.Models;
using Community.VisualStudio.Toolkit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;

namespace CodeNav.Helpers
{
    public static class OutliningHelper
    {
        public static async Task<IOutliningManager> GetOutliningManager()
        {
            var componentModel = await VS.Services.GetComponentModelAsync();
            var outliningManagerService = componentModel.GetService<IOutliningManagerService>();

            var documentView = await VS.Documents.GetActiveDocumentViewAsync();
            var textView = documentView?.TextView;

            if (outliningManagerService == null ||
                textView == null)
            {
                return null;
            }

            return outliningManagerService.GetOutliningManager(textView);
        }

        public static void RegionsCollapsed(RegionsCollapsedEventArgs e, IEnumerable<CodeItem> document) =>
            e.CollapsedRegions.ToList().ForEach(region => SetRegionIsExpanded(document, region, false));

        public static void RegionsExpanded(RegionsExpandedEventArgs e, IEnumerable<CodeItem> document) =>
            e.ExpandedRegions.ToList().ForEach(region => SetRegionIsExpanded(document, region, true));

        /// <summary>
        /// Set all collapsibles in a document
        /// </summary>
        /// <param name="document">document that holds the collapsibles</param>
        /// <param name="isExpanded">should collapsible be expanded</param>
        public static void ToggleAll(IEnumerable<CodeItem> document, bool isExpanded)
            => document
                .ToList()
                .ForEach(root => root
                    .Descendants()
                    .Where(i => i is ICodeCollapsible)
                    .Cast<IMembers>()
                    .ToList()
                    .ForEach(i => i.IsExpanded = isExpanded));

        public static async Task SyncAllRegions(IEnumerable<CodeItem> document)
        {
            foreach (var item in document)
            {
                if (!(item is IMembers)) continue;

                var collapsible = await FindCollapsibleFromCodeItem(item);

                if (collapsible == null)
                {
                    (item as IMembers).IsExpanded = true;
                }
                else
                {
                    (item as IMembers).IsExpanded = !collapsible.IsCollapsed;
                }

                if (item is ICodeCollapsible)
                {
                    (item as ICodeCollapsible).IsExpandedChanged += OnIsExpandedChanged;
                }
                
                await SyncAllRegions((item as IMembers).Members);
            }
        }

        /// <summary>
        /// Set CodeItem to be expanded or not
        /// </summary>
        /// <param name="document">Document with all codeitems</param>
        /// <param name="region">Region/Collapsible that changed outlining</param>
        /// <param name="isExpanded">Should codeitem be expanded</param>
        private static void SetRegionIsExpanded(IEnumerable<CodeItem> document, ICollapsible region, bool isExpanded)
        {
            if (!document.Any())
            {
                return;
            }

            var startLine = GetStartLineForCollapsible(region);

            document
                .ToList()
                .ForEach(root => root
                    .Descendants()
                    .Where(i => i.StartLine == startLine)
                    .Where(i => i is IMembers)
                    .Cast<IMembers>()
                    .ToList()
                    .ForEach(i => i.IsExpanded = isExpanded));
        }

        /// <summary>
        /// Gets the start line for the specified collapsible.
        /// </summary>
        /// <remarks>
        /// The +1 offset is to accomdate for the 0-based code indexing vs. 1-based code item indexing.
        /// </remarks>
        /// <param name="collapsible">The collapsible region.</param>
        /// <returns>The starting line.</returns>
        private static int GetStartLineForCollapsible(ICollapsible collapsible)
        {
            var startPoint = collapsible.Extent.GetStartPoint(collapsible.Extent.TextBuffer.CurrentSnapshot);
            var line = startPoint.Snapshot.GetLineNumberFromPosition(startPoint.Position) + 1;

            return line;
        }

        /// <summary>
        /// Attempts to find a <see cref="ICollapsible" /> associated with the specified <see
        /// cref="IMembers" />.
        /// </summary>
        /// <param name="item">The IMembers CodeItem.</param>
        /// <returns>The <see cref="ICollapsible" /> on the same starting line, otherwise null.</returns>
        private static async Task<ICollapsible> FindCollapsibleFromCodeItem(CodeItem item)
        {
            if (item.Kind == CodeItemKindEnum.ImplementedInterface)
            {
                return null;
            }

            var documentView = await VS.Documents.GetActiveDocumentViewAsync();

            if (item.StartLine > documentView?.TextView?.TextBuffer.CurrentSnapshot.LineCount)
            {
                return null;
            }

            try
            {
                var outliningManager = await GetOutliningManager();

                if (outliningManager == null)
                {
                    return null;
                }

                var collapsibles = outliningManager.GetAllRegions(ToSnapshotSpan(documentView?.TextView, item.Span));

                return (from collapsible in collapsibles
                        let startLine = GetStartLineForCollapsible(collapsible)
                        where startLine == item.StartLine
                        select collapsible).FirstOrDefault();
            }
            catch (ArgumentOutOfRangeException)
            {
                // FindCollapsibleFromCodeItem failed for item
                return null;
            }
            catch (ObjectDisposedException)
            {
                // FindCollapsibleFromCodeItem failed because of disposed object
                return null;
            }
        }

        /// <summary>
        /// Convert a <see cref="TextSpan"/> to a <see cref="SnapshotSpan"/> on the given <see cref="ITextSnapshot"/> instance
        /// </summary>
        private static SnapshotSpan ToSnapshotSpan(IWpfTextView textView, TextSpan textSpan)
        {
            var currentSnapshot = textView.TextBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(currentSnapshot, 0, currentSnapshot.Length);
            var span = new Span(textSpan.Start, textSpan.Length);

            if (!snapshotSpan.Contains(span))
            {
                return new SnapshotSpan(currentSnapshot, 0, 0);
            }

            return new SnapshotSpan(currentSnapshot, span);
        }

        /// <summary>
        /// An event handler raised when a code item parent's expanded state has changed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static async void OnIsExpandedChanged(object sender, EventArgs eventArgs)
        {
            if (!(sender is IMembers item))
            {
                return;
            }

            var iCollapsible = await FindCollapsibleFromCodeItem((CodeItem)item);

            if (iCollapsible == null)
            {
                return;
            }

            var outliningManager = await GetOutliningManager();

            if (item.IsExpanded && iCollapsible.IsCollapsed)
            {
                outliningManager.Expand(iCollapsible as ICollapsed);
            }
            else if (!item.IsExpanded && !iCollapsible.IsCollapsed)
            {
                outliningManager.TryCollapse(iCollapsible);
            }
        }

        private static IEnumerable<CodeItem> Descendants(this CodeItem root)
        {
            var items = new Stack<CodeItem>(new[] { root });
            while (items.Any())
            {
                var item = items.Pop();
                yield return item;

                if (item is IMembers)
                {
                    foreach (var i in (item as IMembers).Members) items.Push(i);
                }
            }
        }
    }
}
