using CommunityToolkit.Mvvm.ComponentModel;

namespace NetEase.Models
{
    public partial class NavigationItem : ObservableObject
    {
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public Type ViewModelType { get; set; }

        [ObservableProperty]
        private bool _isSelected;
    }
}
