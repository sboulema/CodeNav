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
    [ContentType("CSharp")]                             // Show this margin for supported code-based types
    [ContentType("Basic")]
    [ContentType("JavaScript")]
    [ContentType("TypeScript")]
    [TextViewRole(PredefinedTextViewRoles.Debuggable)]  // This is to prevent the margin from loading in the diff view
    internal sealed class CodeNavLeftFactory : IWpfTextViewMarginProvider
    {
        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider ServiceProvider { get; set; }

        [Import(typeof(VisualStudioWorkspace))]
        private VisualStudioWorkspace Workspace { get; set; }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();

            return CodeNavFactory.CreateMargin(wpfTextViewHost, Workspace, ServiceProvider, MarginSideEnum.Left);
        }
    }

    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(CodeNavMargin.MarginName + "Right")]
    [Order(After = PredefinedMarginNames.RightControl)]  // Ensure that the margin occurs after the vertical scrollbar
    [MarginContainer(PredefinedMarginNames.Right)]       // Set the container to the right of the editor window
    [ContentType("CSharp")]                              // Show this margin for supported code-based types
    [ContentType("Basic")]
    [ContentType("JavaScript")]
    [ContentType("TypeScript")]
    [TextViewRole(PredefinedTextViewRoles.Debuggable)]   // This is to prevent the margin from loading in the diff view
    internal sealed class CodeNavRightFactory : IWpfTextViewMarginProvider
    {
        [Import(typeof(SVsServiceProvider))]
        #pragma warning disable CS0649
        private IServiceProvider ServiceProvider;
        #pragma warning restore CS0649

        [Import(typeof(VisualStudioWorkspace))]
        private VisualStudioWorkspace Workspace { get; set; }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();

            return CodeNavFactory.CreateMargin(wpfTextViewHost, Workspace, ServiceProvider, MarginSideEnum.Right);
        }
    }

    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(CodeNavMargin.MarginName + "Top")]
    [Order(After = PredefinedMarginNames.Top)]
    [MarginContainer(PredefinedMarginNames.Top)]
    [ContentType("CSharp")]
    [ContentType("Basic")]
    [ContentType("JavaScript")]
    [ContentType("TypeScript")]
    [TextViewRole(PredefinedTextViewRoles.Debuggable)]
    internal sealed class CodeNavTopFactory : IWpfTextViewMarginProvider
    {
        [Import(typeof(SVsServiceProvider))]
        #pragma warning disable CS0649
        private IServiceProvider ServiceProvider;
        #pragma warning restore CS0649

        [Import(typeof(VisualStudioWorkspace))]
        private VisualStudioWorkspace Workspace { get; set; }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();

            var margin = CodeNavFactory.CreateMargin(wpfTextViewHost, Workspace, ServiceProvider, MarginSideEnum.Top);
            new NavBarOverrider(margin as CodeNavMargin);

            return margin;
        }
    }

    internal static class CodeNavFactory
    {
        public static IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, 
            VisualStudioWorkspace visualStudioWorkspace, IServiceProvider serviceProvider, MarginSideEnum side)
        {
            if (Settings.Default.MarginSide != side) return null;

            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();

            var dte = (DTE)serviceProvider.GetService(typeof(DTE));
            var outliningManagerService = OutliningHelper.GetOutliningManagerService(serviceProvider);

            var codeNav = new CodeNavMargin(wpfTextViewHost, dte, outliningManagerService, visualStudioWorkspace, side);

            return codeNav;
        }
    } 
}
