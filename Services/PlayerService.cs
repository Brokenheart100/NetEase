using NetEase.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NetEase.Services
{
    public enum PlaybackStatus { Playing, Paused, Stopped }

    public class PlayerService
    {
        //private static readonly Lazy<PlayerService> _instance = new Lazy<PlayerService>(() => new PlayerService());
        //public static PlayerService Instance => _instance.Value;

        // --- 事件 ---
        public event Action<Song> PlayRequested;
        public event Action PlaybackStatusChanged;
        public event Action<Song> CurrentSongChanged;
        public event Action<double> VolumeChanged;
        public event Action<double> SeekRequested;

        // --- 状态属性 (单一数据源) ---
        public PlaybackStatus CurrentStatus { get; private set; } = PlaybackStatus.Stopped;
        public Song CurrentSong { get; private set; }
        private List<Song> _currentPlaylist;

        private PlayerService() { }

        // --- 公共 API (提供给 ViewModel 调用) ---

        /// <summary>
        /// 从一个播放列表开始播放一首指定的歌曲。这是从外部启动播放的唯一入口。
        /// </summary>
        public void StartPlayback(Song song, IEnumerable<Song> playlist)
        {
            if (song == null || playlist == null) return;

            _currentPlaylist = playlist.ToList();

            // 调用私有方法来处理实际的播放逻辑
            RequestPlay(song);
        }

        public void TogglePlayPause()
        {
            if (CurrentSong == null) return; // 如果没有歌曲在播放，则不执行任何操作

            if (CurrentStatus == PlaybackStatus.Playing)
            {
                CurrentStatus = PlaybackStatus.Paused;
            }
            else if (CurrentStatus == PlaybackStatus.Paused)
            {
                CurrentStatus = PlaybackStatus.Playing;
            }

            // 只广播状态变更事件
            PlaybackStatusChanged?.Invoke();
        }

        public void PlayNextSong()
        {
            if (!CanChangeTrack()) return;

            int currentIndex = _currentPlaylist.IndexOf(CurrentSong);
            if (currentIndex == -1) return;

            int nextIndex = (currentIndex + 1) % _currentPlaylist.Count;

            Debug.WriteLine($"请求下一曲：{_currentPlaylist[nextIndex].Title}");
            RequestPlay(_currentPlaylist[nextIndex]);
        }

        public void PlayPreviousSong()
        {
            if (!CanChangeTrack()) return;

            int currentIndex = _currentPlaylist.IndexOf(CurrentSong);
            if (currentIndex == -1) return;

            int previousIndex = (currentIndex - 1 + _currentPlaylist.Count) % _currentPlaylist.Count;

            Debug.WriteLine($"请求上一曲：{_currentPlaylist[previousIndex].Title}");
            RequestPlay(_currentPlaylist[previousIndex]);
        }

        public void SetVolume(double newVolume)
        {
            VolumeChanged?.Invoke(Math.Clamp(newVolume, 0.0, 1.0));
        }

        public void Seek(double percentage)
        {
            SeekRequested?.Invoke(percentage);
        }

        // --- 私有辅助方法 ---

        /// <summary>
        /// 检查是否满足切歌的条件。
        /// </summary>
        private bool CanChangeTrack()
        {
            return _currentPlaylist != null && _currentPlaylist.Count > 0 && CurrentSong != null;
        }

        /// <summary>
        /// 统一的核心播放请求方法。所有需要切换并播放新歌的操作都必须调用此方法。
        /// </summary>
        private void RequestPlay(Song songToPlay)
        {
            if (songToPlay == null) return;
            if (CurrentSong != null)
            {
                CurrentSong.IsPlaying = false;
            }
            // 1. 更新内部状态
            CurrentSong = songToPlay;   
            CurrentStatus = PlaybackStatus.Playing;

            // 3. 将当前歌曲的状态设为 true
            CurrentSong.IsPlaying = true;

            // 2. 依次广播所有相关事件，通知整个应用程序
            CurrentSongChanged?.Invoke(CurrentSong);
            PlaybackStatusChanged?.Invoke();
            PlayRequested?.Invoke(CurrentSong);
        }
    }
}