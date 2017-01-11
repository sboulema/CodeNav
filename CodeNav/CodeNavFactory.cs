using System;
using System.ComponentModel.Composition;
using CodeNav.Helpers;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace CodeNav
{
    /// <summary>
    /// Export a <see cref="IWpfTextViewMarginProvider"/>, which returns an instance of the margin for the editor to use.
    /// </summary>
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(CodeNav.MarginName)]
    [Order(Before = PredefinedMarginNames.Left)]  // Ensure that the margin occurs below the horizontal scrollbar
    [MarginContainer(PredefinedMarginNames.Left)]         // Set the container to the left of the editor window
    [ContentType("CSharp")]                                 // Show this margin for all code-based types
    [TextViewRole(PredefinedTextViewRoles.Debuggable)]    // This is to prevent the margin from loading in the diff view
    internal sealed class CodeNavFactory : IWpfTextViewMarginProvider
    {
        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider _serviceProvider;

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
            var dte = (DTE)_serviceProvider.GetService(typeof(DTE));
            Logger.Initialize(_serviceProvider, "CodeNav");
            var codeNav = new CodeNav(wpfTextViewHost, dte);
            return codeNav;
        }

        #endregion
    }
}
