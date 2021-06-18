namespace CodeNav.Models
{
    public class CodeDepthGroupItem : CodeClassItem
    {
        private int _selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
