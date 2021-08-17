namespace CodeNav.Models
{
    public class CodeDepthGroupItem : CodeClassItem
    {
        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }
    }
}
