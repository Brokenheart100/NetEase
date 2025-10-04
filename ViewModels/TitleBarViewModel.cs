using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Input; // 引入 Mouse 类的命名空间

namespace NetEase.ViewModels
{
    public partial class TitleBarViewModel : BaseViewModel
    {
        // 搜索框文本属性
        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private string _avatarUrl;

        [ObservableProperty]
        private bool _isUserProfileOpen;

        public UserProfileViewModel UserProfileVM { get; }
        // 构造函数现在是空的，因为命令是自动生成的
        public Action RequestLogoutAction { get; set; }
        public TitleBarViewModel(UserProfileViewModel userProfileVM)
        {
            UserProfileVM = userProfileVM;
            UserProfileVM.RequestLogoutAction = () => RequestLogoutAction?.Invoke();
        }

        public Action<string> SearchRequested;

        // 2. 创建搜索命令
        [RelayCommand]
        private void Search()
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                // 触发事件，将搜索词传递出去
                SearchRequested?.Invoke(SearchText);
            }
        }
        [RelayCommand]
        private void ToggleUserProfile()
        {
            IsUserProfileOpen = !IsUserProfileOpen;
        }

        [RelayCommand]
        private void DragWindow(Window window)
        {
            if (window != null)
            {
                // 检查鼠标左键状态
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    window.DragMove();
                }
            }
        }

        [RelayCommand]
        private void MinimizeWindow(Window window)
        {
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        [RelayCommand]
        private void MaximizeWindow(Window window)
        {
            if (window != null)
            {
                window.WindowState = window.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
        }

        [RelayCommand]
        private void CloseWindow(Window window)
        {
            if (window != null)
            {
                window.Close();
            }
        }
    }
}