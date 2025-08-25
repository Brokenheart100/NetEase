using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetEase.Converters;

namespace NetEase.ViewModels
{
    // 1. 继承自 TabbedViewModelBase
    public class FriendsViewModel : TabbedViewModelBase
    {
        // 2. 不再需要重复定义 Tabs 和 SelectedTab 属性，
        //    它们已经从基类继承了。

        // 3. 构造函数可以为空，或者用来注入它自己特有的依赖
        public FriendsViewModel()
        {
            // 基类的构造函数会自动被调用
        }

        // 4. 【必须】实现基类中定义的抽象方法 CreateTabs
        protected override void CreateTabs()
        {
            Tabs.Add(new TabItemViewModel { Header = "联系人", Count = 11, ContentViewModel = new ContactsViewModel() });
            Tabs.Add(new TabItemViewModel { Header = "消息", Count = 56, ContentViewModel = new ChatViewModel()});
            Tabs.Add(new TabItemViewModel { Header = "文件", Count = 33, ContentViewModel = new SessionsViewModel() });
        }
    }
}
