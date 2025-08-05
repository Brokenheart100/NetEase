using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System; // 这些 using 不再是必需的，可以清理
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using NetEase.Views.Pages; // ViewModel 不应该直接引用 View
using Microsoft.WindowsAPICodePack.Dialogs; // 引入文件夹选择对话框
using NetEase.Models;
using System.Collections.ObjectModel;
using System.IO;
using NetEase.ViewModels;
using TagLib; // 引入TagLib#来读取音频元数据

namespace NetEase.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        // 使用特性来关联依赖属性。
        // 当 _currentView 字段的值改变时，工具包会自动为 CurrentView、
        // IsLocalMusicSelected、IsFeaturedSelected 和 IsPodcastSelected 触发属性变更通知。
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLocalMusicSelected))]
        [NotifyPropertyChangedFor(nameof(IsFeaturedSelected))]
        [NotifyPropertyChangedFor(nameof(IsPodcastSelected))]
        [NotifyPropertyChangedFor(nameof(IsMyFavoriteSelected))]
        private BaseViewModel _currentView;

        // 这些只读属性的逻辑保持不变，它们的更新现在由上面的特性来驱动。
        public bool IsLocalMusicSelected => CurrentView is LocalMusicViewModel;
        public bool IsFeaturedSelected => CurrentView is FeaturedViewModel;
        public bool IsPodcastSelected => CurrentView is PodcastViewModel;
        public bool IsMyFavoriteSelected => CurrentView is MyFavoriteMusicViewModel;
        // 定义导航命令 (保持不变)
        public IRelayCommand NavigateLocalMusicCommand { get; }
        public IRelayCommand NavigateFeaturedCommand { get; }
        public IRelayCommand NavigatePodcastCommand { get; }
        public IRelayCommand NavigateFavoriteCommand { get; }
        // 构造函数 (保持不变)
        // 新增：为播放栏创建一个ViewModel实例
        public PlayerControlViewModel PlayerControlVM { get; }
        public ObservableCollection<Playlist> FavoritePlaylists { get; }

        public MainViewModel()
        {
            // 新增：在构造函数中初始化播放栏的ViewModel
            PlayerControlVM = new PlayerControlViewModel();
            NavigateLocalMusicCommand = new RelayCommand(NavigateToLocalMusic);
            NavigateFeaturedCommand = new RelayCommand(NavigateToFeatured);
            NavigatePodcastCommand = new RelayCommand(NavigateToPodcast);
            NavigateFavoriteCommand = new RelayCommand(NavigateToFavorite);

        // 设置默认视图
            CurrentView = new MyFavoriteMusicViewModel();


            // 初始化歌单列表数据
            FavoritePlaylists = new ObservableCollection<Playlist>
            {
                new Playlist { CoverImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\14.jpg", Title = "【入门1】百听不厌啊十大重点知识大全", Subtitle = "的古典名曲", IsVip = true },
                new Playlist { CoverImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\15.jpg", Title = "古典清香 I 我的茶馆阿斯顿撒大大", Subtitle = "里住着巴赫与肖邦" },
                new Playlist { CoverImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\17.jpg", Title = "精选集 I 『全球十大阿斯顿萨达萨达", Subtitle = "配乐公司』" },
                new Playlist { CoverImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\18.jpg", Title = "《空之境界》精选音阿斯顿撒大大", Subtitle = "乐集" },
            };

            // 默认选中第一个歌单
            SelectedPlaylist = FavoritePlaylists.FirstOrDefault();
        }
     
    
        // 导航方法 (保持不变)
        private void NavigateToLocalMusic()
        {
            CurrentView = new LocalMusicViewModel();
        }

        private void NavigateToFeatured()
        {
            CurrentView = new FeaturedViewModel();
        }

        private void NavigateToPodcast()
        {
            CurrentView = new PodcastViewModel();
        }
        private void NavigateToFavorite()
        {
            CurrentView = new MyFavoriteMusicViewModel();
        }
        // 新增：添加本地文件夹的命令

        // 新增：当前选中的歌单
        [ObservableProperty]
        private Playlist _selectedPlaylist;

    }
}