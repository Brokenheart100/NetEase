using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAPICodePack.Dialogs;
using NetEase.Helpers;
using NetEase.Models;
using NetEase.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NetEase.Services.NavigationService;

namespace NetEase.ViewModels.PlaylistViewModels
{
    public partial class PlaylistViewModel : BaseViewModel, IRecipient<PlaylistUpdatedMessage>, INavigationAware
    {
        private readonly PlayerService _playerService;
        private readonly PlaylistService _playlistService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<PlaylistViewModel> _logger;

        private int _currentPlaylistId;

        // --- 属性 ---
        [ObservableProperty]
        private bool _isLoading = true; 

        [ObservableProperty]
        private int _songCount;

        public ObservableCollection<Song> Songs { get; } = [];

        [ObservableProperty]
        private string _coverImageUrl;

        [ObservableProperty]
        private ImageSource? _coverImageSource;

        [ObservableProperty]
        private string _playlistDescription;

        [ObservableProperty]
        private string _playlistTitle;

        [ObservableProperty]
        private string _author;

        [ObservableProperty]
        private string _createDate;

        [ObservableProperty]
        private string _authorAvatarUrl;

        public CommentViewModel CommentVM { get; }

        public PlaylistViewModel(ILogger<PlaylistViewModel> logger,ICacheService cacheService,PlayerService playerService, PlaylistService playlistService, CommentViewModel commentViewModel)
        {
            _playerService = playerService;
            _playlistService = playlistService;
            CommentVM = commentViewModel; // 赋值
            _cacheService = cacheService;
            _logger = logger;
            WeakReferenceMessenger.Default.Register<PlaylistUpdatedMessage>(this);
        }
        public async void OnNavigatedTo(object? parameter)
        {
            // a. 检查传递过来的参数是否是我们期望的 int 类型 (播放列表ID)
            if (parameter is int playlistId && playlistId > 0)
            {
                // b. 如果是，就直接调用我们已有的、功能强大的 LoadPlaylistAsync 方法
                //    这完美地复用了您现有的逻辑！
                _logger.LogInformation("通过导航参数接收到 PlaylistId: {PlaylistId}，开始加载...", playlistId);
                await LoadPlaylistAsync(playlistId);
            }
            else
            {
                _logger.LogWarning("导航到 PlaylistViewModel，但未提供有效的 PlaylistId 参数。");
                // 在这里，您可以处理当没有参数时的情况，
                // 例如清空数据或显示一个提示页面。
                IsLoading = false;
                Songs.Clear();
                PlaylistTitle = "未指定播放列表";
            }
        }
        [RelayCommand]
        private void GoToEditPage()
        {
            // 创建一个临时的Playlist对象，包含所有需要传递给编辑页面的数据
            var playlistData = new Playlist
            {
                Id = _currentPlaylistId,
                Title = PlaylistTitle,
                CoverImageUrl = CoverImageUrl,
                Description = PlaylistDescription,
            };
            // 触发事件，请求主窗口导航
            //EditPlaylistRequested?.Invoke(playlistData);
            WeakReferenceMessenger.Default.Send(new NavigateToEditPlaylistMessage(playlistData));
        }
        public void Receive(PlaylistUpdatedMessage message)
        {
            // 检查收到的更新是否是针对当前正在显示的歌单
            if (_currentPlaylistId == message.PlaylistId)
            {
                // 在UI线程上更新属性
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PlaylistTitle = message.NewName;
                    PlaylistDescription = message.NewDescription;
                    CoverImageUrl = message.NewCoverImageUrl;
                });
            }
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
                _logger.LogInformation("用户正在添加歌曲 {SongId} 到“我喜欢的音乐”...", song.Id);
                // 如果是“喜欢”操作
                success = await _playlistService.AddToFavoritesAsync(song.Id);
            }
            else
            {
                _logger.LogWarning("ToggleLike API调用失败，正在回滚UI状态，歌曲ID: {SongId}", song.Id);
                // 【核心修改】如果是“取消喜欢”操作
                success = await _playlistService.RemoveFromFavoritesAsync(song.Id);
            }

            // 2. 如果API调用失败，回滚UI状态并提示用户
            if (!success)
            {
                _logger.LogWarning("ToggleLike API调用失败，正在回滚UI状态，歌曲ID: {SongId} {}", song.Id,song.Title);
                song.IsLiked = originalLikedState; // 恢复到操作前的状态

                // 使用更友好的通知方式，而不是MessageBox
                // _notificationService.ShowError("操作失败，请稍后重试。");
                MessageBox.Show(song.IsLiked ? "添加到“我喜欢的音乐”失败。" : "取消喜欢失败。");
            }
            else
            {
                _logger.LogInformation("ToggleLike 操作成功，歌曲ID: {SongId} {}", song.Id, song.Title);
            }
        }
        async partial void OnCoverImageUrlChanged(string? value)
        {
            // 如果URL为空，则不进行任何操作，UI会使用FallbackValue
            if (string.IsNullOrWhiteSpace(value))
            {
                CoverImageSource = null;
                return;
            }

            // 1. 异步调用缓存服务获取本地文件路径
            string? localImagePath = await _cacheService.GetFileAsync(value);

            if (localImagePath != null)
            {
                // 2. 如果获取成功，创建一个 BitmapImage 对象
                // 确保这个操作在UI线程上执行
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(localImagePath);
                    // CacheOption.OnLoad 确保图片加载后立即释放对文件的锁定
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    // 3. 将创建好的 ImageSource 赋值给UI绑定的属性
                    CoverImageSource = bitmap;
                });
            }
            else
            {
                // 如果下载失败，也可以在这里设置一个“加载失败”的默认图
                CoverImageSource = null;
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
                    PlaylistDescription = playlistDetail.Description;
                    Author = playlistDetail.UserName;
                    CreateDate = playlistDetail.CreateDate.ToShortDateString();
                    CoverImageUrl = playlistDetail.CoverImageUrl;
                    AuthorAvatarUrl = playlistDetail.AuthorAvatarUrl;
                    Debug.WriteLine($"cover-{CoverImageUrl} authorUrl：{AuthorAvatarUrl} PlaylistDescription：{PlaylistDescription}");
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
                        };
                        _ = song.StartImageLoadingAsync(_cacheService);
                        Songs.Add(song);
                        Debug.WriteLine($"{song.Title} cover-{CoverImageUrl}");
                    }
                    SongCount = Songs.Count;
                }
            }
            finally
            {
                await CommentVM.LoadCommentsAsync($"playlist:{playlistId}");
                IsLoading = false;
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

        [RelayCommand]
        private static void DownloadSong(Song song)
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

    }
}
