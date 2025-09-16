using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NetEase.Models;
using NetEase.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetEase.ViewModels
{
    // 定义一个歌词行模型
    public partial class LyricLine : ObservableObject
    {
        public TimeSpan Time { get; set; }
        public string OriginalText { get; set; }
        public string TranslatedText { get; set; }

        [ObservableProperty]
        private bool _isCurrentLine; // 标记是否是当前播放的行
    }
    public enum SongDetailTab
    {
        Lyrics,
        Wiki,
        Similar
    }
    public partial class SongDetailViewModel : BaseViewModel
    {
        [ObservableProperty]
        private SongDetailTab _selectedTab = SongDetailTab.Lyrics; // 默认选中歌词
        public ObservableCollection<Comment> Comments { get; } = new();
        // --- 当前歌曲信息 ---
        [ObservableProperty]
        private Song _currentSong;
        public event Action RequestClose;
        [RelayCommand]
        private void CloseDetailView()
        {
            // 触发事件
            RequestClose?.Invoke();
        }
        // --- 歌词信息 ---
        public ObservableCollection<LyricLine> Lyrics { get; } = new();

        // --- 播放状态 ---
        [ObservableProperty]
        private bool _isPlaying;

        private readonly PlayerService _playerService;
        private readonly LyricService _lyricService;
        public SongDetailViewModel(PlayerService playerService, LyricService lyricService)
        {
            _playerService = playerService;
            _lyricService = lyricService; // <-- 保存实例
            // 【核心】订阅播放器服务的进度更新事件
            _playerService.ProgressUpdated += OnPlayerProgressUpdated;
            _playerService.CurrentSongChanged += OnPlayerServiceSongChanged;

            // 【核心修正 #2】: 初始化时，从PlayerService获取当前可能已在播放的歌曲
            _currentSong = _playerService.CurrentSong;
            if (_currentSong != null)
            {
                LoadLyrics();
            }
            LoadComments();
            // 填充示例数据，方便UI设计
        }
        [RelayCommand]
        private void ToggleLikeComment(Comment comment)
        {
            if (comment == null) return;

            // 这是一个“乐观更新”
            // 我们先在UI上立即更新状态，然后再在后台调用API

            if (comment.IsLiked)
            {
                // 如果已经点赞了，就取消点赞
                comment.IsLiked = false;
                comment.LikeCount--;
            }
            else
            {
                // 如果没点赞，就点赞
                comment.IsLiked = true;
                comment.LikeCount++;
            }

            // TODO: 在后台异步调用API来将这个点赞/取消点赞的操作同步到服务器
            // Task.Run(() => _commentService.LikeCommentAsync(comment.Id, comment.IsLiked));
        }
        private void LoadComments()
        {
            Comments.Clear();
            // 填充示例评论
            Comments.Add(new Comment { UserName = "芙岚人", Content = "不剧透 (有素质) 的告诉各位：...", Timestamp = "2017-06-07", LikeCount = 71742, IsLiked = true });
            Comments.Add(new Comment { UserName = "ihnsod", Content = "还有最后一句话，希斯特利亚，我这辈子最大的遗憾就是没能娶你 ------ 尤弥尔。", Timestamp = "2017-06-07", LikeCount = 60301 });
            Comments.Add(new Comment { UserName = "路明非x", Content = "“我们为什么要杀死艾伦”\n“因为艾伦要毁灭世界”...", Timestamp = "2023-11-18", LikeCount = 50440 });
        }
        private void OnPlayerServiceSongChanged(Song newSong)
        {
            // 当播放器服务的歌曲改变时，直接更新本ViewModel的属性
            // 因为事件可能在非UI线程触发，所以最好用Dispatcher
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateSong(newSong);
            });
        }

        private void OnPlayerProgressUpdated(TimeSpan currentTime, TimeSpan totalTime)
        {
            // 直接调用我们之前写的逻辑
            UpdateCurrentLyricLineByTime(currentTime);
        }
        public void Cleanup()
        {
            _playerService.ProgressUpdated -= OnPlayerProgressUpdated;
        }
        public void UpdateSong(Song newSong)
        {
            CurrentSong = newSong;
            LoadLyrics();
            LoadComments();
        }
        public void UpdateCurrentLyricLineByTime(TimeSpan currentTime)
        {
            LyricLine nextLine = null;
            // 倒序查找第一个时间戳小于等于当前时间的行
            for (int i = Lyrics.Count - 1; i >= 0; i--)
            {
                if (Lyrics[i].Time <= currentTime)
                {
                    nextLine = Lyrics[i];
                    break;
                }
            }

            // 更新所有行的高亮状态
            foreach (var line in Lyrics)
            {
                line.IsCurrentLine = (line == nextLine);
            }
        }
        private void LoadLyrics()
        {
            Lyrics.Clear();
            if (CurrentSong == null || string.IsNullOrEmpty(CurrentSong.FilePath))
            {
                // 如果没有歌曲信息或文件路径，直接显示无歌词
                Lyrics.Add(new LyricLine { Time = TimeSpan.MaxValue, OriginalText = "(歌曲信息无效)" });
                return;
            }

            List<LyricLine> foundLyrics = null;

            // --- 1. 尝试读取内嵌歌词 ---
            foundLyrics = _lyricService.GetLyricsFromFile(CurrentSong.FilePath);

            // --- 2. 如果没有内嵌歌词，尝试加载外挂.lrc文件 ---
            if (foundLyrics == null || !foundLyrics.Any())
            {
                Debug.WriteLine($"未找到内嵌歌词，尝试查找外挂.lrc文件...");
                foundLyrics = _lyricService.GetLyricsFromLrcFile(CurrentSong.FilePath);
            }

            // --- 3. 如果本地两种方式都失败了，尝试从网络API获取 ---
            if (foundLyrics == null || !foundLyrics.Any())
            {
                Debug.WriteLine($"未找到本地歌词，尝试从API获取...");
                // TODO: 调用网络服务获取歌词
                // foundLyrics = await _lyricApiService.GetLyricsByIdAsync(CurrentSong.Id);
            }

            // --- 4. 最终处理 ---
            if (foundLyrics != null && foundLyrics.Any())
            {
                Debug.WriteLine($"成功加载歌曲 '{CurrentSong.Title}' 的歌词。");
                foreach (var line in foundLyrics)
                {
                    Lyrics.Add(line);
                }
            }
            else
            {
                // 如果所有方法都失败了，显示“暂无歌词”
                Debug.WriteLine($"所有来源均未找到歌曲 '{CurrentSong.Title}' 的歌词。");
                Lyrics.Add(new LyricLine { Time = TimeSpan.MaxValue, OriginalText = "(本歌曲暂无歌词)" });
            }
        }

        
    }
}
