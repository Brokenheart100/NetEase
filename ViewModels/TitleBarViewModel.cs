using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetEase.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging; // 引入 Mouse 类的命名空间

namespace NetEase.ViewModels
{
    public partial class TitleBarViewModel : BaseViewModel
    {
        private readonly ICacheService _cacheService;

        // 搜索框文本属性
        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private string _avatarUrl;

        [ObservableProperty]
        private ImageSource? _avatarImageSource;

        [ObservableProperty]
        private bool _isUserProfileOpen;
        public CurrentUserStateService CurrentUser { get; }
        public UserProfileViewModel UserProfileVM { get; }
        // 构造函数现在是空的，因为命令是自动生成的
        public Action RequestLogoutAction { get; set; }
        public TitleBarViewModel(UserProfileViewModel userProfileVM, CurrentUserStateService currentUserState, ICacheService cacheService)
        {
            UserProfileVM = userProfileVM;
            CurrentUser = currentUserState;
            UserProfileVM.RequestLogoutAction = () => RequestLogoutAction?.Invoke();
            _cacheService = cacheService;
        }

        public Action<string> SearchRequested;

        async partial void OnAvatarUrlChanged(string? value)
        {
            // 如果URL为空，则不进行任何操作，UI会使用FallbackValue
            if (string.IsNullOrWhiteSpace(value))
            {
                AvatarImageSource = null;
                return;
            }

            // 1. 异步调用缓存服务获取本地文件路径
            string? localImagePath = await _cacheService.GetFileAsync(value);

            if (localImagePath != null)
            {
                // 2. 如果获取成功，创建一个 BitmapImage 对象
                // 确保这个操作在UI线程上执行
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(localImagePath);
                    // CacheOption.OnLoad 确保图片加载后立即释放对文件的锁定
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    // 3. 将创建好的 ImageSource 赋值给UI绑定的属性
                    AvatarImageSource = bitmap;
                });
            }
            else
            {
                // 如果下载失败，也可以在这里设置一个“加载失败”的默认图
                AvatarImageSource = null;
            }
        }
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
        private static void DragWindow(Window window)
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
        private static void MinimizeWindow(Window window)
        {
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        [RelayCommand]
        private static void MaximizeWindow(Window window)
        {
            if (window != null)
            {
                window.WindowState = window.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
        }

        [RelayCommand]
        private static void CloseWindow(Window window)
        {
            if (window != null)
            {
                window.Close();
            }
        }
    }
}