#nullable enable

using System.ComponentModel.Composition;
using CodeNav.Helpers;
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
    [ContentType("CSS")]
    [TextViewRole(PredefinedTextViewRoles.Debuggable)]  // This is to prevent the margin from loading in the diff view
    internal sealed class CodeNavLeftFactory : IWpfTextViewMarginProvider
    {
        public IWpfTextViewMargin? CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            return CodeNavFactory.CreateMargin(wpfTextViewHost, MarginSideEnum.Left);
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
    [ContentType("CSS")]
    [TextViewRole(PredefinedTextViewRoles.Debuggable)]   // This is to prevent the margin from loading in the diff view
    internal sealed class CodeNavRightFactory : IWpfTextViewMarginProvider
    {
        public IWpfTextViewMargin? CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            return CodeNavFactory.CreateMargin(wpfTextViewHost, MarginSideEnum.Right);
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
    [ContentType("CSS")]
    [TextViewRole(PredefinedTextViewRoles.Debuggable)]
    internal sealed class CodeNavTopFactory : IWpfTextViewMarginProvider
    {
        public IWpfTextViewMargin? CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            var margin = CodeNavFactory.CreateMargin(wpfTextViewHost, MarginSideEnum.Top);
            new NavBarOverrider(margin as CodeNavMargin);

            return margin;
        }
    }

    internal static class CodeNavFactory
    {
        public static IWpfTextViewMargin? CreateMargin(IWpfTextViewHost wpfTextViewHost, MarginSideEnum side)
        {
            if ((MarginSideEnum)General.Instance.MarginSide != side)
            {
                return null;
            }

            var codeNav = new CodeNavMargin(wpfTextViewHost, side);

            return codeNav;
        }
    } 
}
