using Microsoft.VisualStudio.Extensibility.UI;

namespace CodeNav.OutOfProc.ToolWindows;

internal class CodeNavToolWindowControl : RemoteUserControl
{
    public CodeNavToolWindowControl(object? dataContext, SynchronizationContext? synchronizationContext = null)
        : base(dataContext, synchronizationContext)
    {
        ResourceDictionaries.AddEmbeddedResource("CodeNav.OutOfProc.ToolWindows.Templates.ItemContextMenu.xaml");
        ResourceDictionaries.AddEmbeddedResource("CodeNav.OutOfProc.ToolWindows.Templates.HighlightNameStyle.xaml");
        ResourceDictionaries.AddEmbeddedResource("CodeNav.OutOfProc.ToolWindows.Templates.ToolbarStyles.xaml");
        ResourceDictionaries.AddEmbeddedResource("CodeNav.OutOfProc.ToolWindows.Templates.ExpanderStyles.xaml");
        ResourceDictionaries.AddEmbeddedResource("CodeNav.OutOfProc.ToolWindows.Templates.CodeItemStyles.xaml");
    }
}
