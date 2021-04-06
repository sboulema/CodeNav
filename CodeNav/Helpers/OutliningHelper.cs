using System;
using System.Collections.Generic;
using System.Linq;
using CodeNav.Models;
using Microsoft;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;

namespace CodeNav.Helpers
{
    public static class OutliningHelper
    {
        private static IOutliningManagerService _outliningManagerService;
        private static IWpfTextView _textView;

        public static IOutliningManagerService GetOutliningManagerService(IServiceProvider serviceProvider)
        {
            var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
            Assumes.Present(componentModel);
            return componentModel.GetService<IOutliningManagerService>();
        }

        public static IOutliningManager GetOutliningManager(IOutliningManagerService outliningManagerService, ITextView textView)
        {
            if (outliningManagerService == null) return null;
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
        public static void ToggleAll(IEnumerable<CodeItem> document, bool isExpanded) =>
            document.ToList()
                .ForEach(root => root.Descendants().Where(i => i is ICodeCollapsible).ToList()
                .ForEach(ci => (ci as IMembers).IsExpanded = isExpanded));

        public static void SyncAllRegions(IOutliningManagerService outliningManagerService, IWpfTextView textView, IEnumerable<CodeItem> document)
        {
            if (outliningManagerService == null) return;

            _outliningManagerService = outliningManagerService;
            _textView = textView;

            foreach (var item in document)
            {
                if (!(item is IMembers)) continue;

                var collapsible = FindCollapsibleFromCodeItem(item, outliningManagerService, textView);

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
                
                SyncAllRegions(outliningManagerService, textView, (item as IMembers).Members);
            }
        }

        private static void SetRegionIsExpanded(IEnumerable<CodeItem> document, ICollapsible region, bool isExpanded)
        {
            if (!document.Any()) return;

            var startLine = GetStartLineForCollapsible(region);

            document.ToList().ForEach(
                root => root.Descendants().Where(i => i.StartLine == startLine 
                    && (i is IMembers)).ToList()
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
        /// <param name="outliningManagerService">The outlining manager to get all regions</param>
        /// <param name="textView">The textview to find collapsibles in</param>
        /// <returns>The <see cref="ICollapsible" /> on the same starting line, otherwise null.</returns>
        private static ICollapsible FindCollapsibleFromCodeItem(CodeItem item, 
            IOutliningManagerService outliningManagerService, IWpfTextView textView)
        {
            if (item.Kind == CodeItemKindEnum.ImplementedInterface) return null;
            if (item.StartLine > textView.TextBuffer.CurrentSnapshot.LineCount) return null;

            try
            {
                var outliningManager = GetOutliningManager(outliningManagerService, textView);
                if (outliningManager == null) return null;

                var collapsibles = outliningManager.GetAllRegions(ToSnapshotSpan(textView, item.Span));

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
            var span = new Span(textSpan.Start, textSpan.Length);

            try
            {
                return new SnapshotSpan(textView.TextBuffer.CurrentSnapshot, span);
            }
            catch (ArgumentOutOfRangeException)
            {
                return new SnapshotSpan(textView.TextBuffer.CurrentSnapshot, 0, 0);
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
                var iCollapsible = FindCollapsibleFromCodeItem((CodeItem)item, _outliningManagerService, _textView);
                if (iCollapsible != null)
                {
                    var outliningManager = _outliningManagerService.GetOutliningManager(_textView);

                    if (item.IsExpanded && iCollapsible.IsCollapsed)
                    {
                        outliningManager.Expand(iCollapsible as ICollapsed);
                    }
                    else if (!item.IsExpanded && !iCollapsible.IsCollapsed)
                    {
                        outliningManager.TryCollapse(iCollapsible);
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
