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

        // 这是 ListView 的唯一数据源
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

            // 异步加载所有数据
            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            Songs.Clear();

            // --- 逻辑整合 ---
            // 在这里，您可以决定加载顺序和逻辑。
            // 例如，未来可以先从 API 加载云端收藏的歌曲。
            // var cloudSongs = await _playlistService.GetMyFavoriteSongsAsync();
            // foreach(var song in cloudSongs) { Songs.Add(song); }

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
                    };

                    // 关键：修改 ObservableCollection 必须在 UI 线程上进行
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        song.Index = Songs.Count + 1;
                        Songs.Add(song);
                        SongCount = Songs.Count; // 更新歌曲数量
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