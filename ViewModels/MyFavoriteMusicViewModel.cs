using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAPICodePack.Dialogs;
using NetEase.Helpers;
using NetEase.Models;
using NetEase.Services;
using NetEase.Views.MusicRowContextMenu;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using NetEase.ViewModels.PlaylistViewModels;
using static NetEase.Converters.RandomNumber;

namespace NetEase.ViewModels
{
    public partial class MyFavoriteMusicViewModel : BaseViewModel
    {
        private readonly PlayerService _playerService;
        private readonly PlaylistService _playlistService; // 假设未来会用它加载云端歌曲
        private readonly AuthService _authService;
        private readonly SongService _songService;
        // --- 属性 ---
        [ObservableProperty]
        private bool _isLoading = true; // 启动时默认为加载状态

        [ObservableProperty]
        private int _songCount;

        public ObservableCollection<Song> Songs { get; } = [];

        // --- 页面头部信息 (可以保持不变) ---
        public string CoverImageUrl { get; set; }
        public string PlaylistTitle { get; set; }
        public string Author { get; set; }
        public string CreateDate { get; set; }

        public MyFavoriteMusicViewModel(SongService songService, PlayerService playerService, PlaylistService playlistService, AuthService authService)
        {
            _playerService = playerService;
            _playlistService = playlistService;
            _authService = authService;
            _songService = songService;

            CoverImageUrl = GetRandomAvatarUrl();
            PlaylistTitle = "我喜欢的音乐";
            Author = "Brokenheart100";
            CreateDate = "2017-02-18创建";
            LoadAllSongsFromDatabaseAsync();
        }
        private async Task LoadAllSongsFromDatabaseAsync()
        {
            IsLoading = true;
            Songs.Clear();

            // 4. 调用新服务获取所有歌曲的DTO列表
            var songDtos = await _songService.GetAllSongsAsync();

            if (songDtos != null)
            {
                int index = 1;
                foreach (var dto in songDtos)
                {
                    // 5. 将DTO转换为前端的Song模型
                    var song = new Song
                    {
                        Id = dto.Id,
                        Index = index++,
                        Title = dto.Title,
                        Artist = dto.ArtistName,
                        Album = dto.AlbumTitle,
                        Duration = dto.Duration,
                        CoverImageUrl = dto.CoverImageUrl,
                        FilePath = dto.FilePath
                    };
                    Debug.WriteLine($"Loaded song from DB: {song.CoverImageUrl} by {song.FilePath}");
                    Songs.Add(song);
                }
            }

            SongCount = Songs.Count;
            IsLoading = false;
        }
        // 【新增】一个公共的初始化方法，由 MainViewModel 在登录成功后调用
        public async Task InitializeAsync()
        {
            await LoadMyFavoriteMusicPlaylistAsync();
        }

