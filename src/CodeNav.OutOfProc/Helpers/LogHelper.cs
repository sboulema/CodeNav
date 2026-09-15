using CodeNav.OutOfProc.Services;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace CodeNav.OutOfProc.Helpers;

public static class LogHelper
{
    private static TelemetryClient? _client;
    private const string ConnectionString = "InstrumentationKey=0913ac4a-1127-4d28-91cf-07673e70200f;IngestionEndpoint=https://westeurope-0.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/;ApplicationId=d3364940-b523-44c6-b46f-ddd7a63f8d15";

    public static void GetClient()
    {
        var config = TelemetryConfiguration.CreateDefault();
        config.ConnectionString = ConnectionString;

        _client = new TelemetryClient(config);
    }

    public static async Task LogException(
        CodeDocumentViewModel codeDocumentViewModel,
        string message,
        Exception exception)
        => await LogException(codeDocumentViewModel.CodeDocumentService, message, exception);

    public static async Task LogException(
        CodeDocumentService? codeDocumentService,
        string message,
        Exception exception)
    {
        await WriteException(codeDocumentService, message, exception);

        if (codeDocumentService?.GlobalSettings?.EnableCrashAnalytics != true)
        {
            return;
        }

        LogToApplicationInsights(message, exception);
    }

    public async static Task LogInfo(CodeItem codeItem, string text)
    {
        if (codeItem.CodeDocumentViewModel?.CodeDocumentService?.LogService == null)
        {
            return;
        }

        await codeItem.CodeDocumentViewModel.CodeDocumentService.LogService.WriteInfo(codeItem.FilePath, text);
    }

    public async static Task LogWarning(
        CodeDocumentService? codeDocumentService,
        string message)
    {
        if (codeDocumentService?.LogService == null)
        {
            return;
        }

        await codeDocumentService.LogService.WriteWarning(message);
    }

    private async static Task WriteException(CodeDocumentService? codeDocumentService, string text, Exception exception)
    {
        if (codeDocumentService?.LogService == null)
        {
            return;
        }

        await codeDocumentService.LogService.WriteException(text, exception);
    }

    private static void LogToApplicationInsights(
        string message,
        Exception? exception = null)
    {
        try
        {
            if (_client == null)
            {
                GetClient();
            }

            if (_client == null)
            {
                return;
            }

            var properties = new Dictionary<string, string>
            {
                { "version", GetExecutingAssemblyVersion().ToString() },
                { "message", JsonSerializer.Serialize(message) },
            };

            if (exception == null)
            {
                _client.TrackEvent(message, properties);
            }
            else
            {
                _client.TrackException(exception, properties);
            }

            // Flush telemetry and wait briefly to ensure transmission
            _client.Flush();
            Thread.Sleep(100);
        }
        catch (Exception)
        {
            // Ignore errors while trying to log errors
        }
    }

    private static Version GetExecutingAssemblyVersion()
    {
        var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

        // read what's defined in [assembly: AssemblyFileVersion("1.2.3.4")]
        return new Version(ver.ProductMajorPart, ver.ProductMinorPart, ver.ProductBuildPart, ver.ProductPrivatePart);
    }
}
