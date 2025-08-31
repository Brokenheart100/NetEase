using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetEase.Converters;
using NetEase.Models;
using NetEase.Services;
using NetEase.ViewModels.ChatViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NetEase.Converters.RandomNumber;

namespace NetEase.ViewModels.ChatViewModels
{ // 这个模型用于 TreeView 的分组
    public class FriendGroup : ObservableObject
    {
        public string Name { get; set; }
        public ObservableCollection<Friend> Friends { get; set; } = new();
        // 将 _chatVM 重命名为 ChatVM，去除前缀“_”以符合命名规范
        public ChatViewModel ChatVM { get; }
    }
    public partial class ContactsViewModel : BaseViewModel
    {
        private readonly FriendService _friendService;
        public ObservableCollection<FriendGroup> FriendGroups { get; } = new();
        // 为 ChatView 创建一个子 ViewModel
        public ChatViewModel ChatVM { get; }
        // 持有子 ViewModel 的实例
        // 用于控制右侧内容的属性
        [ObservableProperty]
        private BaseViewModel _currentFriendView;
        // 【新增】定义一个事件，用于通知父ViewModel开始聊天
        // 参数 Friend 就是要聊天的对象
        public Action<Friend> RequestNavigateToChatAction { get; set; }
        // 构造函数参数和赋值同步修改
        public ContactsViewModel(FriendService friendService, ChatViewModel chatVM)
        {
            _friendService = friendService;
            ChatVM = chatVM; // 直接赋值
            LoadFriendsAsync();
        }
        [RelayCommand]
        private void SelectFriend(object selectedItem)
        {
            if (selectedItem == null) return;

            // 在方法内部进行类型检查
            // 只有当选中的项确实是一个 Friend 对象时，才执行后续逻辑
            if (selectedItem is Friend selectedFriend)
            {
                Debug.WriteLine($"Enter selectedFriend: {selectedFriend}");
                // 类型转换成功，selectedFriend 现在就是我们需要的 Friend 对象
                RequestNavigateToChatAction?.Invoke(selectedFriend);
            }
        }

        public async Task SyncDataAsync()
        {
            Debug.WriteLine("ContactsViewModel: Syncing friend list...");
            await LoadFriendsAsync();
        }

        private async Task LoadFriendsAsync()
        {
            Debug.WriteLine($"Enter LoadFriendsAsync()");
            var friendsList = await _friendService.GetFriendsAsync();
            if (friendsList != null)
            {
                // 简单地把所有好友都放在一个“我的好友”分组里
                var group = new FriendGroup { Name = $"我的好友 {friendsList.Count}" };
                foreach (var friend in friendsList)
                {
                    friend.AvatarUrl = GetRandomAvatarUrl();
                    group.Friends.Add(friend);
                    Debug.WriteLine($"friends :{friend}");
                }

                FriendGroups.Clear();
                FriendGroups.Add(group);
            }
        }
      
    }
}
