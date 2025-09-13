using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetEase.Models;
using NetEase.Services;
using System;
using System.Diagnostics;

// 播放器控制相关的视图模型命名空间
namespace NetEase.ViewModels
{
    /// <summary>
    /// 播放器控制视图模型，继承自基础视图模型
    /// 负责处理播放器的各种控制逻辑（播放/暂停、音量调节、进度更新等）
    /// </summary>
    public partial class PlayerControlViewModel : BaseViewModel
    {
        // 播放器服务，用于处理实际的音频播放逻辑
        private readonly PlayerService _playerService;
        // 服务提供者，用于获取其他服务实例
        private readonly IServiceProvider _serviceProvider;

        // --- 事件定义 ---
        /// <summary>
        /// 当请求显示歌曲详情时触发的事件
        /// </summary>
        public event EventHandler<Song> ShowSongDetailRequested;

        /// <summary>
        /// 当前正在播放的歌曲
        /// </summary>
        [ObservableProperty] private Song _currentSong;

        /// <summary>
        /// 播放音量（0.0-1.0范围）
        /// 音量变化时会通知VolumeIcon属性更新
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(VolumeIcon))]
        private double _playbackVolume;

        /// <summary>
        /// 播放/暂停按钮图标（使用Unicode字符表示）
        /// </summary>
        [ObservableProperty] private string _playPauseIcon;

        /// <summary>
        /// 当前播放进度（百分比，0-100）
        /// </summary>
        [ObservableProperty] private double _currentProgress;

        /// <summary>
        /// 当前播放时间（格式化字符串，如"02:30"）
        /// </summary>
        [ObservableProperty] private string _currentTime;

        /// <summary>
        /// 歌曲总时长（格式化字符串，如"03:45"）
        /// </summary>
        [ObservableProperty] private string _totalTime;

        /// <summary>
        /// 是否正在拖动进度条（用于避免拖动时频繁更新进度）
        /// </summary>
        public bool IsDragging { get; set; } = false;

        /// <summary>
        /// 记录最后一次非静音状态的音量（用于取消静音时恢复）
        /// </summary>
        private double _lastPlaybackVolume;

        /// <summary>
        /// 音量图标（根据当前音量大小显示不同图标）
        /// 使用Unicode字符表示不同音量状态的图标
        /// </summary>
        public string VolumeIcon
        {
            get
            {
                if (PlaybackVolume == 0) return "\uE992"; // 静音图标
                if (PlaybackVolume < 0.33) return "\uE993"; // 低音量图标
                if (PlaybackVolume < 0.66) return "\uE994"; // 中音量图标
                return "\uE995"; // 高音量图标
            }
        }

        /// <summary>
        /// 构造函数，注入依赖服务
        /// </summary>
        /// <param name="playerService">播放器服务实例</param>
        /// <param name="serviceProvider">服务提供者实例</param>
        public PlayerControlViewModel(PlayerService playerService, IServiceProvider serviceProvider)
        {
            _playerService = playerService;
            _serviceProvider = serviceProvider;

            // 初始化音量（最大音量）
            _playbackVolume = 1.0;
            _lastPlaybackVolume = 1.0;

            // 订阅播放器服务的事件，以便响应状态变化
            _playerService.PlaybackStatusChanged += OnPlaybackStatusChanged;
            _playerService.CurrentSongChanged += OnCurrentSongChanged;
            _playerService.ProgressUpdated += OnProgressUpdated;

            // 初始化新歌曲的状态
            ResetForNewSong();
            // 初始化播放/暂停图标
            OnPlaybackStatusChanged();
        }

        /// <summary>
        /// 切换静音状态的命令
        /// - 非静音时：记录当前音量并设为0（静音）
        /// - 静音时：恢复到上次记录的音量（默认为最大音量）
        /// </summary>
        [RelayCommand]
        private void ToggleMute()
        {
            if (PlaybackVolume > 0)
            {
                _lastPlaybackVolume = PlaybackVolume;
                PlaybackVolume = 0;
            }
            else
            {
                PlaybackVolume = _lastPlaybackVolume > 0 ? _lastPlaybackVolume : 1.0;
            }
        }

        /// <summary>
        /// 切换播放/暂停状态的命令
        /// 委托给播放器服务处理实际逻辑
        /// </summary>
        [RelayCommand]
        private void TogglePlayPause() => _playerService.TogglePlayPause();

        /// <summary>
        /// 播放下一首歌曲的命令
        /// 委托给播放器服务处理实际逻辑
        /// </summary>
        [RelayCommand]
        private void NextSong()
        {
            Debug.WriteLine($"Enter NextSong()");
            _playerService.PlayNextSong();
        }

        /// <summary>
        /// 播放上一首歌曲的命令
        /// 委托给播放器服务处理实际逻辑
        /// </summary>
        [RelayCommand]
        private void PreviousSong() => _playerService.PlayPreviousSong();

        /// <summary>
        /// 显示当前歌曲详情的命令
        /// 触发ShowSongDetailRequested事件通知UI层显示详情
        /// </summary>
        [RelayCommand]
        private void ShowSongDetail()
        {
            if (CurrentSong != null)
            {
                ShowSongDetailRequested?.Invoke(this, CurrentSong);
            }
        }

        // --- 事件处理器（响应播放器服务的事件） ---

        /// <summary>
        /// 处理播放进度更新事件
        /// 更新当前进度、当前时间和总时长的显示
        /// 拖动进度条时不更新（避免冲突）
        /// </summary>
        /// <param name="currentTime">当前播放时间</param>
        /// <param name="totalTime">歌曲总时长</param>
        private void OnProgressUpdated(TimeSpan currentTime, TimeSpan totalTime)
        {
            if (!IsDragging)
            {
                // 避免除以零的错误（歌曲时长为0时不更新进度）
                if (totalTime.TotalSeconds > 0)
                {
                    CurrentProgress = (currentTime.TotalSeconds / totalTime.TotalSeconds) * 100;
                }
                // 格式化时间显示（mm:ss格式）
                CurrentTime = currentTime.ToString(@"mm\:ss");
                TotalTime = totalTime.ToString(@"mm\:ss");
            }
        }

        /// <summary>
        /// 当前歌曲变化时的处理方法（由ObservableProperty自动生成调用）
        /// 更新当前歌曲并重置进度信息
        /// </summary>
        /// <param name="newSong">新的歌曲实例</param>
        partial void OnCurrentSongChanged(Song newSong)
        {
            CurrentSong = newSong;
            // 歌曲切换时重置进度条和时间显示
            ResetForNewSong();
        }

        /// <summary>
        /// 播放状态变化时的处理方法
        /// 根据当前播放状态更新播放/暂停图标
        /// </summary>
        private void OnPlaybackStatusChanged()
        {
            switch (_playerService.CurrentStatus)
            {
                case PlaybackStatus.Playing:
                    PlayPauseIcon = "\uE769"; // 暂停图标
                    break;
                case PlaybackStatus.Paused:
                case PlaybackStatus.Stopped:
                    PlayPauseIcon = "\uE768"; // 播放图标
                    break;
            }
        }

        // --- 部分方法（由ObservableProperty自动生成，用于属性变化时的额外处理） ---

        /// <summary>
        /// 播放音量变化时的处理方法
        /// 通知播放器服务更新实际播放音量
        /// </summary>
        /// <param name="value">新的音量值</param>
        partial void OnPlaybackVolumeChanged(double value) => _playerService.SetVolume(value);

        /// <summary>
        /// 当前进度变化时的处理方法
        /// 仅在拖动进度条时（IsDragging为true）才通知播放器服务调整播放进度
        /// 避免正常播放时的进度更新触发重复调整
        /// </summary>
        /// <param name="value">新的进度值（百分比）</param>
        partial void OnCurrentProgressChanged(double value)
        {
            if (IsDragging)
            {
                _playerService.Seek(value);
            }
        }

        // --- 公共方法 ---

        /// <summary>
        /// 为新歌曲重置进度信息
        /// 将进度条、当前时间和总时长重置为初始状态
        /// </summary>
        public void ResetForNewSong()
        {
            CurrentProgress = 0;
            CurrentTime = "00:00";
            TotalTime = "00:00";
        }
    }
}