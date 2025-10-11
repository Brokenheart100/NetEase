using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NetEase.Models;
using NetEase.Services;
using NetEase.ViewModels.ChatViewModels;
using NetEase.ViewModels.PlaylistViewModels;
using NetEase.Views.PlaylistViews;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using NetEase.Messages;
using NetEase.Services.NavigationService;

namespace NetEase.ViewModels
{
    /// <summary>
    /// 应用程序的主视图模型，负责全局状态管理、导航控制和各组件协调
    /// 是整个应用的核心协调者，连接各个子视图模型和服务
    /// </summary>
    public partial class MainViewModel : BaseViewModel,
        IRecipient<NavigateToEditPlaylistMessage>,
        IRecipient<GoBackNavigationMessage>,
        IRecipient<LoginSuccessMessage>
    {
        // 服务注入字段
        private readonly IServiceProvider _serviceProvider; // 服务提供者，用于获取其他服务/视图模型
        private readonly INavigationService _navigationService; // 1. 注入导航服务
        private readonly PlaylistService _playlistService; // 播放列表服务，用于管理播放列表数据
        private readonly AuthService _authService; // 认证服务，处理登录/登出逻辑
        private readonly FriendsViewModel _friendsViewModel; // 好友视图模型，管理好友相关功能
        private readonly SignalRService _signalRService; // SignalR服务，处理实时通信

        private readonly Stack<BaseViewModel> _navigationHistory = new();
        public INavigationService Navigation => _navigationService;
        /// <summary>
        /// 当前显示的视图模型（绑定到UI的内容区域，控制显示哪个页面）
        /// </summary>
        public BaseViewModel CurrentView => _navigationService.CurrentView;

        /// <summary>
        /// 覆盖层（如登录窗口）是否可见
        /// </summary>
        [ObservableProperty]
        private bool _isOverlayVisible;


        [ObservableProperty]
        private bool _isLeftSidebarPinned = false; // <-- 1. 命名为 Pinned 更清晰，默认值为 false

        // 这个命令将绑定到“钉住”按钮上
        [RelayCommand]
        private void ToggleLeftSidebarPin() // <-- 2. 命令名也更新一下
        {
            Debug.WriteLine($"Enter ToggleLeftSidebarPin {IsLeftSidebarPinned}");
            IsLeftSidebarPinned = !IsLeftSidebarPinned;
        }
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
        public SongDetailViewModel SongDetailVM { get; }

        /// <summary>
        /// 认证视图模型（管理登录/注册相关UI和逻辑）
        /// </summary>
        public SignUpViewModel SignUpVM { get; }

        /// <summary>
        /// 用户收藏的播放列表集合（绑定到左侧边栏的播放列表区域）
        /// </summary>
        public ObservableCollection<Playlist> FavoritePlaylists { get; } = [];

        /// <summary>
        /// 主要导航项集合（如推荐、精选等，绑定到左侧导航栏）
        /// </summary>
        public ObservableCollection<NavigationItem> MainNavigationItems { get; } =
        [
            new NavigationItem { DisplayName = "推荐", Icon = "\uE896", ViewModelType = typeof(PodcastViewModel) },
            new NavigationItem { DisplayName = "精选", Icon = "\uE8F1" , ViewModelType = typeof(PodcastViewModel)},
            new NavigationItem { DisplayName = "播客", Icon = "\uE1D6", ViewModelType = typeof(PodcastViewModel) },
            new NavigationItem { DisplayName = "漫游", Icon = "\uE8DD", ViewModelType = typeof(PodcastViewModel) },
            new NavigationItem { DisplayName = "关注", Icon = "\uE77B", ViewModelType = typeof(PodcastViewModel) }
        ];

        /// <summary>
        /// "我的音乐"分类下的导航项集合（如我喜欢的音乐、最近播放等）
        /// </summary>
        public ObservableCollection<NavigationItem> MyMusicNavigationItems { get; } =
        [
            new NavigationItem { DisplayName = "我喜欢的音乐", Icon = "\uE00B", ViewModelType = typeof(PlaylistViewModel) ,NavigationParameter = 1 },
            new NavigationItem { DisplayName = "最近播放", Icon = "\uE823" , ViewModelType = typeof(PodcastViewModel)},
            new NavigationItem { DisplayName = "本地音乐", Icon = "\uE1D6" , ViewModelType = typeof(PlaylistViewModel),NavigationParameter = -1},
        ];

        /// <summary>
        /// "更多"分类下的导航项集合（如我的收藏、云盘等）
        /// </summary>
        public ObservableCollection<NavigationItem> MoreNavigationItems { get; } = 
        [
            new NavigationItem { DisplayName = "我的收藏", Icon = "\uE1DE", ViewModelType = typeof(PodcastViewModel) },
            new NavigationItem { DisplayName = "云盘", Icon = "\uE713", ViewModelType = typeof(PodcastViewModel)  },
            new NavigationItem { DisplayName = "已购", Icon = "\uE779", ViewModelType = typeof(PodcastViewModel)  }
        ];

        /// <summary>
        /// 歌曲详情面板是否可见
        /// </summary>
        [ObservableProperty]
        private bool _isSongDetailVisible;

        public SearchResultViewModel SearchResultVM { get; }

        [ObservableProperty]
        private object _currentPage;

        [ObservableProperty]
        private string _searchText;
    

        /// <summary>
        /// 绑定到 NavigationView 的页脚菜单项
        /// </summary>
        private readonly CurrentUserStateService _currentUserState;
        [ObservableProperty]
        private object _selectedNavigationItem;
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
        public MainViewModel(CurrentUserStateService currentUserState,INavigationService navigationService, SearchResultViewModel searchViewModel, SongDetailViewModel songDetailVM, IServiceProvider serviceProvider, AuthService authService, SignalRService signalRService, TitleBarViewModel titleBarVM, PlayerControlViewModel playerControlVM, SignUpViewModel signUpVM, PlaylistService playlistService, FriendsViewModel friendsViewModel)
        {
            _currentUserState = currentUserState;
            _serviceProvider = serviceProvider;
            TitleBarVM = titleBarVM;
            PlayerControlVM = playerControlVM;
            SongDetailVM = songDetailVM;
            SignUpVM = signUpVM;
            _signalRService = signalRService;
            _playlistService = playlistService;
            _friendsViewModel = friendsViewModel;
            _authService = authService;
            SearchResultVM = searchViewModel;
            _navigationService = navigationService; // 4. 赋值

            // 订阅播放器的"显示歌曲详情"请求事件
            PlayerControlVM.ShowSongDetailRequested += OnShowSongDetailRequested;
            // 绑定标题栏的登出请求到当前视图模型的登出命令
            TitleBarVM.RequestLogoutAction = () => LogoutCommand.Execute(null);
            TitleBarVM.SearchRequested += OnSearchRequested;
            SignUpVM.LoginSuccess += OnLoginSuccess;
        
            
            if (_navigationService is ObservableObject navServiceObj)
            {
                navServiceObj.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(INavigationService.CurrentView))
                    {
                        OnPropertyChanged(nameof(CurrentView));
                    }
                };
            }

