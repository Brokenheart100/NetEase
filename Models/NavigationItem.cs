using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace NetEase.Models
{
    public partial class NavigationItem : ObservableObject
    {
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public Type ViewModelType { get; set; }

        [ObservableProperty]
        private bool _isSelected;
        public ObservableCollection<NavigationItem> Children { get; set; } = [];
        public NavigationItemType ItemType { get; set; } = NavigationItemType.Default;
    }
    public enum NavigationItemType
    {
        Default, // 普通导航项
        Header,  // 分区标题 (如 "我的")
        Separator // 分割线
    }
}
