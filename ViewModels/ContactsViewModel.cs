using CommunityToolkit.Mvvm.ComponentModel;
using NetEase.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.ViewModels
{
    public partial class ContactsViewModel : BaseViewModel
    {
        public ObservableCollection<FriendItem> FriendGroups { get; }
        // 为 ChatView 创建一个子 ViewModel
        public ChatViewModel ChatVM { get; }
        public class FriendItem // 这个模型可以复用
        {
            public string Name { get; set; }
            public string AvatarUrl;
            public BaseViewModel ContentViewModel { get; set; }
            ObservableCollection<FriendItem> Children;
        }
        public ContactsViewModel()
        {
            // 在构造函数中初始化
            ChatVM = new ChatViewModel();
        }
        // 持有子 ViewModel 的实例
        // 用于控制右侧内容的属性
        [ObservableProperty]
        private BaseViewModel _currentFriendView;
    }
}