        // 【核心修改】加载 "我喜欢的音乐" 歌单
        private async Task LoadMyFavoriteMusicPlaylistAsync()
        {
            IsLoading = true;
            Songs.Clear();
            try
            {
                // 1. 获取当前用户的所有歌单摘要
                var myPlaylists = await _playlistService.GetMyPlaylistsAsync();
                if (myPlaylists == null)
                {
                    PlaylistTitle = "加载歌单列表失败";
                    return;
                }

                // 2. 找到名为 "我喜欢的音乐" 的歌单 (或者通常是第一个)
                var favoritePlaylist = myPlaylists.FirstOrDefault(p => p.Name == "我喜欢的音乐") ?? myPlaylists.FirstOrDefault();

                if (favoritePlaylist == null)
                {
                    PlaylistTitle = "未找到“我喜欢的音乐”歌单";
                    return;
                }

                // 3. 根据找到的歌单ID，加载详细信息
                var playlistDetail = await _playlistService.GetPlaylistDetailAsync(favoritePlaylist.Id);

                if (playlistDetail != null)
                {
                    // 4. 更新UI绑定的属性
                    PlaylistTitle = playlistDetail.Name;
                    CoverImageUrl = playlistDetail.CoverImageUrl;
                    // Author = playlistDetail.UserName; // DTO需要包含这些信息

                    int index = 1;
                    foreach (var songDto in playlistDetail.Songs)
                    {
                        // 将从 Library.API (聚合了 Catalog.API 数据) 返回的 SongDataDto 转换为前端的 Song 模型
                        var song = new Song
                        {
                            Id = songDto.Id,
                            Index = index++,
                            Title = songDto.Title,
                            //Artist = songDto.ArtistName,
                            //Album = songDto.AlbumTitle,
                            Duration = songDto.Duration,
                            // FilePath 暂时为空，因为我们还没有实现播放流
                            CoverImageUrl = songDto.CoverImageUrl // 确保DTO有这个字段
                        };
                        Songs.Add(song);
                    }
                    SongCount = Songs.Count;
                }
                else
                {
                    PlaylistTitle = "加载歌单详情失败";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading favorite playlist: {ex.Message}");
                PlaylistTitle = "加载时发生错误";
            }
            finally
            {
                IsLoading = false;
            }
        }
        [RelayCommand]
        private static void ShowAddToPlaylistDialog(Song song)
        {
            Debug.WriteLine("Enter ShowAddToPlaylistDialog(Song song)");
            if (song == null) return;

            // 1. 从 DI 容器中获取需要的服务
            var playlistService = App.ServiceProvider.GetRequiredService<PlaylistService>();

            // 2. 创建弹出窗口的 ViewModel，并将【服务】和【要添加的歌曲】传递给它
            var dialogVM = new AddToPlaylistViewModel(playlistService, song);

            // 3. 创建并显示窗口
            var dialogView = new AddToPlaylistView
            {
                DataContext = dialogVM,
                Owner = Application.Current.MainWindow
            };

            dialogView.ShowDialog();
        }
        [RelayCommand]
        private static void DownloadSong(Song song)
        {
            Debug.WriteLine("Enter DownloadSong(Song song)");
            if (song == null) return;
            Debug.WriteLine($"Downloading song: {song.Title}");
        }
        [RelayCommand]
        private static void ShareSong(Song song)
        {
            if (song == null) return;
            Debug.WriteLine($"Sharing song: {song.Title}");
        }
        [RelayCommand]
        private void RemoveFromPlaylist(Song song)
        {
            if (song == null) return;
            Debug.WriteLine($"Removing song: {song.Title} from playlist.");
            // 实际逻辑：
            // 1. 调用后端 API 从数据库中删除关系
            // 2. 如果成功，从 Songs 这个 ObservableCollection 中移除 song
            Songs.Remove(song);
        }
        [RelayCommand]
        private async Task AddToPlaylistAsync(Song song)
        {
            if (song == null) return;

            // 为了简化，我们先硬编码添加到 ID 为 1 的播放列表 ("我喜欢的音乐")
            int targetPlaylistId = 1;

            var (success, errorMessage) = await _playlistService.AddSongToPlaylistAsync(targetPlaylistId, song.Index);

            if (success)
            {
                MessageBox.Show($"已将“{song.Title}”添加到播放列表！");

            }
            else
            {
                MessageBox.Show($"添加失败: {errorMessage}");

            }
        }
      
        public async Task LoadPlaylistAsync(int playlistId)
        {
            Debug.WriteLine($"Enter LoadPlaylistAsync playlistId:{playlistId} ");
            IsLoading = true;
            Songs.Clear();
            try
            {
                var playlistDetail = await _playlistService.GetPlaylistDetailAsync(playlistId);

                if (playlistDetail != null)
                {
                    // 1. 更新页面头部信息
                    PlaylistTitle = playlistDetail.Name;
                    Author = playlistDetail.UserName;
                    CreateDate = playlistDetail.CreateDate.ToShortDateString();
                    CoverImageUrl = playlistDetail.CoverImageUrl;
                    int index = 1;
                    foreach (var songDto in playlistDetail.Songs)
                    {
                        // 2. 将 SongDto 转换为 Song (WPF Model)
                        var song = new Song
                        {
                            Id = songDto.Id, // <-- 确保赋值
                            Index = index++,
                            Title = songDto.Title,
                            Artist = songDto.ArtistName,
                            Album = songDto.AlbumTitle,
                            Duration = songDto.Duration,
                            FilePath = songDto.FilePath,

                        };
                        Songs.Add(song);
                        Debug.WriteLine($"{song.Title}");
                    }
                    SongCount = Songs.Count;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
      
        [RelayCommand]
        private void PlaySong(Song? song)
        {
            Debug.WriteLine($"Enter PlaySong {song?.Title},path {song?.FilePath}");
            if (song != null)
            {
                _playerService.StartPlayback(song, this.Songs);
            }
        }
    }
}