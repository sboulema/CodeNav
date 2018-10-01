using Loggly;
using Loggly.Config;
using System;
using System.Diagnostics;
using System.Reflection;

namespace CodeNav.Helpers
{
    public static class LogHelper
    {
        private static ILogglyClient _client;
        private const string CustomerToken = "4a0f1123-41cd-4f9a-bca0-835914aa51d3";
        private const string ApplicationName = "CodeNav";

        private const string TelemetryKey = "0913ac4a-1127-4d28-91cf-07673e70200f";

        public static void Initialize(IServiceProvider serviceProvider)
        {
            Logger.Initialize(serviceProvider, ApplicationName, GetExecutingAssemblyVersion().ToString(), TelemetryKey);
        }

        public static ILogglyClient GetClient()
        {
            var config = LogglyConfig.Instance;
            config.CustomerToken = CustomerToken;
            config.ApplicationName = ApplicationName;

            config.Transport.EndpointHostname = "logs-01.loggly.com";
            config.Transport.EndpointPort = 443;
            config.Transport.LogTransport = LogTransport.Https;

            var ct = new ApplicationNameTag();
            ct.Formatter = "application-{0}";
            config.TagConfig.Tags.Add(ct);

            return new LogglyClient();
        }

        public static void Log(string message, Exception exception = null, 
            object additional = null, string language = null)
        {
            if (_client == null)
            {
                _client = GetClient();
            }

            var logEvent = new LogglyEvent();
            logEvent.Data.Add("version", GetExecutingAssemblyVersion());
            logEvent.Data.Add("message", message);

            if (exception != null)
            {
                logEvent.Data.Add("exception", exception);
            }

            if (additional != null)
            {
                logEvent.Data.Add("additional", additional);
            }

            _client.Log(logEvent);
            Logger.Log(exception);
        }

        private static Version GetExecutingAssemblyVersion()
        {
            var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            // read what's defined in [assembly: AssemblyFileVersion("1.2.3.4")]
            return new Version(ver.ProductMajorPart, ver.ProductMinorPart, ver.ProductBuildPart, ver.ProductPrivatePart);
        }
    }
}
