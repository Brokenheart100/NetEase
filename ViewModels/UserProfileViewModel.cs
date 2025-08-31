using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetEase.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NetEase.ViewModels
{
    public partial class UserProfileViewModel : BaseViewModel
    {
        // --- 用户基础信息 ---
        [ObservableProperty] private string _userName;
        [ObservableProperty] private string _avatarUrl;
        [ObservableProperty] private bool _isVip;
        [ObservableProperty] private string _vipInfo;

        // --- 用户统计数据 ---
        [ObservableProperty] private int _dynamicCount;
        [ObservableProperty] private int _followingCount;
        [ObservableProperty] private int _followerCount;
        [ObservableProperty] private string _level;

        // --- 签到信息 ---
        [ObservableProperty] private int _signInPoints;
        [ObservableProperty] private int _consecutiveSignInDays;
        public Action RequestLogoutAction { get; set; }
        // 可以在 ViewModels/UserProfileViewModel.cs 文件中定义
        public class SignInStep
        {
            public string DayLabel { get; set; } // 例如 "第1天"
            public bool IsCompleted { get; set; } // 是否已签到
            public bool IsSpecialReward { get; set; } // 是否是特殊奖励节点（如第28天）
        }
        public ObservableCollection<SignInStep> SignInProgress { get; }
        public class MenuItem
        {
            public string Icon { get; set; }
            public string Text { get; set; }
            public bool IsVip { get; set; }
            public bool HasNotification { get; set; }
            public int NotificationCount { get; set; }
            public string SubText { get; set; }
        }
        public ObservableCollection<MenuItem> MenuItems1 { get; }
        public ObservableCollection<MenuItem> MenuItems2 { get; }
        public ObservableCollection<MenuItem> MenuItems3 { get; }
        public UserProfileViewModel(AuthService authService)
        {

            // 初始化时从 AuthService 加载当前用户信息
            UserName = authService.GetCurrentUserName() ?? "未登录";

            // 填充示例数据
            IsVip = true;
            VipInfo = "¥4.8/首月";
            DynamicCount = 2;
            FollowingCount = 194;
            FollowerCount = 36;
            Level = "Lv.9";
            SignInPoints = 0;
            ConsecutiveSignInDays = 1;
            MenuItems1 = new ObservableCollection<MenuItem>
            {
                new MenuItem { Icon = "", Text = "会员中心", IsVip = true },
                new MenuItem { Icon = "", Text = "商城", HasNotification = true },
                new MenuItem { Icon = "", Text = "福利活动中心", NotificationCount = 2, SubText = "参与活动得黑胶VIP" }
            };
            SignInProgress = new ObservableCollection<SignInStep>
            {
                // 假设用户今天还没签到，所以都是 false
                new SignInStep { DayLabel = "第1天", IsCompleted = false },
                new SignInStep { DayLabel = "第3天", IsCompleted = false },
                new SignInStep { DayLabel = "第7天", IsCompleted = false },
                new SignInStep { DayLabel = "第14天", IsCompleted = false },
                new SignInStep { DayLabel = "第28天", IsCompleted = false, IsSpecialReward = true }
            };
        }

        [RelayCommand]
        private void Logout()
        {
            RequestLogoutAction?.Invoke();
        }

        [RelayCommand]
        private void SignIn()
        {
            System.Windows.MessageBox.Show("签到成功！");
        }
    }
}
