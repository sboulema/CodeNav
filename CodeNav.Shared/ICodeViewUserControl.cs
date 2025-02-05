#nullable enable

using CodeNav.Models;
using CodeNav.Models.ViewModels;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CodeNav
{
    public interface ICodeViewUserControl
    {
        CodeDocumentViewModel CodeDocumentViewModel { get; set; }

        /// <summary>
        /// Update the Code Document View Model
        /// </summary>
        /// <param name="filePath">Optional path to use as code source file</param>
        /// <param name="force">Optional boolean to indicate to update documents that may contain too many lines</param>
        void UpdateDocument(string filePath = "", bool force = false);

        void HighlightCurrentItem(CaretPositionChangedEventArgs e, Color backgroundBrushColor);

        void ToggleAll(bool isExpanded, List<CodeItem>? root = null);

        void FilterBookmarks();

        Task RegisterDocumentEvents();

        IDisposable? CaretPositionChangedSubscription { get; set; }

        IDisposable? TextContentChangedSubscription { get; set; }

        IDisposable? UpdateWhileTypingSubscription { get; set; }

        IDisposable? FileActionOccurredSubscription { get; set; }
    }
}
