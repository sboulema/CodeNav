using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Documents;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.Helpers;

namespace CodeNav.OutOfProc.Services;

#pragma warning disable VSEXTPREVIEW_OUTPUTWINDOW // Type is for evaluation purposes only and is subject to change or removal in future updates.

public class OutputWindowService : DisposableObject
{
    private readonly VisualStudioExtensibility _extensibility;
    private readonly Task _initializationTask;
    private OutputChannel? _outputChannel;

    public OutputWindowService(VisualStudioExtensibility extensibility)
    {
        _extensibility = extensibility;
        _initializationTask = Task.Run(InitializeAsync);
    }

    public async Task WriteInfo(ITextViewSnapshot textView, string text)
        => await WriteInfo(Path.GetFileName(textView.FilePath), text);

    public async Task WriteInfo(Uri? FilePath, string text)
        => await WriteInfo(Path.GetFileName(FilePath?.AbsolutePath), text);

    public async Task WriteInfo(string? fileName, string text)
        => await WriteLine($"[Info] [{fileName}] {text}");

    public async Task WriteLine(string text)
    {
        if (_outputChannel is null)
        {
            return;
        }

        await _outputChannel.WriteLineAsync(text);
    }

    public async Task WriteException(string message, Exception exception)
        => await WriteLine($"[Exception] {message}{Environment.NewLine}{exception.Message}{Environment.NewLine}{exception.StackTrace}");

    private async Task InitializeAsync()
    {
        _outputChannel = await _extensibility
            .Views()
            .Output
            .CreateOutputChannelAsync("CodeNav", default);
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        if (isDisposing)
        {
            _outputChannel?.Dispose();
        }
    }
}
