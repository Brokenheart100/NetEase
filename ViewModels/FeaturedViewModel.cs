using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.ViewModels
{
    public partial class FeaturedViewModel : BaseViewModel
    {
        // 存储所有 Tab 页的集合
        public ObservableCollection<TabItemViewModel> Tabs { get; }

        // 存储当前被选中的 Tab 页
        [ObservableProperty]
        private TabItemViewModel _selectedTab;

        public FeaturedViewModel()
        {
            // 在构造函数中，创建所有需要的 Tab 页面
            Tabs = new ObservableCollection<TabItemViewModel>
            {
                // DI 容器可以被用来创建这些子 ViewModel，但为了简单，我们先直接 new
                new TabItemViewModel { Header = "精选", ContentViewModel = new PodcastViewModel() },
            };

            // 默认选中第一个 Tab
            SelectedTab = Tabs.FirstOrDefault();
        }
    }
}
