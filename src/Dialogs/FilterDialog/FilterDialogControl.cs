using Microsoft.VisualStudio.Extensibility.UI;

namespace CodeNav.Dialogs.FilterDialog;

internal class FilterDialogControl(object? dataContext, SynchronizationContext? synchronizationContext = null)
    : RemoteUserControl(dataContext, synchronizationContext)
{
}
