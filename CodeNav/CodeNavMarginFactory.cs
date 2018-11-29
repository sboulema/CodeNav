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

namespace CodeNav
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(CodeNavMargin.MarginName + "Left")]
    [Order(Before = PredefinedMarginNames.Left)]         // Ensure that the margin occurs left of the editor window
    [MarginContainer(PredefinedMarginNames.Left)]       // Set the container to the left of the editor window
    [ContentType("CSharp")]                             // Show this margin for all code-based types
    [ContentType("Basic")]
    [TextViewRole(PredefinedTextViewRoles.Debuggable)]  // This is to prevent the margin from loading in the diff view
    internal sealed class CodeNavLeftFactory : IWpfTextViewMarginProvider
    {
        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider ServiceProvider { get; set; }

        [Import(typeof(VisualStudioWorkspace))]
        private VisualStudioWorkspace Workspace { get; set; }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            return CodeNavFactory.CreateMargin(wpfTextViewHost, Workspace, ServiceProvider, MarginSideEnum.Left);
        }
    }

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

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            return CodeNavFactory.CreateMargin(wpfTextViewHost, Workspace, ServiceProvider, MarginSideEnum.Right);
        }
    }

    internal static class CodeNavFactory
    {
        public static IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, 
            VisualStudioWorkspace visualStudioWorkspace, IServiceProvider serviceProvider, MarginSideEnum side)
        {
            if (Settings.Default.MarginSide != side) return null;

            var dte = (DTE)serviceProvider.GetService(typeof(DTE));
            var outliningManagerService = OutliningHelper.GetOutliningManagerService(serviceProvider);

            var codeNav = new CodeNavMargin(wpfTextViewHost, dte, outliningManagerService, visualStudioWorkspace, side);

            return codeNav;
        }
    } 
}
