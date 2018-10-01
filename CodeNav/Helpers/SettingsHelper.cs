using CodeNav.Properties;

namespace CodeNav.Helpers
{
    public static class SettingsHelper
    {
        private static bool? _useXmlComments;
        public static bool UseXMLComments
        {
            get
            {
                if (_useXmlComments == null)
                {
                    _useXmlComments = Settings.Default.UseXMLComments;
                }
                return _useXmlComments.Value;
            }
            set => _useXmlComments = value;
        }
    }
}
