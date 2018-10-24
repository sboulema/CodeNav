using System;
using System.ComponentModel.Composition;
using CodeNav.Helpers;
using CodeNav.Properties;
using EnvDTE;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using CodeNav.Models;
using Microsoft.VisualStudio.Shell.Interop;

namespace CodeNav
{
    /// <summary>
    /// Export a <see cref="IWpfTextViewMarginProvider"/>, which returns an instance of the margin for the editor to use.
    /// </summary>
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(CodeNavMargin.MarginName + "Left")]
    [Order(Before = PredefinedMarginNames.Left)]         // Ensure that the margin occurs left of the editor window
    [MarginContainer(PredefinedMarginNames.Left)]       // Set the container to the left of the editor window
    [ContentType("CSharp")]                             // Show this margin for all code-based types
    [ContentType("Basic")]
    [TextViewRole(PredefinedTextViewRoles.Debuggable)]  // This is to prevent the margin from loading in the diff view
    internal sealed class CodeNavMarginFactory : IWpfTextViewMarginProvider
    {
        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider ServiceProvider { get; set; }

        [Import(typeof(VisualStudioWorkspace))]
        private VisualStudioWorkspace Workspace { get; set; }

        #region IWpfTextViewMarginProvider

        /// <summary>
        /// Creates an <see cref="IWpfTextViewMargin"/> for the given <see cref="IWpfTextViewHost"/>.
        /// </summary>
        /// <param name="wpfTextViewHost">The <see cref="IWpfTextViewHost"/> for which to create the <see cref="IWpfTextViewMargin"/>.</param>
        /// <param name="marginContainer">The margin that will contain the newly-created margin.</param>
        /// <returns>The <see cref="IWpfTextViewMargin"/>.
        /// The value may be null if this <see cref="IWpfTextViewMarginProvider"/> does not participate for this context.
        /// </returns>
        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            if (Settings.Default.MarginSide != MarginSideEnum.Left) return null;

            var dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            var outliningManager = OutliningHelper.GetManager(ServiceProvider, wpfTextViewHost.TextView);

            var codeNav = new CodeNavMargin(wpfTextViewHost, dte, outliningManager, Workspace, MarginSideEnum.Left);

            return codeNav;
        }

        #endregion
    }

    /// <summary>
    /// Export a <see cref="IWpfTextViewMarginProvider"/>, which returns an instance of the margin for the editor to use.
    /// </summary>
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(CodeNavMargin.MarginName + "Right")]
    [Order(After = PredefinedMarginNames.RightControl)]  // Ensure that the margin occurs after the vertical scrollbar
    [MarginContainer(PredefinedMarginNames.Right)]       // Set the container to the right of the editor window
    [ContentType("CSharp")]                              // Show this margin for all code-based types
    [ContentType("Basic")]
    [TextViewRole(PredefinedTextViewRoles.Debuggable)]   // This is to prevent the margin from loading in the diff view
    internal sealed class CodeNavRightFactory : IWpfTextViewMarginProvider
    {
        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider ServiceProvider;

        [Import(typeof(VisualStudioWorkspace))]
        private VisualStudioWorkspace Workspace { get; set; }

        #region IWpfTextViewMarginProvider

        /// <summary>
        /// Creates an <see cref="IWpfTextViewMargin"/> for the given <see cref="IWpfTextViewHost"/>.
        /// </summary>
        /// <param name="wpfTextViewHost">The <see cref="IWpfTextViewHost"/> for which to create the <see cref="IWpfTextViewMargin"/>.</param>
        /// <param name="marginContainer">The margin that will contain the newly-created margin.</param>
        /// <returns>The <see cref="IWpfTextViewMargin"/>.
        /// The value may be null if this <see cref="IWpfTextViewMarginProvider"/> does not participate for this context.
        /// </returns>
        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            if (Settings.Default.MarginSide != MarginSideEnum.Right) return null;

            var dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            var outliningManager = OutliningHelper.GetManager(ServiceProvider, wpfTextViewHost.TextView);

            var codeNav = new CodeNavMargin(wpfTextViewHost, dte, outliningManager, Workspace, MarginSideEnum.Right);

            return codeNav;
        }

        #endregion
    }
}
