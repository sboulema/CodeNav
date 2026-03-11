using Microsoft.VisualStudio.Extensibility.UI;

namespace CodeNav.OutOfProc.Dialogs.SettingsDialog;

internal class SettingsDialogControl(object? dataContext, SynchronizationContext? synchronizationContext = null)
    : RemoteUserControl(dataContext, synchronizationContext)
{
}
