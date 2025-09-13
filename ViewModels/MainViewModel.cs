using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NetEase.Models;
using NetEase.Services;
using NetEase.ViewModels.ChatViewModels;
using NetEase.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static NetEase.Converters.RandomNumber;

namespace NetEase.ViewModels
{
    /// <summary>
    /// 应用程序的主视图模型，负责全局状态管理、导航控制和各组件协调
    /// 是整个应用的核心协调者，连接各个子视图模型和服务
    /// </summary>
    public partial class MainViewModel : BaseViewModel
    {
        // 服务注入字段
        private readonly IServiceProvider _serviceProvider; // 服务提供者，用于获取其他服务/视图模型
        private readonly PlaylistService _playlistService; // 播放列表服务，用于管理播放列表数据
        private readonly AuthService _authService; // 认证服务，处理登录/登出逻辑
        private readonly FriendsViewModel _friendsViewModel; // 好友视图模型，管理好友相关功能
        private readonly SignalRService _signalRService; // SignalR服务，处理实时通信

        // --- 可观察属性（由CommunityToolkit.Mvvm自动生成通知） ---

        /// <summary>
        /// 当前显示的视图模型（绑定到UI的内容区域，控制显示哪个页面）
        /// </summary>
        [ObservableProperty]
        private BaseViewModel _currentView;

        /// <summary>
        /// 覆盖层（如登录窗口）是否可见
        /// </summary>
        [ObservableProperty]
        private bool _isOverlayVisible;

        /// <summary>
        /// 左侧边栏是否展开
        /// </summary>
        [ObservableProperty]
        private bool _isLeftSidebarExpanded = true;

        /// <summary>
        /// 当前选中的播放列表
        /// </summary>
        [ObservableProperty]
        private Playlist _selectedPlaylist;

        /// <summary>
        /// 标题栏视图模型（管理标题栏的状态和交互）
        /// </summary>
        public TitleBarViewModel TitleBarVM { get; }

        /// <summary>
        /// 播放器控制视图模型（管理播放器的状态和控制
        /// </summary>
        public PlayerControlViewModel PlayerControlVM { get; }
        /// <summary>
        /// 认证视图模型（管理登录/注册相关UI和逻辑）
        /// </summary>
        public AuthenticationViewModel AuthVM { get; }

        /// <summary>
        /// 用户收藏的播放列表集合（绑定到左侧边栏的播放列表区域）
        /// </summary>
        public ObservableCollection<Playlist> FavoritePlaylists { get; } = new();

        /// <summary>
        /// 主要导航项集合（如推荐、精选等，绑定到左侧导航栏）
        /// </summary>
        public ObservableCollection<NavigationItem> MainNavigationItems { get; }

        /// <summary>
        /// "我的音乐"分类下的导航项集合（如我喜欢的音乐、最近播放等）
        /// </summary>
        public ObservableCollection<NavigationItem> MyMusicNavigationItems { get; }

        /// <summary>
        /// "更多"分类下的导航项集合（如我的收藏、云盘等）
        /// </summary>
        public ObservableCollection<NavigationItem> MoreNavigationItems { get; }

        /// <summary>
        /// 歌曲详情面板是否可见
        /// </summary>
        [ObservableProperty]
        private bool _isSongDetailVisible;

        /// <summary>
        /// 歌曲详情视图模型（管理歌曲详情面板的内容）
        /// </summary>
        [ObservableProperty]
        private SongDetailViewModel _songDetailVM;

