using CodeNav.Models;
using CodeNav.Properties;
using System.Collections.Generic;

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

        private static List<FilterRule> _filterRules;
        public static List<FilterRule> FilterRules
        {
            get
            {
                if (_filterRules == null)
                {
                    _filterRules = Settings.Default.FilterRules;
                }
                return _filterRules;
            }
            set => _filterRules = value;
        }

        public static void Refresh()
        {
            _useXmlComments = null;
            _filterRules = null;
        }
    }
}
