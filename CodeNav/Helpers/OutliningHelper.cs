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

        public static void RegionsCollapsed(RegionsCollapsedEventArgs e, IEnumerable<CodeItem> document) => 
            e.CollapsedRegions.ToList().ForEach(region => SetRegionIsExpanded(document, region, false));

        public static void RegionsExpanded(RegionsExpandedEventArgs e, IEnumerable<CodeItem> document) => 
            e.ExpandedRegions.ToList().ForEach(region => SetRegionIsExpanded(document, region, true));

        /// <summary>
        /// Set all #region collapsibles in a document
        /// </summary>
        /// <param name="document">document that holds the regions</param>
        /// <param name="isExpanded">should region be expanded</param>
        public static void SetAllRegions(IEnumerable<CodeItem> document, bool isExpanded) => 
            document.ToList().ForEach(
                root => root.Descendants().Where(i => i.Kind == CodeItemKindEnum.Region).ToList()
                    .ForEach(ci => (ci as IMembers).IsExpanded = isExpanded)
            );

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
            if (!document.Any()) return;

            var startLine = GetStartLineForCollapsible(region);

            document.ToList().ForEach(
                root => root.Descendants().Where(i => i.StartLine == startLine && i.Kind == CodeItemKindEnum.Region).ToList()
                    .ForEach(ci => (ci as IMembers).IsExpanded = isExpanded)
            );
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
        /// <param name="manager">The outlining manager to get all regions</param>
        /// <param name="textView">The textview to find collapsibles in</param>
        /// <returns>The <see cref="ICollapsible" /> on the same starting line, otherwise null.</returns>
        private static ICollapsible FindCollapsibleFromCodeItem(CodeItem item, IOutliningManager manager, IWpfTextView textView)
        {
            if (item.Kind == CodeItemKindEnum.ImplementedInterface) return null;
            if (item.StartLine > textView.TextBuffer.CurrentSnapshot.LineCount) return null;

            try
            {
                var snapshotLine = textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(item.StartLine);
                var collapsibles = manager.GetAllRegions(snapshotLine.Extent);

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