        /// <summary>
        /// 构造函数，通过依赖注入初始化服务和子视图模型
        /// 初始化导航项集合，并订阅关键事件
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        /// <param name="authService">认证服务</param>
        /// <param name="signalRService">SignalR服务</param>
        /// <param name="titleBarVM">标题栏视图模型</param>
        /// <param name="playerControlVM">播放器控制视图模型</param>
        /// <param name="authVM">认证视图模型</param>
        /// <param name="playlistService">播放列表服务</param>
        /// <param name="friendsViewModel">好友视图模型</param>
        public MainViewModel(IServiceProvider serviceProvider, AuthService authService, SignalRService signalRService, TitleBarViewModel titleBarVM, PlayerControlViewModel playerControlVM, AuthenticationViewModel authVM, PlaylistService playlistService, FriendsViewModel friendsViewModel)
        {
            _serviceProvider = serviceProvider;
            TitleBarVM = titleBarVM;
            PlayerControlVM = playerControlVM;
            AuthVM = authVM;
            _signalRService = signalRService;
            _playlistService = playlistService;
            _friendsViewModel = friendsViewModel;
            _authService = authService;

            // 订阅播放器的"显示歌曲详情"请求事件
            PlayerControlVM.ShowSongDetailRequested += OnShowSongDetailRequested;
            // 绑定标题栏的登出请求到当前视图模型的登出命令
            TitleBarVM.RequestLogoutAction = () => LogoutCommand.Execute(null);

            // 初始化主要导航项（绑定到左侧边栏的主要导航区域）
            MainNavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { DisplayName = "推荐", Icon = "\uE896", ViewModelType = typeof(LocalMusicViewModel) },
                new NavigationItem { DisplayName = "精选", Icon = "\uE8F1" , ViewModelType = typeof(FeaturedViewModel)},
                new NavigationItem { DisplayName = "播客", Icon = "\uE1D6", ViewModelType = typeof(PodcastViewModel) },
                new NavigationItem { DisplayName = "漫游", Icon = "\uE8DD", ViewModelType = typeof(PodcastViewModel) },
                new NavigationItem { DisplayName = "关注", Icon = "\uE77B", ViewModelType = typeof(FriendsViewModel) }
            };

            // 初始化"我的音乐"导航项
            MyMusicNavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { DisplayName = "我喜欢的音乐", Icon = "\uE00B", ViewModelType = typeof(MyFavoriteMusicViewModel) },
                new NavigationItem { DisplayName = "最近播放", Icon = "\uE823" , ViewModelType = typeof(LocalMusicViewModel)},
                new NavigationItem { DisplayName = "本地音乐", Icon = "\uE1D6" , ViewModelType = typeof(LocalMusicViewModel)},
            };

