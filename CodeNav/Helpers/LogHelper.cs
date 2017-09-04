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

        public static void Log(string message)
        {
            #if DEBUG
            Logger.Log(message);
            #endif
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

        public static void Log(Exception e)
        {
            if (_client == null)
            {
                _client = GetClient();
            }

            var logEvent = new LogglyEvent();
            logEvent.Data.Add("version", GetExecutingAssemblyVersion());
            logEvent.Data.Add("exception", e);
            _client.Log(logEvent);
        }

        public static void Log(Exception e, object context)
        {
            if (_client == null)
            {
                _client = GetClient();
            }

            var logEvent = new LogglyEvent();
            logEvent.Data.Add("version", GetExecutingAssemblyVersion());
            logEvent.Data.Add("exception", e);
            logEvent.Data.Add("context", context);
            _client.Log(logEvent);
        }

        public static void Log(string message, Exception e)
        {
            if (_client == null)
            {
                _client = GetClient();
            }

            var logEvent = new LogglyEvent();
            logEvent.Data.Add("version", GetExecutingAssemblyVersion());
            logEvent.Data.Add("message", message);
            logEvent.Data.Add("exception", e);
            _client.Log(logEvent);
        }

        public static void Log(object log)
        {
            if (_client == null)
            {
                _client = GetClient();
            }

            var logEvent = new LogglyEvent();
            logEvent.Data.Add("version", GetExecutingAssemblyVersion());
            logEvent.Data.Add("message", log);
            _client.Log(logEvent);
        }

        private static Version GetExecutingAssemblyVersion()
        {
            var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            // read what's defined in [assembly: AssemblyFileVersion("1.2.3.4")]
            return new Version(ver.ProductMajorPart, ver.ProductMinorPart, ver.ProductBuildPart, ver.ProductPrivatePart);
        }

        public static string GetDocumentName(EnvDTE.Window window)
        {
            var name = "";
            try
            {
                name = window.Document.Name;
            }
            catch (ObjectDisposedException e)
            {
                // Ignore exception we only got it trying to log
            }
            return name;
        }
    }
}
