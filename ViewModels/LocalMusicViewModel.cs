using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using NetEase.Helpers;
using NetEase.Models;
using NetEase.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace NetEase.ViewModels
{
    public partial class LocalMusicViewModel : BaseViewModel
    {
        private readonly PlayerService _playerService;
        private readonly PlaylistService _playlistService; // 假设未来会用它加载云端歌曲
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


        // 构造函数现在非常简洁，只负责依赖注入和命令初始化
        public LocalMusicViewModel(SongService songService, PlayerService playerService, PlaylistService playlistService)
        {
            _playerService = playerService;
            _playlistService = playlistService;
            _songService = songService;

            LoadDataAsync();
            //PlaySong(Songs[6]);

        }
        [RelayCommand]
        private async Task ToggleLike(Song song)
        {
            if (song == null) return;

            // 1. 先在UI上进行乐观更新
            bool originalLikedState = song.IsLiked;
            song.IsLiked = !originalLikedState;

            bool success;
            if (song.IsLiked)
            {
                // 如果是“喜欢”操作
                success = await _playlistService.AddToFavoritesAsync(song.Id);
            }
            else
            {
                // 【核心修改】如果是“取消喜欢”操作
                success = await _playlistService.RemoveFromFavoritesAsync(song.Id);
            }

            // 2. 如果API调用失败，回滚UI状态并提示用户
            if (!success)
            {
                song.IsLiked = originalLikedState; // 恢复到操作前的状态
                MessageBox.Show(song.IsLiked ? "取消喜欢失败。" : "添加到“我喜欢的音乐”失败。");
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

            // 目前，我们只加载本地歌曲
            var defaultMusicPath = @"E:\Computer\VS\NetEaseProject\NetEase\music";
            if (Directory.Exists(defaultMusicPath))
            {
                // 在后台线程扫描，避免 UI 卡顿
                await Task.Run(() => ScanAndLoadSongsFromPath(defaultMusicPath));
            }

            SongCount = Songs.Count;
            IsLoading = false;
            //PlaySong(Songs[8]);
        }

        [RelayCommand]
        private void AddLocalFolder()
        {
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Task.Run(() => ScanAndLoadSongsFromPath(dialog.FileName));
            }
        }

        [RelayCommand]
        private void PlaySong(Song? song)
        {
            Debug.WriteLine($"Enter PlaySong {song?.Title},{song?.FilePath}");
            if (song != null)
            {
                _playerService.StartPlayback(song, this.Songs);
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
                    if (exists)
                        continue;

                    var tagFile = TagLib.File.Create(file);
                    var coverImage = ImageHelper.CreateImageFromPicture(tagFile.Tag.Pictures.FirstOrDefault());

                    var song = new Song
                    {
                        Id = SongCount + 1,
                        Title = string.IsNullOrEmpty(tagFile.Tag.Title) ? Path.GetFileNameWithoutExtension(file) : tagFile.Tag.Title,
                        Artist = tagFile.Tag.FirstPerformer ?? "未知艺术家",
                        Album = tagFile.Tag.Album ?? "未知专辑",
                        Duration = tagFile.Properties.Duration.ToString(@"mm\:ss"),
                        FilePath = file,
                        CoverImage = coverImage,
                        IsDownloaded = true,

                    };

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        // 这段代码块内的所有代码，都会在 UI 线程上安全地执行
                        if (!Songs.Any(s => s.FilePath == file))
                        {
                            // 在 UI 线程上创建 SongTag 和 Brushes
                            song.Tags =
                            [
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
                            ];

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
