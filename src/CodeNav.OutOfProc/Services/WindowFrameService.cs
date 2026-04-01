using CodeNav.Services;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Helpers;

namespace CodeNav.OutOfProc.Services;

public class WindowFrameService : DisposableObject
{
    private readonly VisualStudioExtensibility _extensibility;
    private readonly OutputWindowService _outputWindowService;
    private readonly Task _initializationTask;
    private IInProcService? _inProcService;
    
    private bool _isSubscribed;

    public WindowFrameService(
        VisualStudioExtensibility extensibility,
        OutputWindowService outputWindowService)
    {
        _extensibility = extensibility;
        _initializationTask = Task.Run(InitializeAsync);
        _outputWindowService = outputWindowService;
    }

    public async Task SubscribeToWindowFrameEvents()
    {
        if (_isSubscribed)
        {
            await _outputWindowService.WriteLine("[Info] Already subscribed to window frame events.");
            return;
        }

        try
        {
            Assumes.NotNull(_inProcService);

            // Subscribe to window frame events
            await _inProcService.SubscribeToWindowFrameEvents();

            await _outputWindowService.WriteLine("[Info] Subscribed to window frame events.");

            _isSubscribed = true;
        }
        catch (Exception e)
        {
            await _outputWindowService.WriteException("Exception subscribing to window frame events", e);
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
