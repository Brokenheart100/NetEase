using CommunityToolkit.Mvvm.ComponentModel;
using NetEase.Models;
using System.Diagnostics;

namespace NetEase.ViewModels.ChatViewModels
{
    // 1. 继承自 TabbedViewModelBase
    public partial class FriendsViewModel : TabbedViewModelBase
    {
        [ObservableProperty]
        private object _currentTabViewModel;

        private readonly ContactsViewModel _contactsVM;
        private readonly ChatViewModel _chatVM;
        private readonly SessionsViewModel _sessionsVM;

        public FriendsViewModel(ContactsViewModel contactsVM, ChatViewModel chatVM, SessionsViewModel sessionsVM)
        {
            _contactsVM = contactsVM;
            _chatVM = chatVM;
            _sessionsVM = sessionsVM;

            // 【新增】订阅 ContactsViewModel 的事件
            // 当 ContactsViewModel 请求导航时，执行我们的 OnRequestNavigateToChat 方法
            _contactsVM.RequestNavigateToChatAction = OnRequestNavigateToChat;
            InitializeTabs();
        }
        public async Task SyncDataAsync()
        {
            Debug.WriteLine("FriendsViewModel: SyncDataAsync() Syncing data after login...");
            if (_chatVM != null)
            {
                await _chatVM.InitializeAsync();
            }
            // 1. 如果Tabs是空的，就先创建它们
            if (!Tabs.Any())
            {
                InitializeTabs();
            }

            // 2. 调用 ContactsViewModel 的数据同步方法
            //    确保 ContactsViewModel 也有一个类似的方法
            if (_contactsVM != null)
            {
                await _contactsVM.SyncDataAsync();
            }

            // 可以在这里更新Tab的角标，比如未读消息数
        }

        // 【新增】事件处理方法
        private void OnRequestNavigateToChat(Friend friendToChat)
        {
            if (friendToChat == null) return;

            // 1. 切换到“消息”Tab
            //    我们通过查找ContentViewModel的类型来找到对应的Tab
            var chatTab = Tabs.FirstOrDefault(t => t.ContentViewModel is ChatViewModel);
            if (chatTab != null)
            {
                SelectedTab = chatTab;
            }

            // 2. 通知ChatViewModel开始新的聊天
            //    ChatViewModel需要有一个方法来接收这个指令
            _chatVM.StartChatWith(friendToChat);
        }
        // 4. 【必须】实现基类中定义的抽象方法 CreateTabs
        protected override void CreateTabs()
        {
            Tabs.Add(new TabItemViewModel { Header = "联系人", Count = 11, ContentViewModel = _contactsVM });
            Tabs.Add(new TabItemViewModel { Header = "消息", Count = 56, ContentViewModel = _chatVM });
            Tabs.Add(new TabItemViewModel { Header = "文件", Count = 33, ContentViewModel = _sessionsVM });
        }
    }
}
