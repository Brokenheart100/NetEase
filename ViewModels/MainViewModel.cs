using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NetEase.Models;
using NetEase.Services;
using NetEase.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NetEase.ViewModels.ChatViewModels;
using static NetEase.Converters.RandomNumber;

namespace NetEase.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly PlaylistService _playlistService;
        private readonly AuthService _authService;
        private readonly FriendsViewModel _friendsViewModel;
        private readonly SignalRService _signalRService;

        [ObservableProperty]
        private BaseViewModel _currentView;

        [ObservableProperty]
        private bool _isOverlayVisible;

        [ObservableProperty]
        private bool _isLeftSidebarExpanded = true;

        [ObservableProperty]
        private Playlist _selectedPlaylist;

        public TitleBarViewModel TitleBarVM { get; }
        public PlayerControlViewModel PlayerControlVM { get; }
        public AuthenticationViewModel AuthVM { get; }

        public ObservableCollection<Playlist> FavoritePlaylists { get; } = new();
        public ObservableCollection<NavigationItem> MainNavigationItems { get; }
        public ObservableCollection<NavigationItem> MyMusicNavigationItems { get; }
        public ObservableCollection<NavigationItem> MoreNavigationItems { get; }

        public MainViewModel(AuthService authService,SignalRService signalRService, TitleBarViewModel titleBarVM, PlayerControlViewModel playerControlVM, AuthenticationViewModel authVM, PlaylistService playlistService, FriendsViewModel friendsViewModel)
        {
            TitleBarVM = titleBarVM;
            PlayerControlVM = playerControlVM;
            AuthVM = authVM;
            _signalRService = signalRService;
            _playlistService = playlistService;
            AuthVM.LoginSuccess += OnLoginSuccess;
            _friendsViewModel = friendsViewModel; // <-- 2. 保存 FriendsViewModel 实
            _authService = authService;

            TitleBarVM.RequestLogoutAction = () => LogoutCommand.Execute(null);
            MainNavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { DisplayName = "推荐", Icon = "\uE896", ViewModelType = typeof(LocalMusicViewModel) },
                new NavigationItem { DisplayName = "精选", Icon = "\uE8F1" , ViewModelType = typeof(FeaturedViewModel)},
                new NavigationItem { DisplayName = "播客", Icon = "\uE1D6", ViewModelType = typeof(PodcastViewModel) },
                new NavigationItem { DisplayName = "漫游", Icon = "\uE8DD", ViewModelType = typeof(PodcastViewModel) },
                new NavigationItem { DisplayName = "关注", Icon = "\uE77B", ViewModelType = typeof(FriendsViewModel) }
            };

            MyMusicNavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { DisplayName = "我喜欢的音乐", Icon = "\uE00B", ViewModelType = typeof(MyFavoriteMusicViewModel) },
                new NavigationItem { DisplayName = "最近播放", Icon = "\uE823" , ViewModelType = typeof(LocalMusicViewModel)},
                new NavigationItem { DisplayName = "本地音乐", Icon = "\uE1D6" , ViewModelType = typeof(LocalMusicViewModel)},
            };

            MoreNavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { DisplayName = "我的收藏", Icon = "\uE1DE", ViewModelType = typeof(LocalMusicViewModel) },
                new NavigationItem { DisplayName = "云盘", Icon = "\uE713", ViewModelType = typeof(LocalMusicViewModel)  },
                new NavigationItem { DisplayName = "已购", Icon = "\uE779", ViewModelType = typeof(LocalMusicViewModel)  }
            };
            //InitializeAsync();
            //Thread.Sleep(1500);
            //Startup();
        }
        private async void Startup()
        {
            // 尝试自动登录或检查已保存的Token
            //var (isLoggedIn, loginResponse) = await _authService.TryAutoLoginAsync();

            //if (isLoggedIn)
            //{
            //    // 如果自动登录成功，直接触发登录成功事件
            //    OnLoginSuccess(this, new LoginSuccessEventArgs(loginResponse));
            //    Navigate(MainNavigationItems[4]);

            //}
            //else
            //{
            //    // 如果没有有效的Token或自动登录失败，则显示登录遮罩层
            //    IsOverlayVisible = true;
            //}
            IsOverlayVisible = true;
        }
        private async void OnLoginSuccess(object sender, LoginSuccessEventArgs e)
        {
            Debug.WriteLine($"Enter OnLoginSuccess User {e.UserLoginInfo.User.Name} logged in. Loading data and navigating...");

            // a. 更新全局UI元素
            TitleBarVM.UserName = e.UserLoginInfo.User.Name;
            TitleBarVM.Avatar = e.UserLoginInfo.User.AvatarUrl;

            // b. 异步加载需要用户身份的全局数据（例如播放列表）
            await LoadUserSpecificDataAsync();
            await _signalRService.ConnectAsync(_authService.Token);
            // c. 【核心】导航到 FriendsView
            //    我们通过查找导航项来找到 FriendsViewModel 的类型
            var friendsNavItem = MainNavigationItems.FirstOrDefault(item => item.ViewModelType == typeof(FriendsViewModel));
            if (friendsNavItem != null)
            {
                Navigate(friendsNavItem); // 调用已有的导航命令
            }

            // d. 【核心】通知 FriendsViewModel 同步其内部数据
            //    因为 FriendsViewModel 可能已经创建，但没有登录信息时无法加载数据
            //    所以我们在这里手动触发它的数据加载
            await _friendsViewModel.SyncDataAsync();
        }
        private async Task LoadUserSpecificDataAsync()
        {
            // 这个方法取代了旧的 InitializeAsync 的部分功能
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
                        CoverImageUrl = dto.CoverImageUrl ?? GetRandomAvatarUrl() // 默认图标
                    });
                }
            }
            // 可以在这里加载其他全局数据...
        }
        #region Commands (Generated by [RelayCommand])

        [RelayCommand]
        private void ToggleLeftSidebar()
        {
            IsLeftSidebarExpanded = !IsLeftSidebarExpanded;
        }

        [RelayCommand]
        private void ShowSignUp()
        {
            IsOverlayVisible = true;
        }

        [RelayCommand]
        private void HideOverlay()
        {
            IsOverlayVisible = false;
        }

        [RelayCommand]
        private async Task CreatePlaylistAsync()
        {
            var dialogVM = new InputDialogViewModel
            {
                Title = "创建新歌单",
                Message = "请输入新歌单的名称："
            };

            var dialogView = new InputDialogView
            {
                DataContext = dialogVM,
                Owner = Application.Current.MainWindow
            };

            var dialogResult = dialogView.ShowDialog();

            if (dialogResult == true && !string.IsNullOrWhiteSpace(dialogVM.InputText))
            {
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
                }
            }
        }

        // Renamed from NavigateTo so the generator creates "NavigateCommand"
        [RelayCommand]
        private void Navigate(NavigationItem item)
        {
            if (item == null || item.ViewModelType == null) return;
            ClearAllSelections();
            item.IsSelected = true;
            CurrentView = (BaseViewModel)App.ServiceProvider.GetRequiredService(item.ViewModelType);
        }

        // Renamed from NavigateToPlaylist and logic merged from NavigateToPlaylistAsync
        [RelayCommand]
        private async Task NavigatePlaylist(Playlist playlist)
        {
            if (playlist == null) return;
            ClearAllSelections();
            playlist.IsSelected = true;
            var favoriteVm = App.ServiceProvider.GetRequiredService<MyFavoriteMusicViewModel>();
            await favoriteVm.LoadPlaylistAsync(playlist.Id);
            CurrentView = favoriteVm;
        }

        #endregion


        [RelayCommand]
        private async Task Logout()
        {
            await _signalRService.DisconnectAsync();
            // 1. 调用 AuthService 清除认证状态
            _authService.Logout();

            // 2. 清理 MainViewModel 中的用户相关数据
            TitleBarVM.UserName = string.Empty; // 清空标题栏的用户名
            FavoritePlaylists.Clear(); // 清空播放列表

            // 也可以考虑通知其他子ViewModel进行清理
            // _friendsViewModel.ClearData(); 

            // 3. 【核心】显示登录遮罩层
            //IsOverlayVisible = true;
        }
        #region Private Methods

        private async Task InitializeAsync()
        {
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
                        CoverImageUrl = dto.CoverImageUrl??GetRandomAvatarUrl() // 默认图标
                    });
                }
            }

            var favoriteNavItem = MyMusicNavigationItems.FirstOrDefault(item => item.ViewModelType == typeof(MyFavoriteMusicViewModel));
            if (favoriteNavItem != null)
            {
                Navigate(favoriteNavItem); // Now calls the new command method

                var favPlaylist = FavoritePlaylists.FirstOrDefault(p => p.Title == "我喜欢的音乐");
                if (CurrentView is MyFavoriteMusicViewModel vm && favPlaylist != null)
                {
                    await vm.LoadPlaylistAsync(favPlaylist.Id);
                }
            }
            else
            {
                Navigate(MainNavigationItems.FirstOrDefault());
            }
        }

        private void ClearAllSelections()
        {
            Debug.WriteLine("enter ClearAllSelections()");
            foreach (var navItem in MainNavigationItems.Concat(MyMusicNavigationItems).Concat(MoreNavigationItems))
            {
                navItem.IsSelected = false;
            }
            foreach (var playlist in FavoritePlaylists)
            {
                playlist.IsSelected = false;
            }
        }


        #endregion
    }
}