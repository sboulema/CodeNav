using CodeNav.Models;
using Newtonsoft.Json;
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
                    _useXmlComments = General.Instance.UseXMLComments;
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
                    _filterRules = JsonConvert.DeserializeObject<List<FilterRule>>(General.Instance.FilterRules);
                }

                if (_filterRules == null)
                {
                    _filterRules = new List<FilterRule>();
                }

                return _filterRules;
            }
            set => _filterRules = value;
        }

        public static void SaveFilterRules()
        {
            General.Instance.FilterRules = JsonConvert.SerializeObject(FilterRules);
            General.Instance.Save();
        }

        public static void Refresh()
        {
            _useXmlComments = null;
            _filterRules = null;
        }
    }
}
