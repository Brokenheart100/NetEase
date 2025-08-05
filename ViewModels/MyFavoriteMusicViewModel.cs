using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using NetEase.Helpers;
using NetEase.Models;
using NetEase.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TagLib;

namespace NetEase.ViewModels
{
    public partial class MyFavoriteMusicViewModel : BaseViewModel
    {
        // --- 页面头部信息属性 ---
        public string CoverImageUrl { get; set; }
        public string PlaylistTitle { get; set; }
        public string Author { get; set; }
        public string CreateDate { get; set; }
        //public int SongCount { get; set; }
        // 修改SongCount为可通知属性，以便在添加歌曲后UI能自动更新数量
        [ObservableProperty]
        private int _songCount;
        // --- 歌曲列表属性 ---
        public ObservableCollection<Song> Songs { get; set; }
        // --- 命令 ---
        public IRelayCommand AddLocalFolderCommand { get; }
        // 新增：播放歌曲的命令。它接收一个 Song 对象作为参数。
        public IRelayCommand<Song> PlaySongCommand { get; }
        // --- 构造函数，负责初始化所有数据 ---
        public MyFavoriteMusicViewModel()
        {
            // 初始化头部信息
            CoverImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\25.jpg"; // 使用相对路径！
            PlaylistTitle = "我喜欢的音乐";
            Author = "Brokenheart100";
            CreateDate = "2017-02-18创建";
            AddLocalFolderCommand = new RelayCommand(AddLocalFolder);
            PlaySongCommand = new RelayCommand<Song>(PlaySong);
            // 初始化歌曲列表
            Songs = new ObservableCollection<Song>
            {
                new Song
                {
                    Index = 1,
                    CoverImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\15.jpg",
                    Title = "Call of Silence",
                    Subtitle = "(沉默的呼唤)",
                    Tags = new List<SongTag>
                    {
                        new SongTag { Text = "沉浸声", Background = Brushes.Transparent, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A90E2")), BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A90E2")) }
                    },
                    Artist = "Clear Sky",
                    Album = "*希望我们都是自由的——进击的巨...",
                    IsLiked = true,
                    Duration = "05:10",
                    FilePath= "E:\\Computer\\VS\\NetEase\\music\\Clear Sky - Call of Silence.flac",
                },
                new Song
                {
                    Index = 2,
                    CoverImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\27.jpg",
                    Title = "ダイヤモンドの純度 ~ Yukino Ballade ~",
                    Tags = new List<SongTag>
                    {
                        new SongTag { Text = "超清母带", Background = Brushes.Transparent, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D0A9F5")), BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D0A9F5")) },
                        new SongTag { Text = "VIP", Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D93B3B")), Foreground = Brushes.White, BorderBrush = Brushes.Transparent }
                    },
                    Artist = "早見沙織",
                    Album = "ダイヤモンドの純度",
                    IsLiked = true,
                    Duration = "04:42"
                }
            };
            foreach (var song in Songs)
            {
                Debug.WriteLine(song.FilePath);

            }
            // 默认播放第一首歌曲
            //PlaySong(Songs.First());
            SongCount = Songs.Count;
            // 为了演示，可以预加载一些示例歌曲
            // 定义你的默认音乐文件夹路径
            string defaultMusicPath = @"E:\Computer\VS\NetEase\music";

            // 检查文件夹是否存在，如果存在就加载
            if (Directory.Exists(defaultMusicPath))
            {
                ScanAndLoadSongsFromPath(defaultMusicPath);
            }
        }
        private void PlaySong(Song song)
        {
            if (song != null)
            {
                // 调用播放服务来请求播放
                PlayerService.Instance.PlaySong(song);
                Debug.WriteLine($"正在播放歌曲: {song.Title}");
            }
        }
        private void ScanAndLoadSongsFromPath(string folderPath)
        {
            var supportedExtensions = new[] { ".mp3", ".flac", ".wav", ".wma", ".m4a" };

            try
            {
                var audioFiles = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
                    .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()));

                // 为了避免重复添加，可以先检查文件路径是否存在
                var existingPaths = new HashSet<string>(Songs.Select(s => s.FilePath));

                int currentIndex = Songs.Count + 1;
                foreach (var file in audioFiles)
                {
                    if (existingPaths.Contains(file)) continue; // 如果已存在，则跳过

                    var tagFile = TagLib.File.Create(file);

                    // 尝试从文件中提取第一张封面图
                    IPicture coverPicture = tagFile.Tag.Pictures.FirstOrDefault();
                    ImageSource coverImage = ImageHelper.CreateImageFromPicture(coverPicture) ?? ImageHelper.GetDefaultCover();
                    Songs.Add(new Song
                    {
                        Index = currentIndex++,
                        Title = string.IsNullOrEmpty(tagFile.Tag.Title) ? Path.GetFileNameWithoutExtension(file) : tagFile.Tag.Title,
                        Artist = tagFile.Tag.FirstPerformer ?? "未知艺术家",
                        Album = tagFile.Tag.Album ?? "未知专辑",
                        Duration = tagFile.Properties.Duration.ToString(@"mm\:ss"),
                        FilePath = file,
                        CoverImageUrl = "/Assets/Images/default_cover.png", // 使用默认封面
                        CoverImage = coverImage,
                        Tags = new List<SongTag>
                        {
                            new SongTag { Text = "超清母带", Background = Brushes.Transparent, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D0A9F5")), BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D0A9F5")) },
                            new SongTag { Text = "VIP", Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D93B3B")), Foreground = Brushes.White, BorderBrush = Brushes.Transparent }
                        },
                    });
                }

                // 扫描完成后，更新歌曲总数
                SongCount = Songs.Count;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"扫描文件夹 '{folderPath}' 时出错: {ex.Message}");
            }
        }
        private void LoadSampleSongs()
        {
            // (这里可以保留您之前的示例歌曲数据)
            //Songs.Add(new Song { /* ... */ });
            //SongCount = Songs.Count; // 更新数量
        }
        //E:\Computer\VS\NetEase\music
        private void AddLocalFolder()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "请选择包含音乐的文件夹"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string folderPath = dialog.FileName;
                var supportedExtensions = new[] { ".mp3", ".flac", ".wav", ".wma", ".m4a" };
                //Console.WriteLine($"用户选择的文件夹是: {folderPath}"); // <-- 这行输出
                Debug.WriteLine($"用户选择的文件夹是: {folderPath}");
                try
                {
                    var audioFiles = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
                                              .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()));
                    int currentIndex = Songs.Count + 1;
                    foreach (var file in audioFiles)
                    {
                        var tagFile = TagLib.File.Create(file);

                        Songs.Add(new Song
                        {
                            Index = currentIndex++,
                            Title = string.IsNullOrEmpty(tagFile.Tag.Title) ? Path.GetFileNameWithoutExtension(file) : tagFile.Tag.Title,
                            Artist = tagFile.Tag.FirstPerformer ?? "未知艺术家",
                            Album = tagFile.Tag.Album ?? "未知专辑",
                            Duration = tagFile.Properties.Duration.ToString(@"mm\:ss"),
                            FilePath = file,
                            // 假设新添加的本地音乐封面都一样，或者尝试从文件读取
                            //CoverImageUrl = GetAlbumArt(tagFile) ?? "/Assets/Images/default_cover.png"
                        });
                    }

                    // 扫描完成后，更新歌曲总数
                    SongCount = Songs.Count;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"扫描文件夹时出错: {ex.Message}");
                }
            }
        }

    }
}
