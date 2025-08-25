using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using NetEase.Models;
using NetEase.Services;
using NetEase.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace NetEase.ViewModels
{
    // MainViewModel：应用程序的主视图模型，继承自BaseViewModel（项目自定义的基础ViewModel）
    // 负责管理全局视图切换、导航状态、子ViewModel和核心数据（如播放列表）
    public partial class MainViewModel : BaseViewModel
    {
        // 注入的播放列表服务：用于从API/本地获取用户的播放列表数据
        private readonly PlaylistService _playlistService;

        // --- 可观察属性（通过CommunityToolkit的[ObservableProperty]自动生成get/set和PropertyChanged事件）---
        /// <summary>
        /// 当前显示的页面ViewModel（用于实现页面导航，UI绑定此属性切换内容）
        /// </summary>
        [ObservableProperty]
        private BaseViewModel _currentView;

        /// <summary>
        /// 覆盖层（如登录弹窗）的可见性（true=显示，false=隐藏）
        /// </summary>
        [ObservableProperty]
        private bool _isOverlayVisible;

        /// <summary>
        /// 左侧侧边栏的展开状态（true=展开，false=折叠，默认展开）
        /// </summary>
        [ObservableProperty]
        private bool _isLeftSidebarExpanded = true;

        // --- 命令属性（ICommand类型，用于绑定UI交互事件）---
        /// <summary>
        /// 切换左侧侧边栏展开/折叠状态的命令
        /// </summary>
        public ICommand ToggleLeftSidebarCommand { get; }

        // --- 子ViewModel（通过构造函数注入，常驻内存，用于页面固定区域如标题栏、播放器）---
        /// <summary>
        /// 标题栏ViewModel（管理顶部标题栏的显示和交互）
        /// </summary>
        public TitleBarViewModel TitleBarVM { get; }

        /// <summary>
        /// 播放器控制ViewModel（管理底部音乐播放控件的逻辑）
        /// </summary>
        public PlayerControlViewModel PlayerControlVM { get; }

        /// <summary>
        /// 认证ViewModel（管理登录/注册相关逻辑）
        /// </summary>
        public AuthenticationViewModel AuthVM { get; }

        // --- 其他功能命令 ---
        /// <summary>
        /// 显示注册弹窗的命令
        /// </summary>
        public ICommand ShowSignUpCommand { get; }

        /// <summary>
        /// 隐藏覆盖层（如关闭注册弹窗）的命令
        /// </summary>
        public ICommand HideOverlayCommand { get; }

        /// <summary>
        /// 当前选中的播放列表（UI绑定此属性，高亮显示选中状态）
        /// </summary>
        [ObservableProperty]
        private Playlist _selectedPlaylist;

        // --- 可观察集合（用于绑定UI列表，数据变化时自动更新UI）---
        /// <summary>
        /// 用户收藏的播放列表集合（绑定左侧或主页的播放列表列表）
        /// </summary>
        public ObservableCollection<Playlist> FavoritePlaylists { get; }

        /// <summary>
        /// 主导航项集合（如"推荐""精选""播客"等，绑定左侧导航栏）
        /// </summary>
        public ObservableCollection<NavigationItem> MainNavigationItems { get; }

        /// <summary>
        /// "我的音乐"分类下的导航项集合（如"我喜欢的音乐""最近播放"，绑定左侧导航栏）
        /// </summary>
        public ObservableCollection<NavigationItem> MyMusicNavigationItems { get; }

        /// <summary>
        /// "更多"分类下的导航项集合（如"我的收藏""云盘"，绑定左侧导航栏）
        /// </summary>
        public ObservableCollection<NavigationItem> MoreNavigationItems { get; }

        // --- 导航相关命令 ---
        /// <summary>
        /// 导航到指定导航项对应的页面（如点击"推荐"跳转到推荐页面）
        /// </summary>
        public ICommand NavigateCommand { get; }

        /// <summary>
        /// 导航到指定播放列表的详情页面（如点击某个歌单跳转到歌单详情）
        /// </summary>
        public ICommand NavigatePlaylistCommand { get; }

        public IAsyncRelayCommand CreatePlaylistCommand { get; }

        // --- 构造函数（通过依赖注入初始化子ViewModel和服务，初始化数据和命令）---
        /// <summary>
        /// MainViewModel构造函数（依赖注入关键参数）
        /// </summary>
        /// <param name="titleBarVM">标题栏ViewModel（注入）</param>
        /// <param name="playerControlVM">播放器控制ViewModel（注入）</param>
        /// <param name="authVM">认证ViewModel（注入）</param>
        /// <param name="playlistService">播放列表服务（注入，用于获取播放列表数据）</param>
        public MainViewModel(TitleBarViewModel titleBarVM, PlayerControlViewModel playerControlVM, AuthenticationViewModel authVM, PlaylistService playlistService)
        {
            // 1. 保存注入的常驻依赖（子ViewModel和服务）
            TitleBarVM = titleBarVM;
            PlayerControlVM = playerControlVM;
            AuthVM = authVM;
            _playlistService = playlistService;

            CreatePlaylistCommand = new AsyncRelayCommand(CreatePlaylistAsync);
            // 2. 初始化功能命令（使用RelayCommand绑定无参/有参方法）
            // 显示注册弹窗：执行时设置IsOverlayVisible为true
            ShowSignUpCommand = new RelayCommand(() => IsOverlayVisible = true);
            // 隐藏覆盖层：执行时设置IsOverlayVisible为false
            HideOverlayCommand = new RelayCommand(() => IsOverlayVisible = false);
            // 导航到指定导航项：执行时调用NavigateTo方法（参数为NavigationItem）
            NavigateCommand = new RelayCommand<NavigationItem>(NavigateTo);
            // 导航到指定播放列表：执行时调用NavigateToPlaylist方法（参数为Playlist）
            NavigatePlaylistCommand = new RelayCommand<Playlist>(NavigateToPlaylist);
            // 切换左侧侧边栏状态：执行时反转IsLeftSidebarExpanded的值（展开↔折叠）
            ToggleLeftSidebarCommand = new RelayCommand(() => IsLeftSidebarExpanded = !IsLeftSidebarExpanded);

            // 3. 初始化收藏播放列表集合（先添加示例数据，后续可被API数据覆盖）
            FavoritePlaylists = new ObservableCollection<Playlist> { };
         

            // 4. 初始化导航项集合（绑定左侧导航栏，每个项包含显示名称、图标、目标ViewModel类型）
            // 主导航项（"推荐""精选"等）
            MainNavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { DisplayName = "推荐", Icon = "\uE896", ViewModelType = typeof(LocalMusicViewModel) },
                new NavigationItem { DisplayName = "精选", Icon = "\uE8F1" , ViewModelType = typeof(FeaturedViewModel)},
                new NavigationItem { DisplayName = "播客", Icon = "\uE1D6", ViewModelType = typeof(PodcastViewModel) },
                new NavigationItem { DisplayName = "漫游", Icon = "\uE8DD", ViewModelType = typeof(PodcastViewModel) },
                new NavigationItem { DisplayName = "关注", Icon = "\uE77B", ViewModelType = typeof(FriendsViewModel) }
            };

            // "我的音乐"分类导航项
            MyMusicNavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { DisplayName = "我喜欢的音乐", Icon = "\uE00B", ViewModelType = typeof(MyFavoriteMusicViewModel) },
                new NavigationItem { DisplayName = "最近播放", Icon = "\uE823" , ViewModelType = typeof(LocalMusicViewModel)},
                new NavigationItem { DisplayName = "本地音乐", Icon = "\uE1D6" , ViewModelType = typeof(LocalMusicViewModel)},
            };

            // "更多"分类导航项
            MoreNavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { DisplayName = "我的收藏", Icon = "\uE1DE", ViewModelType = typeof(LocalMusicViewModel) },
                new NavigationItem { DisplayName = "云盘", Icon = "\uE713", ViewModelType = typeof(LocalMusicViewModel)  },
                new NavigationItem { DisplayName = "已购", Icon = "\uE779", ViewModelType = typeof(LocalMusicViewModel)  }
            };
            InitializeAsync();
        }
        // --- 新增：命令的执行逻辑 ---
        private async Task CreatePlaylistAsync()
        {
            // 1. 创建对话框的 ViewModel
            var dialogVM = new InputDialogViewModel
            {
                Title = "创建新歌单",
                Message = "请输入新歌单的名称："
            };

            // 2. 创建对话框的 View
            var dialogView = new InputDialogView
            {
                DataContext = dialogVM,
                // 设置 Owner，让对话框显示在主窗口的正中间
                Owner = Application.Current.MainWindow
            };

            // 3. 以“模态”方式显示对话框，并等待它关闭
            //    ShowDialog() 会阻塞代码执行，直到对话框关闭
            var dialogResult = dialogView.ShowDialog();

            if (dialogResult == true && !string.IsNullOrWhiteSpace(dialogVM.InputText))
            {
                // 用户点击了确定并且输入了内容
                // 4. 如果用户点击了“确定”，则获取输入内容并调用服务
                var newPlaylistDto = await _playlistService.CreatePlaylistAsync(dialogVM.InputText);

                if (newPlaylistDto != null)
                {
                    var newPlaylist = new Playlist
                    {
                        Id = newPlaylistDto.Id,
                        Title = newPlaylistDto.Name,
                        CoverImageUrl = newPlaylistDto.CoverImageUrl
                    };
                    FavoritePlaylists.Add(newPlaylist);
                    // 可以在这里显示一个成功的通知
                }
            }
        }

        // --- 初始化相关方法（用于应用启动时加载数据和默认视图）---
        /// <summary>
        /// 应用程序启动时的异步初始化方法（加载用户数据+设置默认视图）
        /// </summary>
        private async void InitializeAsync()
        {
            // 注：实际应用中需先判断用户是否登录（如检查AuthService的Token），未登录则不加载数据并显示登录弹窗

            // 1. 从API加载用户收藏的播放列表（通过PlaylistService）
            var playlistDtos = await _playlistService.GetMyPlaylistsAsync();
            if (playlistDtos != null) // 若获取到数据
            {
                FavoritePlaylists.Clear(); // 清空示例数据
                foreach (var dto in playlistDtos) // 遍历DTO（数据传输对象），转换为本地Playlist模型
                {
                    FavoritePlaylists.Add(new Playlist
                    {
                        Id = dto.Id, // 歌单ID（用于后续加载歌单详情）
                        Title = dto.Name, // 歌单名称（映射DTO的Name属性）
                        CoverImageUrl = dto.CoverImageUrl // 歌单封面（映射DTO的封面地址）
                        // 若DTO有IsVip属性，可在此处添加映射：IsVip = dto.IsVip
                    });
                }
            }

            // 2. 设置默认视图（优先导航到"我喜欢的音乐"页面）
            // 查找"MyMusicNavigationItems"中目标类型为MyFavoriteMusicViewModel的导航项
            var favoriteNavItem = MyMusicNavigationItems.FirstOrDefault(item => item.ViewModelType == typeof(MyFavoriteMusicViewModel));
            if (favoriteNavItem != null)
            {
                NavigateTo(favoriteNavItem); // 导航到"我喜欢的音乐"页面

                // 若"我喜欢的音乐"对应一个播放列表，加载该歌单详情
                var favPlaylist = FavoritePlaylists.FirstOrDefault(p => p.Title == "我喜欢的音乐");
                if (CurrentView is MyFavoriteMusicViewModel vm && favPlaylist != null)
                {
                    await vm.LoadPlaylistAsync(favPlaylist.Id); // 异步加载歌单数据
                }
            }
            else
            {
                // 若未找到"我喜欢的音乐"，导航到主导航项的第一个页面
                NavigateTo(MainNavigationItems.FirstOrDefault());
            }
        }

        /// <summary>
        /// 加载初始数据的异步方法（侧重播放列表数据加载）
        /// </summary>
        private async void LoadInitialDataAsync()
        {
            // 注：实际应用中需先验证用户登录状态（如检查Token有效性）

            // 1. 从API加载用户收藏的播放列表
            var playlistDtos = await _playlistService.GetMyPlaylistsAsync();

            if (playlistDtos != null)
            {
                FavoritePlaylists.Clear(); // 清空现有数据
                foreach (var dto in playlistDtos)
                {
                    Debug.WriteLine($"{dto.Name}"); // 调试输出歌单名称（用于开发时验证数据）
                    // DTO转换为本地Playlist模型
                    FavoritePlaylists.Add(new Playlist
                    {
                        Id = dto.Id,
                        Title = dto.Name,
                        CoverImageUrl = dto.CoverImageUrl
                        // 可添加其他属性映射：如IsVip、SongCount等
                    });
                }
            }

            // 2. 加载完播放列表后，设置默认选中的歌单并导航
            var firstPlaylist = FavoritePlaylists.FirstOrDefault();
            if (firstPlaylist != null)
            {
                await NavigateToPlaylistAsync(firstPlaylist); // 异步导航到第一个歌单的详情页
            }
            else
            {
                // 若用户无任何歌单，导航到主导航的第一个页面
                NavigateTo(MainNavigationItems.First());
            }
        }

        /// <summary>
        /// 初始化默认视图的方法（侧重页面导航逻辑）
        /// </summary>
        private void InitializeView()
        {
            // 注：此方法假设FavoritePlaylists已加载完成（实际应用中需确保异步加载完成）

            // 应用启动时自动加载第一个收藏歌单
            var firstPlaylist = FavoritePlaylists.FirstOrDefault();
            if (firstPlaylist != null)
            {
                _ = NavigateToPlaylistAsync(firstPlaylist); // 异步导航到第一个歌单（_ = 忽略返回的Task，避免警告）
            }
            else
            {
                // 若无歌单，导航到主导航的第一个页面（如"推荐"）
                NavigateTo(MainNavigationItems.First());
            }
        }


        // --- 导航辅助方法 ---
        /// <summary>
        /// 清除所有选中状态（导航项和播放列表），避免多个项同时高亮
        /// </summary>
        private void ClearAllSelections()
        {
            Debug.WriteLine("enter ClearAllSelections()"); // 调试输出，标记进入该方法

            // 清除所有导航项的选中状态（合并三个导航项集合，遍历设置IsSelected为false）
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

        /// <summary>
        /// 导航到指定导航项对应的页面
        /// </summary>
        /// <param name="item">目标导航项（包含ViewModelType）</param>
        private void NavigateTo(NavigationItem item)
        {
            // 若导航项为空或无目标ViewModel类型，直接返回（避免空引用）
            if (item == null || item.ViewModelType == null) return;

            ClearAllSelections(); // 清除之前的选中状态
            item.IsSelected = true; // 设置当前导航项为选中状态

            // 从依赖注入容器中获取目标ViewModel实例，并赋值给CurrentView（触发UI切换）
            CurrentView = (BaseViewModel)App.ServiceProvider.GetRequiredService(item.ViewModelType);
        }

        /// <summary>
        /// 异步导航到指定播放列表的详情页面（先加载数据，再切换视图）
        /// </summary>
        /// <param name="playlist">目标播放列表（包含Id用于加载详情）</param>
        private async Task NavigateToPlaylistAsync(Playlist playlist)
        {
            // 若播放列表为空，直接返回
            if (playlist == null) return;

            ClearAllSelections(); // 清除之前的选中状态
            playlist.IsSelected = true; // 设置当前播放列表为选中状态

            // 从依赖注入容器中获取"MyFavoriteMusicViewModel"实例（假设歌单详情用此ViewModel）
            var favoriteVm = App.ServiceProvider.GetRequiredService<MyFavoriteMusicViewModel>();

            // 异步加载该播放列表的详情数据（等待加载完成，避免UI空白）
            await favoriteVm.LoadPlaylistAsync(playlist.Id);

            // 数据加载完成后，切换到歌单详情视图
            CurrentView = favoriteVm;
        }

        /// <summary>
        /// 同步版本的播放列表导航方法（适配RelayCommand的同步委托，内部调用异步方法）
        /// </summary>
        /// <param name="playlist">目标播放列表</param>
        private async void NavigateToPlaylist(Playlist playlist)
        {
            await NavigateToPlaylistAsync(playlist); // 调用异步方法，确保数据加载完成
        }

        /// <summary>
        /// 导航到"我喜欢的音乐"页面（并加载默认数据）
        /// </summary>
        private void NavigateToFavorite()
        {
            // 从依赖注入容器中获取"MyFavoriteMusicViewModel"实例
            var favoriteVm = App.ServiceProvider.GetRequiredService<MyFavoriteMusicViewModel>();

            // 调用ViewModel的初始化方法，加载默认播放列表数据
            favoriteVm.LoadInitialPlaylistAsync();

            // 切换到"我喜欢的音乐"视图
            CurrentView = favoriteVm;
        }
    }
}