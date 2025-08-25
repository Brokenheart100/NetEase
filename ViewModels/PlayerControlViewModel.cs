using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetEase.Models;
using NetEase.Services;
using System.Diagnostics;
using System.Windows.Input;

namespace NetEase.ViewModels
{
    public partial class PlayerControlViewModel : BaseViewModel
    {
        private readonly PlayerService _playerService;

        // --- 属性 ---
        [ObservableProperty]
        private Song _currentSong;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(VolumeIcon))]
        private double _playbackVolume;

        [ObservableProperty]
        private string _playPauseIcon;

        [ObservableProperty]
        private double _currentProgress;

        [ObservableProperty]
        private string _currentTime;

        [ObservableProperty]
        private string _totalTime;

        public bool IsDragging { get; set; } = false;
        private double _lastPlaybackVolume;

        public string VolumeIcon
        {
            get
            {
                if (PlaybackVolume == 0) return "\uE992";
                if (PlaybackVolume < 0.33) return "\uE993";
                if (PlaybackVolume < 0.66) return "\uE994";
                return "\uE995";
            }
        }

        // --- 命令 ---
        public ICommand MuteCommand { get; }
        public ICommand TogglePlayPauseCommand { get; }
        public ICommand NextSongCommand { get; }
        public ICommand PreviousSongCommand { get; }

        public PlayerControlViewModel(PlayerService playerService)
        {
            _playerService = playerService;

            // 1. 初始化所有命令，全部使用注入的 _playerService 实例
            MuteCommand = new RelayCommand(ToggleMute);
            TogglePlayPauseCommand = new RelayCommand(_playerService.TogglePlayPause);
            NextSongCommand = new RelayCommand(_playerService.PlayNextSong);
            PreviousSongCommand = new RelayCommand(_playerService.PlayPreviousSong);

            // 2. 初始化所有状态
            _playbackVolume = 1.0;
            _lastPlaybackVolume = 1.0;

            // 3. 订阅服务事件
            _playerService.PlaybackStatusChanged += OnPlaybackStatusChanged;
            _playerService.CurrentSongChanged += OnCurrentSongChanged; // **添加了对 CurrentSongChanged 的订阅**
            _playerService.ProgressUpdated += OnProgressUpdated; // 新增订阅
            // 4. 初始化UI状态
            ResetForNewSong();
            OnPlaybackStatusChanged(); // 设置正确的初始图标
        }
        // 新增事件处理器
        private void OnProgressUpdated(TimeSpan currentTime, TimeSpan totalTime)
        {
            if (!IsDragging)
            {
                CurrentProgress = currentTime.TotalSeconds / totalTime.TotalSeconds * 100;
                CurrentTime = currentTime.ToString(@"mm\:ss");
                TotalTime = totalTime.ToString(@"mm\:ss");
            }
        }
        // --- 事件处理器 ---

        partial void OnCurrentSongChanged(Song newSong)
        {
            CurrentSong = newSong;
        }



        private void OnPlaybackStatusChanged()
        {
            // 关键修正：使用注入的 _playerService 实例来获取状态
            switch (_playerService.CurrentStatus)
            {
                case PlaybackStatus.Playing:
                    PlayPauseIcon = "\uE769"; // Pause icon
                    break;
                case PlaybackStatus.Paused:
                case PlaybackStatus.Stopped:
                    PlayPauseIcon = "\uE768"; // Play icon
                    break;
            }
        }

        // --- 由 CommunityToolkit.Mvvm 自动调用的 Partial 方法 ---

        partial void OnPlaybackVolumeChanged(double value)
        {
            _playerService.SetVolume(value);
        }

        partial void OnCurrentProgressChanged(double value)
        {
            if (IsDragging)
            {
                _playerService.Seek(value);
            }
        }

        // --- 公共方法 ---
        public void ResetForNewSong()
        {
            CurrentProgress = 0;
            CurrentTime = "00:00";
            TotalTime = "00:00";
        }

        // --- 私有命令逻辑 ---
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
    }
}