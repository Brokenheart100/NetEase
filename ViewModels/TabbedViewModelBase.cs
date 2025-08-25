using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.ViewModels
{
    public class TabItemViewModel
    {
        public string Header { get; set; }
        public int Count { get; set; }
        public BaseViewModel ContentViewModel { get; set; }
    }
    // 1. 将基类声明为 abstract
    public abstract partial class TabbedViewModelBase : BaseViewModel
    {
        // 2. 将所有共享的属性和逻辑放在这里
        public ObservableCollection<TabItemViewModel> Tabs { get; }

        [ObservableProperty]
        private TabItemViewModel _selectedTab;

        protected TabbedViewModelBase()
        {
            // 在基类的构造函数中初始化集合
            Tabs = new ObservableCollection<TabItemViewModel>();

            // 调用一个抽象方法来填充具体的 Tab 内容
            CreateTabs();

            // 默认选中第一个 Tab
            SelectedTab = Tabs.FirstOrDefault();
        }

        // 3. 定义一个【抽象方法】
        //    这个方法没有实现体，它强制所有【子类】都必须提供自己的实现，
        //    来定义它们各自的 Tab 应该是什么。
        protected abstract void CreateTabs();
    }
}
