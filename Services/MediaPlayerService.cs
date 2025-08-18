using NetEase.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NetEase.Services
{
    // 这个服务专门用于控制 MainWindow 中的 MediaElement
    public class MediaPlayerService
    {
        private MediaElement _mediaPlayer;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly PlayerService _playerService; // 它需要 PlayerService 来广播进度

        public MediaPlayerService(PlayerService playerService)
        {
            _playerService = playerService;
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += Timer_Tick;
        }

        // MainWindow 在加载时，会调用这个方法来“注册”它的 MediaElement
        public void Initialize(MediaElement mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;
            _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            // 订阅来自 PlayerService 的命令
            _playerService.PlayRequested += OnPlayRequested;
            _playerService.PlaybackStatusChanged += OnPlaybackStatusChanged;
            _playerService.SeekRequested += OnSeekRequested;
            _playerService.VolumeChanged += OnVolumeChanged;
        }

        private void OnPlayRequested(Song songToPlay)
        {
            if (_mediaPlayer == null || songToPlay == null || string.IsNullOrEmpty(songToPlay.FilePath))
            {
                _timer.Stop();
                return;
            }
            _mediaPlayer.Source = new Uri(songToPlay.FilePath, UriKind.Absolute);
        }

        private void OnPlaybackStatusChanged()
        {
            if (_mediaPlayer == null) return;
            switch (_playerService.CurrentStatus)
            {
                case PlaybackStatus.Playing:
                    _mediaPlayer.Play();
                    _timer.Start();
                    break;
                case PlaybackStatus.Paused:
                    _mediaPlayer.Pause();
                    _timer.Stop();
                    break;
                case PlaybackStatus.Stopped:
                    _mediaPlayer.Stop();
                    _timer.Stop();
                    break;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_mediaPlayer.Source == null || !_mediaPlayer.NaturalDuration.HasTimeSpan) return;

            // 通知 PlayerService 更新进度
            _playerService.UpdateProgress(
                _mediaPlayer.Position,
                _mediaPlayer.NaturalDuration.TimeSpan);
        }

        private void OnSeekRequested(double percentage)
        {
            if (_mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                var newPosition = TimeSpan.FromSeconds(
                    _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds * (percentage / 100.0));
                _mediaPlayer.Position = newPosition;
            }
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            _playerService.PlayNextSong();
        }

        private void OnVolumeChanged(double newVolume)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Volume = newVolume;
            }
        }
    }
}