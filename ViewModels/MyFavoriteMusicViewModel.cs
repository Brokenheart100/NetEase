using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAPICodePack.Dialogs;
using NetEase.Helpers;
using NetEase.Models;
using NetEase.Services;
using NetEase.ViewModels.MusicRowContextMenu;
using NetEase.Views.MusicRowContextMenu;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TagLib;
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
        private void ShowAddToPlaylistDialog(Song song)
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
        private void DownloadSong(Song song)
        {
            Debug.WriteLine("Enter DownloadSong(Song song)");
            if (song == null) return;
            Debug.WriteLine($"Downloading song: {song.Title}");
        }
        [RelayCommand]
        private void ShareSong(Song song)
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
        public async Task LoadInitialPlaylistAsync()
        {
            Debug.WriteLine("Enter LoadInitialPlaylistAsync() ");
            IsLoading = true;
            Songs.Clear();
            try
            {
                // 1. 获取当前用户的所有播放列表摘要
                var myPlaylists = await _playlistService.GetMyPlaylistsAsync();

                // 2. 找到第一个播放列表（或者可以根据名字查找 "我喜欢的音乐"）
                var firstPlaylist = myPlaylists?.FirstOrDefault();

                if (firstPlaylist != null)
                {
                    // 3. 如果找到了，就去加载这个播放列表的详细信息
                    await LoadPlaylistAsync(firstPlaylist.Id);
                }
                else
                {
                    // 没有找到任何播放列表
                    PlaylistTitle = "没有找到播放列表";
                    // 可以在这里显示一个“创建播放列表”的提示
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading initial playlist: {ex.Message}");
                PlaylistTitle = "加载失败";
            }
            finally
            {
                IsLoading = false;
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
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            Songs.Clear();

            // --- 逻辑整合 ---
            // 在这里，您可以决定加载顺序和逻辑。
            // 例如，未来可以先从 API 加载云端收藏的歌曲。
            // var cloudSongs = await _playlistService.GetMyFavoriteSongsAsync();
            //foreach (var song in cloudSongs) { Songs.Add(song); }

            // 目前，我们只加载本地歌曲
            string defaultMusicPath = @"E:\Computer\VS\NetEase\music";
            if (Directory.Exists(defaultMusicPath))
            {
                // 在后台线程扫描，避免 UI 卡顿
                await Task.Run(() => ScanAndLoadSongsFromPath(defaultMusicPath));
            }

            SongCount = Songs.Count;
            IsLoading = false;
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
        [RelayCommand]
        private void AddLocalFolder()
        {
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // 异步扫描新文件夹
                Task.Run(() => ScanAndLoadSongsFromPath(dialog.FileName));
            }
        }
        private void ScanAndLoadSongsFromPath(string folderPath)
        {
            var supportedExtensions = new[] { ".mp3", ".flac", ".wav", ".wma", ".m4a" };
            try
            {
                var audioFiles = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
                    .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()));

                foreach (var file in audioFiles)
                {
                    // 检查重复
                    bool exists = false;
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        exists = Songs.Any(s => s.FilePath == file);
                    });
                    if (exists) continue;

                    var tagFile = TagLib.File.Create(file);
                    var coverImage = ImageHelper.CreateImageFromPicture(tagFile.Tag.Pictures.FirstOrDefault());

                    var song = new Song
                    {
                        // Index 将在添加到集合后设置
                        Title = string.IsNullOrEmpty(tagFile.Tag.Title) ? Path.GetFileNameWithoutExtension(file) : tagFile.Tag.Title,
                        Artist = tagFile.Tag.FirstPerformer ?? "未知艺术家",
                        Album = tagFile.Tag.Album ?? "未知专辑",
                        Duration = tagFile.Properties.Duration.ToString(@"mm\:ss"),
                        FilePath = file,
                        CoverImage = coverImage,
                        IsDownloaded = true,

                    };

                    // 关键：修改 ObservableCollection 必须在 UI 线程上进行
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        // 这段代码块内的所有代码，都会在 UI 线程上安全地执行
                        if (!Songs.Any(s => s.FilePath == file))
                        {
                            // 在 UI 线程上创建 SongTag 和 Brushes
                            song.Tags = new List<SongTag>
                            {
                                new SongTag
                                {
                                    Text = "超清母带",
                                    Background = Brushes.Transparent,
                                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D0A9F5")),
                                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D0A9F5"))
                                },
                                new SongTag
                                {
                                    Text = "VIP",
                                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5C5C")),
                                    Background = Brushes.Transparent,
                                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5C5C"))
                                }
                            };

                            song.Index = Songs.Count + 1;
                            Songs.Add(song);
                            SongCount = Songs.Count;
                        }

                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"扫描文件夹 '{folderPath}' 时出错: {ex.Message}");
            }
        }
    }
}