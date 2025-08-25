using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NetEase.Converters;
using static NetEase.Converters.RandomNumber;

namespace NetEase.ViewModels
{
    // 模型
    public class ChatSession { public string Name { get; set; } 
    public string AvatarUrl { get; set; } }
    public class GroupMember { public string Name { get; set; } 
    public string AvatarUrl { get; set; } }
    public class ChatMessage
    {
        public string SenderName { get; set; }
        public string Content { get; set; }
        public string AvatarUrl { get; set; }

        // --- 新增：标志位 ---
        public bool IsSentByMe { get; set; } = false; // 默认为 false
    }

    public class ChatViewModel : BaseViewModel
    {
        // 第一栏：会话列表
        public ObservableCollection<ChatSession> Sessions { get; }

        // 第二栏：当前会话的消息
        public ObservableCollection<ChatMessage> Messages { get; }

        // 第三栏：群成员列表
        public ObservableCollection<GroupMember> Members { get; }

        public ChatViewModel()
        {
            // 填充示例数据
            Sessions = new ObservableCollection<ChatSession>
            {
                new ChatSession { Name = "BKTV", AvatarUrl = GetRandomAvatarUrl()},
                new ChatSession { Name = "卡2小号", AvatarUrl = GetRandomAvatarUrl() }
            };

            Messages = new ObservableCollection<ChatMessage>
            {
                new ChatMessage { SenderName = "【管理员】AAA-信用卡贷款办理-职高-刘哥", Content = "好贵", AvatarUrl = GetRandomAvatarUrl() },
                new ChatMessage { SenderName = "【管理员】AAA-信用卡贷款办理-职高-刘哥", Content = "@AAA-银行业务办理-9... 你国庆婚礼吗", AvatarUrl = GetRandomAvatarUrl()  },
                new ChatMessage { SenderName = "【管理员】清真bot", Content = "地铁，归乡", AvatarUrl = GetRandomAvatarUrl() },
                new ChatMessage { SenderName = "我", Content = "好的，收到了！", AvatarUrl = GetRandomAvatarUrl(), IsSentByMe = true }
            };

            Members = new ObservableCollection<GroupMember>
            {
                new GroupMember { Name = "蒂派", AvatarUrl = GetRandomAvatarUrl() },
                new GroupMember { Name = "AAA-信用卡贷款办理-...", AvatarUrl = GetRandomAvatarUrl()},
                new GroupMember { Name = "清真bot", AvatarUrl = GetRandomAvatarUrl() }
            };
        }
    }
}
