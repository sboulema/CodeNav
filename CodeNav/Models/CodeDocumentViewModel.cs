using System.Collections.Generic;
using System.ComponentModel;
using Caliburn.Micro;
using CodeNav.Properties;

namespace CodeNav.Models
{
    public class CodeDocumentViewModel : PropertyChangedBase
    {
        public List<CodeItem> CodeDocument { get; set; }
        public double MaxWidth { get; set; }
        public double MaxItemWidth { get; set; }

        public void LoadMaxWidth()
        {
            MaxWidth = Settings.Default.Width - 13;
            MaxItemWidth = Settings.Default.Width - 27;
            OnPropertyChanged(new PropertyChangedEventArgs("MaxWidth"));
            OnPropertyChanged(new PropertyChangedEventArgs("MaxItemWidth"));
        }

        public void LoadCodeDocument(List<CodeItem> codeDocument)
        {
            CodeDocument = codeDocument;
            OnPropertyChanged(new PropertyChangedEventArgs("CodeDocument"));
        }
    }
}
