using CodeNav.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        private static ObservableCollection<FilterRule> _filterRules;
        public static ObservableCollection<FilterRule> FilterRules
        {
            get
            {
                if (_filterRules == null)
                {
                    _filterRules = JsonConvert.DeserializeObject<ObservableCollection<FilterRule>>(General.Instance.FilterRules);
                }

                if (_filterRules == null)
                {
                    _filterRules = new ObservableCollection<FilterRule>();
                }

                return _filterRules;
            }
            set => _filterRules = value;
        }

        public static void SaveFilterRules(ObservableCollection<FilterRule> filterRules)
        {
            General.Instance.FilterRules = JsonConvert.SerializeObject(filterRules);
            General.Instance.Save();
        }

        public static void Refresh()
        {
            _useXmlComments = null;
            _filterRules = null;
        }
    }
}
