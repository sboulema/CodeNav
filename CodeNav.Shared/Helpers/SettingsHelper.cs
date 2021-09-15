using CodeNav.Models;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Drawing;

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

        private static Font _font;
        public static Font Font
        {
            get
            {
                if (_font == null)
                {
                    _font = new Font(General.Instance.FontFamilyName, General.Instance.FontSize, General.Instance.FontStyle);
                }
                return _font;
            }
            set => _font = value;
        }

        private static ObservableCollection<FilterRule> _filterRules;
        public static ObservableCollection<FilterRule> FilterRules
        {
            get => LoadFilterRules();
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
            _font = null;
        }

        private static ObservableCollection<FilterRule> LoadFilterRules()
        {
            if (_filterRules != null)
            {
                return _filterRules;
            }

            try
            {
                _filterRules = JsonConvert.DeserializeObject<ObservableCollection<FilterRule>>(General.Instance.FilterRules);
            }
            catch (Exception)
            {
                // Ignore error while loading filter rules
                _filterRules = new ObservableCollection<FilterRule>();
            }

            return _filterRules;
        }
    }
}
