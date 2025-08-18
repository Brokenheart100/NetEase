using NetEase.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NetEase.Services
{
    public enum PlaybackStatus { Playing, Paused, Stopped }

    // PlayerService 现在是一个普通的类，可以被 DI 容器实例化
    public class PlayerService
    {
        // --- 事件 ---
        public event Action<Song> PlayRequested;
        public event Action PlaybackStatusChanged;
        public event Action<Song> CurrentSongChanged;
        public event Action<double> VolumeChanged;
        public event Action<double> SeekRequested;
        public event Action<TimeSpan, TimeSpan> ProgressUpdated;
        // --- 状态属性 ---
        public PlaybackStatus CurrentStatus { get; private set; } = PlaybackStatus.Stopped;
        public Song CurrentSong { get; private set; }
        private List<Song> _currentPlaylist;

        // 关键修改 1: 将构造函数改为 public
        // 这允许依赖注入容器创建这个类的实例
        public PlayerService() { }

        // --- 公共 API (方法保持不变) ---
        public void StartPlayback(Song song, IEnumerable<Song> playlist)
        {
            if (song == null || playlist == null) return;
            _currentPlaylist = playlist.ToList();
            RequestPlay(song);
        }
        // 新增方法，由 MediaPlayerService 调用
        public void UpdateProgress(TimeSpan currentTime, TimeSpan totalTime)
        {
            ProgressUpdated?.Invoke(currentTime, totalTime);
        }
        public void TogglePlayPause()
        {
            if (CurrentSong == null) return;

            if (CurrentStatus == PlaybackStatus.Playing)
            {
                CurrentStatus = PlaybackStatus.Paused;
                CurrentSong.IsPlaying = false; // 暂停时更新状态
            }
            else if (CurrentStatus == PlaybackStatus.Paused)
            {
                CurrentStatus = PlaybackStatus.Playing;
                CurrentSong.IsPlaying = true; // 恢复播放时更新状态
            }
            PlaybackStatusChanged?.Invoke();
        }

        public void PlayNextSong()
        {
            if (!CanChangeTrack()) return;
            int currentIndex = _currentPlaylist.IndexOf(CurrentSong);
            if (currentIndex == -1) return;
            int nextIndex = (currentIndex + 1) % _currentPlaylist.Count;
            RequestPlay(_currentPlaylist[nextIndex]);
        }



        public void PlayPreviousSong()
        {
            if (!CanChangeTrack()) return;
            int currentIndex = _currentPlaylist.IndexOf(CurrentSong);
            if (currentIndex == -1) return;
            int previousIndex = (currentIndex - 1 + _currentPlaylist.Count) % _currentPlaylist.Count;
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
        private bool CanChangeTrack()
        {
            return _currentPlaylist != null && _currentPlaylist.Count > 0 && CurrentSong != null;
        }

        private void RequestPlay(Song songToPlay)
        {
            if (songToPlay == null) return;

            // 关键修改 2: 确保在播放新歌之前，将【上一首】歌曲的状态设为 false
            if (CurrentSong != null && CurrentSong != songToPlay)
            {
                CurrentSong.IsPlaying = false;
            }

            // 更新内部状态
            CurrentSong = songToPlay;
            CurrentStatus = PlaybackStatus.Playing;
            CurrentSong.IsPlaying = true;

            // 依次广播所有相关事件
            CurrentSongChanged?.Invoke(CurrentSong);
            PlaybackStatusChanged?.Invoke();
            PlayRequested?.Invoke(CurrentSong);
        }
    }
}