            // 初始化"更多"导航项
            MoreNavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { DisplayName = "我的收藏", Icon = "\uE1DE", ViewModelType = typeof(LocalMusicViewModel) },
                new NavigationItem { DisplayName = "云盘", Icon = "\uE713", ViewModelType = typeof(LocalMusicViewModel)  },
                new NavigationItem { DisplayName = "已购", Icon = "\uE779", ViewModelType = typeof(LocalMusicViewModel)  }
            };
        }

        /// <summary>
        /// 处理"显示歌曲详情"的请求事件（由播放器触发）
        /// 显示歌曲详情面板并加载对应歌曲信息
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="songToShow">需要显示详情的歌曲</param>
        private void OnShowSongDetailRequested(object sender, Song songToShow)
        {
            // 从服务容器获取歌曲详情视图模型
            var vm = _serviceProvider.GetRequiredService<SongDetailViewModel>();

            // 更新视图模型的歌曲信息
            vm.UpdateSong(songToShow);

            // 订阅关闭请求事件（防止重复订阅，先取消再订阅）
            vm.RequestClose -= OnCloseSongDetailRequested;
            vm.RequestClose += OnCloseSongDetailRequested;

            // 显示歌曲详情面板
            SongDetailVM = vm;
            IsSongDetailVisible = true;
        }

        /// <summary>
        /// 处理"关闭歌曲详情"的请求事件
        /// 隐藏歌曲详情面板
        /// </summary>
        private void OnCloseSongDetailRequested()
        {
            IsSongDetailVisible = false;
        }

        /// <summary>
        /// 应用启动时的初始化逻辑
        /// 尝试自动登录，若失败则显示登录覆盖层
        /// </summary>
        private void Startup()
        {
            // 注释：原逻辑为尝试自动登录，现简化为直接显示登录层
            IsOverlayVisible = true;
        }

        /// <summary>
        /// 登录成功后的处理逻辑
        /// 更新UI、加载用户数据、连接实时服务并导航到好友页面
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">登录成功事件参数（包含用户信息）</param>
        private async void OnLoginSuccess(object sender, LoginSuccessEventArgs e)
        {
            Debug.WriteLine($"用户 {e.UserLoginInfo.User.Name} 登录成功，加载数据并导航...");

            // 更新标题栏的用户信息（用户名和头像）
            TitleBarVM.UserName = e.UserLoginInfo.User.Name;
            TitleBarVM.Avatar = e.UserLoginInfo.User.AvatarUrl;

            // 异步加载用户特定数据（如播放列表）
            await LoadUserSpecificDataAsync();
            // 连接SignalR实时服务（使用登录后的令牌）
            await _signalRService.ConnectAsync(_authService.Token);

            // 导航到"关注"页面（好友列表）
            var friendsNavItem = MainNavigationItems.FirstOrDefault(item => item.ViewModelType == typeof(FriendsViewModel));
            if (friendsNavItem != null)
            {
                Navigate(friendsNavItem);
            }

            // 通知好友视图模型同步数据（因登录状态已就绪）
            await _friendsViewModel.SyncDataAsync();
        }

        /// <summary>
        /// 加载用户特定的数据（如收藏的播放列表）
        /// 登录成功后调用
        /// </summary>
        private async Task LoadUserSpecificDataAsync()
        {
            // 获取用户的播放列表
            var playlistDtos = await _playlistService.GetMyPlaylistsAsync();
            if (playlistDtos != null)
            {
                FavoritePlaylists.Clear();
                foreach (var dto in playlistDtos)
                {
                    FavoritePlaylists.Add(new Playlist
                    {
                        Id = dto.Id,
                        Title = dto.Name,
                        CoverImageUrl = dto.CoverImageUrl ?? GetRandomAvatarUrl() // 若无封面则使用随机头像
                    });
                }
            }
            // 可扩展：加载其他用户数据（如收藏、最近播放等）
        }

        #region 命令（由[RelayCommand]自动生成）

        /// <summary>
        /// 切换左侧边栏的展开/折叠状态
        /// </summary>
        [RelayCommand]
        private void ToggleLeftSidebar()
        {
            IsLeftSidebarExpanded = !IsLeftSidebarExpanded;
        }

        /// <summary>
        /// 显示注册界面（显示覆盖层）
        /// </summary>
        [RelayCommand]
        private void ShowSignUp()
        {
            IsOverlayVisible = true;
        }

        /// <summary>
        /// 隐藏覆盖层（如关闭登录/注册界面）
        /// </summary>
        [RelayCommand]
        private void HideOverlay()
        {
            IsOverlayVisible = false;
        }

        /// <summary>
        /// 创建新播放列表的命令
        /// 弹出输入对话框获取名称，创建成功后更新播放列表集合
        /// </summary>
        [RelayCommand]
        private async Task CreatePlaylistAsync()
        {
            // 创建输入对话框视图模型
            var dialogVM = new InputDialogViewModel
            {
                Title = "创建新歌单",
                Message = "请输入新歌单的名称："
            };

            // 创建并显示对话框
            var dialogView = new InputDialogView
            {
                DataContext = dialogVM,
                Owner = Application.Current.MainWindow
            };

            var dialogResult = dialogView.ShowDialog();

            // 若用户确认且输入有效，则创建播放列表
            if (dialogResult == true && !string.IsNullOrWhiteSpace(dialogVM.InputText))
            {
                var newPlaylistDto = await _playlistService.CreatePlaylistAsync(dialogVM.InputText);
                if (newPlaylistDto != null)
                {
                    // 添加新播放列表到集合（UI会自动更新）
                    FavoritePlaylists.Add(new Playlist
                    {
                        Id = newPlaylistDto.Id,
                        Title = newPlaylistDto.Name,
                        CoverImageUrl = newPlaylistDto.CoverImageUrl
                    });
                }
            }
        }

        /// <summary>
        /// 导航到指定导航项对应的页面
        /// 清除之前的选中状态，更新当前视图模型
        /// </summary>
        /// <param name="item">要导航到的导航项</param>
        [RelayCommand]
        private void Navigate(NavigationItem item)
        {
            if (item == null || item.ViewModelType == null) return;

            // 清除所有导航项的选中状态
            ClearAllSelections();
            // 标记当前导航项为选中
            item.IsSelected = true;
            // 从服务容器获取目标视图模型并设置为当前视图
            CurrentView = (BaseViewModel)App.ServiceProvider.GetRequiredService(item.ViewModelType);
        }

        /// <summary>
        /// 导航到指定播放列表的页面
        /// 加载该播放列表的歌曲数据并显示
        /// </summary>
        /// <param name="playlist">要导航到的播放列表</param>
        [RelayCommand]
        private async Task NavigatePlaylist(Playlist playlist)
        {
            if (playlist == null) return;

            // 清除所有选中状态
            ClearAllSelections();
            // 标记当前播放列表为选中
            playlist.IsSelected = true;
            // 获取我的收藏视图模型并加载指定播放列表数据
            var favoriteVm = App.ServiceProvider.GetRequiredService<MyFavoriteMusicViewModel>();
            await favoriteVm.LoadPlaylistAsync(playlist.Id);
            // 设置为当前视图
            CurrentView = favoriteVm;
        }

        #endregion

        /// <summary>
        /// 登出命令
        /// 断开实时连接，清除认证状态和用户数据，显示登录层
        /// </summary>
        [RelayCommand]
        private async Task Logout()
        {
            // 断开SignalR连接
            await _signalRService.DisconnectAsync();
            // 清除认证状态
            _authService.Logout();

            // 清理用户相关UI数据
            TitleBarVM.UserName = string.Empty;
            FavoritePlaylists.Clear();

            // 显示登录覆盖层
            IsOverlayVisible = true;
        }

        #region 私有方法

        /// <summary>
        /// 初始化应用数据（如加载用户播放列表）
        /// 并导航到默认页面
        /// </summary>
        private async Task InitializeAsync()
        {
            // 加载用户播放列表
            var playlistDtos = await _playlistService.GetMyPlaylistsAsync();
            if (playlistDtos != null)
            {
                FavoritePlaylists.Clear();
                foreach (var dto in playlistDtos)
                {
                    FavoritePlaylists.Add(new Playlist
                    {
                        Id = dto.Id,
                        Title = dto.Name,
                        CoverImageUrl = dto.CoverImageUrl ?? GetRandomAvatarUrl() // 无封面时使用随机图
                    });
                }
            }

            // 导航到"我喜欢的音乐"页面（默认页面）
            var favoriteNavItem = MyMusicNavigationItems.FirstOrDefault(item => item.ViewModelType == typeof(MyFavoriteMusicViewModel));
            if (favoriteNavItem != null)
            {
                Navigate(favoriteNavItem);

                // 加载"我喜欢的音乐"播放列表数据
                var favPlaylist = FavoritePlaylists.FirstOrDefault(p => p.Title == "我喜欢的音乐");
                if (CurrentView is MyFavoriteMusicViewModel vm && favPlaylist != null)
                {
                    await vm.LoadPlaylistAsync(favPlaylist.Id);
                }
            }
            else
            {
                // 若默认页面不存在，导航到第一个主要导航项
                Navigate(MainNavigationItems.FirstOrDefault());
            }
        }

        /// <summary>
        /// 清除所有导航项和播放列表的选中状态
        /// 用于导航切换时重置选中样式
        /// </summary>
        private void ClearAllSelections()
        {
            Debug.WriteLine("进入ClearAllSelections()");
            // 清除所有导航项的选中状态
            foreach (var navItem in MainNavigationItems.Concat(MyMusicNavigationItems).Concat(MoreNavigationItems))
            {
                navItem.IsSelected = false;
            }
            // 清除所有播放列表的选中状态
            foreach (var playlist in FavoritePlaylists)
            {
                playlist.IsSelected = false;
            }
        }

        #endregion
    }
}