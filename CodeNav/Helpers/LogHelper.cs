using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace CodeNav.Helpers
{
    public static class LogHelper
    {
        private static TelemetryClient _client;
        private const string InstrumentationKey = "0913ac4a-1127-4d28-91cf-07673e70200f";

        public static void GetClient()
        {
            _client = new TelemetryClient();
            _client.Context.Session.Id = Guid.NewGuid().ToString();
            _client.InstrumentationKey = InstrumentationKey;
            _client.Context.Component.Version = GetExecutingAssemblyVersion().ToString();

            byte[] enc = Encoding.UTF8.GetBytes(Environment.UserName + Environment.MachineName);
            using (var crypto = new MD5CryptoServiceProvider())
            {
                byte[] hash = crypto.ComputeHash(enc);
                _client.Context.User.Id = Convert.ToBase64String(hash);
            }
        }

        public static void Log(string message, Exception exception = null, 
            object additional = null, string language = null)
        {
            if (_client == null)
            {
                GetClient();
            }

            var properties = new Dictionary<string, string>
            {
                { "version", GetExecutingAssemblyVersion().ToString() },
                { "message", JsonConvert.SerializeObject(message) },
                { "language", language },
                { "additional", JsonConvert.SerializeObject(additional) },
            };

            if (exception == null)
            {
                _client.TrackEvent(message, properties);
            }
            else
            {
                _client.TrackException(exception, properties);
            }
        }

        private static Version GetExecutingAssemblyVersion()
        {
            var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            // read what's defined in [assembly: AssemblyFileVersion("1.2.3.4")]
            return new Version(ver.ProductMajorPart, ver.ProductMinorPart, ver.ProductBuildPart, ver.ProductPrivatePart);
        }
    }
}
