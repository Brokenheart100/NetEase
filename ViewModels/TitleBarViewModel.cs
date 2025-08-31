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
        private bool _isUserProfileOpen;

        // 【新增】用户个人资料浮窗的ViewModel
        public UserProfileViewModel UserProfileVM { get; }
        // 构造函数现在是空的，因为命令是自动生成的
        public Action RequestLogoutAction { get; set; }
        public TitleBarViewModel(UserProfileViewModel userProfileVM)
        {
            UserProfileVM = userProfileVM;
            UserProfileVM.RequestLogoutAction = () => RequestLogoutAction?.Invoke();
        }
        [RelayCommand]
        private void ToggleUserProfile()
        {
            IsUserProfileOpen = !IsUserProfileOpen;
        }
        // --- 命令 (由源生成器自动创建) ---

        // 方法名从 DragWindow 改为 DragWindowCommand (或者保持原名，但XAML绑定要写成 DragWindowCommand)
        // 为了清晰，我们保持原名，让生成器自动添加 "Command" 后缀
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