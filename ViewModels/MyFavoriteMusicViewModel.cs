using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using NetEase.Helpers;
using NetEase.Models;
using NetEase.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TagLib;
using System.Collections.Generic;
namespace NetEase.ViewModels
{
    public partial class MyFavoriteMusicViewModel : BaseViewModel
    {
        private readonly PlayerService _playerService;
        private readonly PlaylistService _playlistService; // 假设未来会用它加载云端歌曲

        // --- 属性 ---
        [ObservableProperty]
        private bool _isLoading = true; // 启动时默认为加载状态

        [ObservableProperty]
        private int _songCount;

        public ObservableCollection<Song> Songs { get; } = new();

        // --- 页面头部信息 (可以保持不变) ---
        public string CoverImageUrl { get; set; }
        public string PlaylistTitle { get; set; }
        public string Author { get; set; }
        public string CreateDate { get; set; }

        // --- 命令 ---
        public IRelayCommand AddLocalFolderCommand { get; }
        public IRelayCommand<Song> PlaySongCommand { get; }

        // 构造函数现在非常简洁，只负责依赖注入和命令初始化
        public MyFavoriteMusicViewModel(PlayerService playerService, PlaylistService playlistService)
        {
            _playerService = playerService;
            _playlistService = playlistService;

            // 初始化命令
            AddLocalFolderCommand = new RelayCommand(AddLocalFolder);
            PlaySongCommand = new RelayCommand<Song>(PlaySong);

            // 初始化静态头部信息
            CoverImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\25.jpg";
            PlaylistTitle = "我喜欢的音乐";
            Author = "Brokenheart100";
            CreateDate = "2017-02-18创建";
            // 构造函数现在非常干净，只调用异步加载方法
            LoadInitialPlaylistAsync();
            // 异步加载所有数据
            //LoadDataAsync();
        }
        /// <summary>
        /// 加载用户的第一个播放列表（例如 "我喜欢的音乐"）
        /// </summary>
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
                            Index = index++,
                            Title = songDto.Title,
                            Artist = songDto.Artist,
                            Album = songDto.Album,
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

        // 修改 PlaySong 方法以接受可空参数 Song?，以匹配 Action<Song?> 委托签名
        private void PlaySong(Song? song)
        {
            Debug.WriteLine("Enter PlaySong(Song? song)");
            if (song != null)
            {
                _playerService.StartPlayback(song, this.Songs);
            }
        }

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