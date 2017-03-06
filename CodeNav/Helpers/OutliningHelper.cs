using System;
using System.Collections.Generic;
using System.Linq;
using CodeNav.Models;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;

namespace CodeNav.Helpers
{
    public static class OutliningHelper
    {
        private static IOutliningManager _manager;
        private static IWpfTextView _textView;

        public static IOutliningManager GetManager(IServiceProvider serviceProvider, ITextView textView)
        {
            var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
            var outliningManagerService = componentModel.GetService<IOutliningManagerService>();
            return outliningManagerService.GetOutliningManager(textView);
        }

        public static void RegionsCollapsed(RegionsCollapsedEventArgs e, IEnumerable<CodeItem> document)
        {
            foreach (var region in e.CollapsedRegions)
            {
                SetRegionIsExpanded(document, region, false);
            }          
        }

        public static void RegionsExpanded(RegionsExpandedEventArgs e, IEnumerable<CodeItem> document)
        {
            foreach (var region in e.ExpandedRegions)
            {
                SetRegionIsExpanded(document, region, true);
            }
        }

        public static void SyncAllRegions(IOutliningManager manager, IWpfTextView textView, IEnumerable<CodeItem> document)
        {
            if (manager == null) return;

            _manager = manager;
            _textView = textView;

            foreach (var item in document)
            {
                if (!(item is IMembers)) continue;

                var collapsible = FindCollapsibleFromCodeItem(item, manager, textView);

                if (collapsible == null)
                {
                    (item as IMembers).IsExpanded = true;
                }
                else
                {
                    (item as IMembers).IsExpanded = !collapsible.IsCollapsed;
                }

                (item as IMembers).IsExpandedChanged += OnIsExpandedChanged;

                SyncAllRegions(manager, textView, (item as IMembers).Members);
            }
        }

        private static void SetRegionIsExpanded(IEnumerable<CodeItem> document, ICollapsible region, bool isExpanded)
        {
            var startLine = GetStartLineForCollapsible(region);

            var found = new List<CodeItem>();
            FindCodeItem(found, document, startLine);
            if (!found.Any()) return;

            var item = found.Last();
            if (!(item is IMembers)) return;
            (item as IMembers).IsExpanded = isExpanded;
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
        private static ICollapsible FindCollapsibleFromCodeItem(CodeItem item, IOutliningManager manager, IWpfTextView textView)
        {
            if (item.StartLine > textView.TextBuffer.CurrentSnapshot.LineCount) return null;

                var snapshotLine = textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(item.StartLine);
                var collapsibles = manager.GetAllRegions(snapshotLine.Extent);

                return (from collapsible in collapsibles
                        let startLine = GetStartLineForCollapsible(collapsible)
                        where startLine == item.StartLine
                        select collapsible).FirstOrDefault();
        }

        /// <summary>
        /// An event handler raised when a code item parent's expanded state has changed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static void OnIsExpandedChanged(object sender, EventArgs eventArgs)
        {
            var item = sender as IMembers;
            if (item != null)
            {
                var iCollapsible = FindCollapsibleFromCodeItem((CodeItem)item, _manager, _textView);
                if (iCollapsible != null)
                {
                    if (item.IsExpanded && iCollapsible.IsCollapsed)
                    {
                        _manager.Expand(iCollapsible as ICollapsed);
                    }
                    else if (!item.IsExpanded && !iCollapsible.IsCollapsed)
                    {
                        _manager.TryCollapse(iCollapsible);
                    }
                }
            }
        }

        private static IEnumerable<CodeItem> FindCodeItem(List<CodeItem> found, IEnumerable<CodeItem> items, int line)
        {
            foreach (var item in items)
            {
                if (item.StartLine <= line && item.EndLine >= line)
                {
                    found.Add(item);
                }

                if (item is IMembers)
                {
                    found.AddRange(FindCodeItem(found, ((IMembers)item).Members, line));
                }
            }

            return found;
        }
    }
}