            WeakReferenceMessenger.Default.RegisterAll(this);

        }
        public async void Receive(LoginSuccessMessage message)
        {
            var loginInfo = message.Value; // 获取 LoginResponse 对象

            // 将原来 OnLoginSuccess 方法中的所有逻辑都搬到这里
            //_logger.LogInformation("接收到登录成功消息，用户: {UserName}", loginInfo.User.Name);

            TitleBarVM.UserName = loginInfo.User.Name;
            TitleBarVM.AvatarUrl = loginInfo.User.AvatarUrl;

            await LoadUserSpecificDataAsync();
            _currentUserState.SetLoggedInUser(loginInfo.User, loginInfo.Token);

            // 可以在这里默认导航到某个页面
            // _navigationService.NavigateTo<...>();
        }
        public void Receive(NavigateToEditPlaylistMessage message)
        {
            var playlistToEdit = message.PlaylistToEdit;
            var editVM = _serviceProvider.GetRequiredService<EditPlaylistViewModel>();
            editVM.LoadPlaylist(playlistToEdit);

            // 统一使用导航服务执行导航
            _navigationService.NavigateToViewModel(editVM);
        }

        // 【重构】接收返回导航的消息
        public void Receive(GoBackNavigationMessage message)
        {
            // 统一使用导航服务执行返回
            _navigationService.GoBack();
        }
        public void Receive(PlaylistUpdatedMessage message)
        {
            var playlistToUpdate = FavoritePlaylists.FirstOrDefault(p => p.Id == message.PlaylistId);
            if (playlistToUpdate != null)
            {
                playlistToUpdate.Title = message.NewName;
                playlistToUpdate.CoverImageUrl = message.NewCoverImageUrl;
            }
        }
    
        private async void OnSearchRequested(string query)
        {
            _navigationService.NavigateToViewModel(SearchResultVM); // <-- 新方式
            await SearchResultVM.PerformSearchAsync(query);
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
        private async void OnLoginSuccess(object? sender, LoginSuccessEventArgs e)
        {
            Debug.WriteLine($"用户 {e.UserLoginInfo.User.Name} 登录成功，加载数据并导航...");

            TitleBarVM.UserName = e.UserLoginInfo.User.Name;
            TitleBarVM.AvatarUrl = e.UserLoginInfo.User.AvatarUrl;
            Debug.WriteLine($"TitleBarVM.AvatarUrl:{TitleBarVM.AvatarUrl}");

            await LoadUserSpecificDataAsync();

            _currentUserState.SetLoggedInUser(e.UserLoginInfo.User, e.UserLoginInfo.Token);

        }

        /// <summary>
        /// 加载用户特定的数据,收藏的播放列表
        /// 登录成功后调用
        /// </summary>
        private async Task LoadUserSpecificDataAsync()
        {
            var playlistDtos = await _playlistService.GetMyPlaylistsAsync();
            if (playlistDtos != null)
            {
                // 从配置中获取网关地址
                var baseUrl = "http://localhost:5240";

                FavoritePlaylists.Clear();
                foreach (var dto in playlistDtos)
                {
                    var absoluteUrl = dto.CoverImageUrl;
                    // 如果收到的URL不是一个完整的绝对URL，就手动拼接
                    if (!string.IsNullOrEmpty(absoluteUrl) && !absoluteUrl.StartsWith("http"))
                    {
                        absoluteUrl = $"{baseUrl}{absoluteUrl}";
                    }

                    FavoritePlaylists.Add(new Playlist
                    {
                        Id = dto.Id,
                        Title = dto.Name,
                        CoverImageUrl = dto.CoverImageUrl,
                        TrackCount = dto.TrackCount
                    });
                }
            }
        }

        #region 命令（由[RelayCommand]自动生成）


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
            var dialogVM = new CreateLPlaylistViewModel(); // 使用新的 ViewModel

            var dialogView = new CreatePlaylistView
            {
                DataContext = dialogVM,
                Owner = Application.Current.MainWindow
            };

            if (dialogView.ShowDialog() == true)
            {

                try
                {
                    // a. 显示一个加载指示器 (如果您的BaseViewModel支持)
                    // IsBusy = true;

                    // b. 调用 PlaylistService，将对话框中获取的数据传递给它
                    var newPlaylistDto = await _playlistService.CreatePlaylistAsync(dialogVM.InputText, dialogVM.IsPrivate);

                    // c. 检查API调用是否成功
                    if (newPlaylistDto != null)
                    {
                        // d. 如果成功，将后端返回的新歌单信息添加到UI的集合中
                        FavoritePlaylists.Add(new Playlist
                        {
                            Id = newPlaylistDto.Id,
                            Title = newPlaylistDto.Name,
                            CoverImageUrl = newPlaylistDto.CoverImageUrl // 后端可能会返回一个默认封面
                                                                         // 未来可以添加一个 IsPrivate 属性来显示锁图标
                        });

                        MessageBox.Show("歌单创建成功！");
                    }
                    else
                    {
                        // API 调用失败
                        MessageBox.Show("创建歌单失败，请检查网络或稍后重试。", "错误");
                    }
                }
                catch (Exception ex)
                {
                    // 捕获意外异常
                    MessageBox.Show($"发生未知错误: {ex.Message}", "严重错误");
                }
                finally
                {
                    // IsBusy = false;
                }
            }
        }

        /// <summary>
        /// 导航到指定导航项对应的页面
        /// 清除之前的选中状态，更新当前视图模型
        /// </summary>
        /// <param name="item">要导航到的导航项</param>
        [RelayCommand]
        private async Task Navigate(NavigationItem item)
        {
            if (item == null || item.ViewModelType == null) return;

            // 清除所有导航项的选中状态
            ClearAllSelections();
            // 标记当前导航项为选中
            item.IsSelected = true;
            _navigationService.NavigateTo(item.ViewModelType, item.NavigationParameter);

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

            ClearAllSelections();
            playlist.IsSelected = true;

            // 【核心修改】同样使用带参数的导航方法，直接传递 playlist.Id
            _navigationService.NavigateTo<PlaylistViewModel>(playlist.Id);
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
            _currentUserState.ClearState();
            // 显示登录覆盖层
            IsOverlayVisible = true;
        }

        #region 私有方法

        /// <summary>
        /// 清除所有导航项和播放列表的选中状态
        /// 用于导航切换时重置选中样式
        /// </summary>
        private void ClearAllSelections()
        {
            MainNavigationItems
                .Concat(MyMusicNavigationItems)
                .Concat(MoreNavigationItems)
                .ToList()
                .ForEach(item => item.IsSelected = false);

            FavoritePlaylists
                .ToList()
                .ForEach(playlist => playlist.IsSelected = false);
        }

        #endregion
    }
}