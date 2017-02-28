using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using Microsoft.VisualStudio.PlatformUI;

namespace CodeNav.Models
{
    public class CodeItem : PropertyChangedBase
    {
        public CodeItem()
        {
            _clickItemCommand = new DelegateCommand(ClickItem);
        }

        public string Name { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string IconPath { get; set; }
        public string Id { get; set; }
        public string Tooltip { get; set; }
        internal string FullName;
        internal CodeItemKindEnum Kind;
        internal CodeItemAccessEnum Access;
        internal CodeViewUserControl Control;

        #region Fonts
        private float _fontSize;
        public float FontSize
        {
            get
            {
                return _fontSize;
            }
            set
            {
                _fontSize = value;
                NotifyOfPropertyChange();
            }
        }

        private float _parameterFontSize;
        public float ParameterFontSize
        {
            get
            {
                return _parameterFontSize;
            }
            set
            {
                _parameterFontSize = value;
                NotifyOfPropertyChange();
            }
        }

        private FontFamily _fontFamily;
        public FontFamily FontFamily
        {
            get
            {
                return _fontFamily;
            }
            set
            {
                _fontFamily = value;
                NotifyOfPropertyChange();
            }
        }

        private FontStyle _fontStyle;
        public FontStyle FontStyle
        {
            get
            {
                return _fontStyle;
            }
            set
            {
                _fontStyle = value;
                NotifyOfPropertyChange();
            }
        }

        private FontWeight _fontWeight;
        public FontWeight FontWeight
        {
            get
            {
                return _fontWeight;
            }
            set
            {
                _fontWeight = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region IsVisible
        private Visibility _visibility;
        public Visibility IsVisible
        {
            get
            {
                return _visibility;
            }
            set
            {
                _visibility = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Foreground
        private SolidColorBrush _foreground;
        public SolidColorBrush Foreground
        {
            get
            {
                return _foreground;
            }
            set
            {
                _foreground = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Background
        private SolidColorBrush _highlightBackground;
        public SolidColorBrush HighlightBackground
        {
            get
            {
                return _highlightBackground;
            }
            set
            {
                _highlightBackground = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        private readonly DelegateCommand _clickItemCommand;
        internal string ShortId;

        public ICommand ClickItemCommand => _clickItemCommand;

        public void ClickItem(object startLine)
        {
            Control.SelectLine(startLine);
        }
    }

    public class CodeItemComparer : IEqualityComparer<CodeItem>
    {

        public bool Equals(CodeItem x, CodeItem y)
        {
            //Check whether the objects are the same object. 
            if (ReferenceEquals(x, y)) return true;

            //Check whether the products' properties are equal. 
            var membersAreEqual = true;
            if (x is CodeClassItem && y is CodeClassItem)
            {
                membersAreEqual = (x as CodeClassItem).Members.SequenceEqual((y as CodeClassItem).Members, new CodeItemComparer());
            }
            if (x is CodeNamespaceItem && y is CodeNamespaceItem)
            {
                membersAreEqual = (x as CodeNamespaceItem).Members.SequenceEqual((y as CodeNamespaceItem).Members, new CodeItemComparer());
            }

            return x != null && y != null && x.Id.Equals(y.Id) && membersAreEqual;
        }

        public int GetHashCode(CodeItem obj)
        {
            //Get hash code for the Name field if it is not null. 
            int hashProductName = obj.Name == null ? 0 : obj.Name.GetHashCode();

            //Get hash code for the Code field. 
            int hashProductCode = obj.Id.GetHashCode();

            //Calculate the hash code for the product. 
            return hashProductName ^ hashProductCode;
        }
    }
}
