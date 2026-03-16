using Microsoft.VisualStudio.ApplicationInsights;
using Microsoft.VisualStudio.ApplicationInsights.Channel;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace CodeNav.OutOfProc.Helpers;

public static class LogHelper
{
    private static TelemetryClient? _client;
    private const string InstrumentationKey = "0913ac4a-1127-4d28-91cf-07673e70200f";

    public static void GetClient()
    {
        _client = new TelemetryClient(new()
        {
            InstrumentationKey = InstrumentationKey,
            TelemetryChannel = new InMemoryChannel(),
        })
        {
            InstrumentationKey = InstrumentationKey,
        };

        _client.Context.Component.Version = GetExecutingAssemblyVersion().ToString();
    }

    public static void Log(string message, Exception? exception = null, 
        object? additional = null)
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
                { "additional", JsonSerializer.Serialize(additional) },
            };

            if (exception == null)
            {
                _client.TrackEvent(message, properties);
            }
            else
            {
                _client.TrackException(exception, properties);
            }

            _client.Flush();
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
