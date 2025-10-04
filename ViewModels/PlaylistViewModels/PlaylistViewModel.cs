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

namespace NetEase.ViewModels.PlaylistViewModels
{
    public partial class PlaylistViewModel : BaseViewModel
    {
        private readonly PlayerService _playerService;
        private readonly PlaylistService _playlistService; // 假设未来会用它加载云端歌曲
        private int _currentPlaylistId;
        private string _playlistDescription;
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

        public CommentViewModel CommentVM { get; }
        // 构造函数现在非常简洁，只负责依赖注入和命令初始化
        public event Action<Playlist> EditPlaylistRequested;
        public PlaylistViewModel(PlayerService playerService, PlaylistService playlistService, CommentViewModel commentViewModel)
        {
            _playerService = playerService;
            _playlistService = playlistService;
            CommentVM = commentViewModel; // 赋值
        }
        [RelayCommand]
        private void GoToEditPage()
        {
            // 创建一个临时的Playlist对象，包含所有需要传递给编辑页面的数据
            var playlistData = new Playlist
            {
                Id = _currentPlaylistId,
                Title = this.PlaylistTitle,
                CoverImageUrl = this.CoverImageUrl,
                Description = _playlistDescription,
            };
            // 触发事件，请求主窗口导航
            EditPlaylistRequested?.Invoke(playlistData);
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
            _currentPlaylistId = playlistId;
            Songs.Clear();
            try
            {
                var playlistDetail = await _playlistService.GetPlaylistDetailAsync(playlistId);

                if (playlistDetail != null)
                {
                    // 1. 更新页面头部信息
                    PlaylistTitle = playlistDetail.Name;
                    _playlistDescription = playlistDetail.Description;
                    Author = playlistDetail.UserName;
                    CreateDate = playlistDetail.CreateDate.ToShortDateString();
                    CoverImageUrl = playlistDetail.CoverImageUrl;
                    int index = 1;
                    foreach (var songDto in playlistDetail.Songs)
                    {
                        var song = new Song
                        {
                            Id = songDto.Id,
                            Index = index++,
                            Title = songDto.Title,
                            Artist = songDto.ArtistName,
                            Album = songDto.AlbumTitle,
                            Duration = songDto.Duration,
                            CoverImageUrl = songDto.CoverImageUrl,
                            FilePath = songDto.FilePath,
                            //IsLiked = songDto.
                        };
                        Songs.Add(song);
                        Debug.WriteLine($"{song.Title}");
                    }
                    SongCount = Songs.Count;
                }
            }
            finally
            {
                await CommentVM.LoadCommentsAsync($"playlist_{playlistId}");
                IsLoading = false;
            }
        }
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            Songs.Clear();

            // 目前，我们只加载本地歌曲
            var defaultMusicPath = @"E:\Computer\C#\NetEaseProject\NetEase\music\";
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
            Debug.WriteLine($"Enter PlaySong({song})");
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
        private async Task RemoveFromPlaylist(Song song)
        {
            if (song == null || _currentPlaylistId <= 0) return;

            Debug.WriteLine($"Attempting to remove '{song.Title}' from playlist ID: {_currentPlaylistId}");

            // 1. 乐观更新UI：立即从列表中移除歌曲
            //    我们需要记住它的原始位置，以便失败时可以插回去
            int originalIndex = Songs.IndexOf(song);
            if (originalIndex == -1) return; // 如果找不到，则不执行

            Songs.RemoveAt(originalIndex);
            SongCount = Songs.Count; // 更新歌曲数量

            // 2. 调用API执行后端删除操作
            var success = await _playlistService.RemoveSongFromPlaylistAsync(_currentPlaylistId, song.Id);

            // 3. 如果API调用失败，则回滚UI状态
            if (!success)
            {
                Debug.WriteLine($"Failed to remove '{song.Title}' from playlist on the server. Rolling back UI.");

                // 将歌曲插回到它原来的位置
                if (originalIndex >= 0 && originalIndex <= Songs.Count)
                {
                    Songs.Insert(originalIndex, song);
                }
                else
                {
                    Songs.Add(song); // 如果索引无效，就加到末尾
                }
                SongCount = Songs.Count; // 恢复歌曲数量

                MessageBox.Show("从歌单中删除歌曲失败，请稍后重试。", "操作失败");
            }
            else
            {
                Debug.WriteLine($"Successfully removed '{song.Title}' from playlist.");
            }
        }

        [RelayCommand]
        private async Task AddToPlaylistAsync(Song song)
        {
            if (song == null) return;
            if (_currentPlaylistId <= 0)
            {
                MessageBox.Show("无效的操作：当前没有选定一个有效的播放列表。");
                return;
            }

            var (success, errorMessage) = await _playlistService.AddSongToPlaylistAsync(_currentPlaylistId, song.Id);

            if (success)
            {
                MessageBox.Show($"已将“{song.Title}”添加到播放列表！");

            }
            else
            {
                MessageBox.Show($"添加{song.Title}失败: {errorMessage}");

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


    }
}
