using CodeNav.Services;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Helpers;

namespace CodeNav.OutOfProc.Services;

public class WindowFrameService : DisposableObject
{
    private readonly VisualStudioExtensibility _extensibility;
    private readonly Task _initializationTask;
    private IInProcService? _inProcService;

    public WindowFrameService(VisualStudioExtensibility extensibility)
    {
        _extensibility = extensibility;
        _initializationTask = Task.Run(InitializeAsync);
    }

    public async Task SubscribeToWindowFrameEvents()
    {
        try
        {
            Assumes.NotNull(_inProcService);

            // Subscribe to window frame events
            await _inProcService.SubscribeToWindowFrameEvents();
        }
        catch (Exception e)
        {
            // TODO: Add logging
        }
    }

    private async Task InitializeAsync()
    {
        (_inProcService as IDisposable)?.Dispose();
        _inProcService = await _extensibility
            .ServiceBroker
            .GetProxyAsync<IInProcService>(IInProcService.Configuration.ServiceDescriptor, cancellationToken: default);
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        if (isDisposing)
        {
            (_inProcService as IDisposable)?.Dispose();
        }
    }
}
