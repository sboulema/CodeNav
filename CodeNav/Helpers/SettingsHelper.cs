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

        private static bool? _hideItemsWithoutChildren;
        public static bool HideItemsWithoutChildren
        {
            get
            {
                if (_hideItemsWithoutChildren == null)
                {
                    _hideItemsWithoutChildren = Settings.Default.HideItemsWithoutChildren;
                }
                return _hideItemsWithoutChildren.Value;
            }
            set => _hideItemsWithoutChildren = value;
        }

        public static void Refresh()
        {
            _useXmlComments = null;
            _hideItemsWithoutChildren = null;
        }
    }
}
