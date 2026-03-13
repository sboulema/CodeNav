using Microsoft.VisualStudio.Extensibility.UI;

namespace CodeNav.ToolWindows;

internal class CodeNavToolWindowControl : RemoteUserControl
{
    public CodeNavToolWindowControl(object? dataContext, SynchronizationContext? synchronizationContext = null)
        : base(dataContext, synchronizationContext)
    {
        ResourceDictionaries.AddEmbeddedResource("CodeNav.ToolWindows.Templates.ItemContextMenu.xaml");
        ResourceDictionaries.AddEmbeddedResource("CodeNav.ToolWindows.Templates.HighlightNameStyle.xaml");
        ResourceDictionaries.AddEmbeddedResource("CodeNav.ToolWindows.Templates.ToolbarStyles.xaml");
        ResourceDictionaries.AddEmbeddedResource("CodeNav.ToolWindows.Templates.ExpanderStyles.xaml");
        ResourceDictionaries.AddEmbeddedResource("CodeNav.ToolWindows.Templates.CodeItemStyles.xaml");
    }
}
