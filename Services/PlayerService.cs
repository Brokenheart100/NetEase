using NetEase.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NetEase.Services
{
    /// <summary>
    /// 播放状态枚举，定义播放器可能的三种状态
    /// </summary>
    public enum PlaybackStatus { Playing, Paused, Stopped }

    /// <summary>
    /// 播放器服务类，负责管理音乐播放逻辑、状态跟踪及事件通知
    /// 设计为可被依赖注入(DI)容器实例化的普通类
    /// </summary>
    public class PlayerService
    {
        /// <summary>
        /// 媒体元素对象，用于实际处理媒体文件的播放（如音频文件）
        /// 通常由外部视图层注入或关联
        /// </summary>
        private MediaElement _mediaElement;

        /// <summary>
        /// 进度更新定时器，用于定期触发播放进度更新事件
        /// </summary>
        private DispatcherTimer _progressTimer;

        // --- 事件定义 ---

        /// <summary>
        /// 播放请求事件，当需要播放指定歌曲时触发
        /// 事件参数为待播放的歌曲对象
        /// </summary>
        public event Action<Song> PlayRequested;

        /// <summary>
        /// 播放状态变更事件，当播放状态（播放/暂停/停止）发生变化时触发
        /// </summary>
        public event Action PlaybackStatusChanged;

        /// <summary>
        /// 当前歌曲变更事件，当切换到新歌曲时触发
        /// 事件参数为新的当前歌曲对象
        /// </summary>
        public event Action<Song> CurrentSongChanged;

        /// <summary>
        /// 音量变更事件，当音量设置发生变化时触发
        /// 事件参数为新的音量值（0.0-1.0之间）
        /// </summary>
        public event Action<double> VolumeChanged;

        /// <summary>
        ///  seek 请求事件，当需要调整播放进度时触发
        ///  事件参数为进度百分比（0.0-1.0之间）
        /// </summary>
        public event Action<double> SeekRequested;

        /// <summary>
        /// 进度更新事件，定期触发以通知当前播放进度
        /// 事件参数为当前播放时间和总时长
        /// </summary>
        public event Action<TimeSpan, TimeSpan> ProgressUpdated;
        // --- 状态属性 ---

        /// <summary>
        /// 当前播放状态（播放/暂停/停止）
        /// 私有setter确保状态只能通过内部方法修改
        /// </summary>
        public PlaybackStatus CurrentStatus { get; private set; } = PlaybackStatus.Stopped;

        /// <summary>
        /// 当前正在播放（或暂停）的歌曲
        /// 私有setter确保只能通过内部方法修改
        /// </summary>
        public Song CurrentSong { get; private set; }

        /// <summary>
        /// 当前播放列表，存储当前可播放的歌曲集合
        /// </summary>
        private List<Song> _currentPlaylist;

        /// <summary>
        /// 构造函数，初始化播放器服务
        /// 设为public以允许依赖注入容器创建实例
        /// </summary>
        public PlayerService()
        {
            // 初始化进度更新定时器
            _progressTimer = new DispatcherTimer();
            // 设置定时器间隔为200毫秒（每200ms更新一次进度）
            _progressTimer.Interval = TimeSpan.FromMilliseconds(200);
            // 绑定定时器触发事件
            _progressTimer.Tick += OnTimerTick;
        }

        /// <summary>
        /// 定时器触发事件处理方法
        /// 用于定期检查并更新播放进度
        /// </summary>
        private void OnTimerTick(object sender, EventArgs e)
        {
            Debug.WriteLine($"Timer ticked for progress update. OnTimerTick({sender}, EventArgs {e})");
            // 检查媒体元素是否存在且已加载有效时长
            if (_mediaElement != null && _mediaElement.NaturalDuration.HasTimeSpan)
            {
                // 触发进度更新事件，传递当前位置和总时长
                ProgressUpdated?.Invoke(_mediaElement.Position, _mediaElement.NaturalDuration.TimeSpan);
            }
        }

        /// <summary>
        /// 停止播放操作
        /// </summary>
        public void StopPlayback()
        {
            // 停止媒体元素播放
            _mediaElement.Stop();
            // 更新当前状态为停止
            CurrentStatus = PlaybackStatus.Stopped;
            // 停止进度定时器（停止播放时不再更新进度）
            _progressTimer.Stop();
        }

        /// <summary>
        /// 开始播放指定歌曲和播放列表
        /// </summary>
        /// <param name="song">要播放的歌曲</param>
        /// <param name="playlist">当前播放列表</param>
        public void StartPlayback(Song song, IEnumerable<Song> playlist)
        {
            // 校验参数有效性（歌曲或列表为空则不执行）
            if (song == null || playlist == null) return;

            // 保存当前播放列表（转换为List便于索引操作）
            _currentPlaylist = playlist.ToList();
            // 发起播放请求
            RequestPlay(song);
            // 启动进度定时器（开始更新进度）
            _progressTimer.Start();
            // 更新播放状态为播放中
            CurrentStatus = PlaybackStatus.Playing;
            Debug.WriteLine($"StartPlayback {CurrentSong.Title}");
            CurrentSongChanged?.Invoke(CurrentSong);
        }

        /// <summary>
        /// 更新播放进度（由外部媒体服务调用）
        /// </summary>
        /// <param name="currentTime">当前播放时间</param>
        /// <param name="totalTime">总时长</param>
        public void UpdateProgress(TimeSpan currentTime, TimeSpan totalTime)
        {
            // 触发进度更新事件
            ProgressUpdated?.Invoke(currentTime, totalTime);
        }

        /// <summary>
        /// 切换播放/暂停状态
        /// </summary>
        public void TogglePlayPause()
        {
            // 如果当前没有播放的歌曲，则不执行操作
            if (CurrentSong == null) return;

            // 根据当前状态切换
            if (CurrentStatus == PlaybackStatus.Playing)
            {
                // 从播放切换到暂停
                CurrentStatus = PlaybackStatus.Paused;
                CurrentSong.IsPlaying = false; // 更新歌曲状态为未播放
                _progressTimer.Stop(); // 暂停时停止进度更新
            }
            else if (CurrentStatus == PlaybackStatus.Paused)
            {
                // 从暂停切换到播放
                CurrentStatus = PlaybackStatus.Playing;
                CurrentSong.IsPlaying = true; // 更新歌曲状态为播放中
                _progressTimer.Start(); // 恢复进度更新
            }

            // 触发播放状态变更事件，通知外部状态变化
            PlaybackStatusChanged?.Invoke();
        }

        /// <summary>
        /// 播放下一首歌曲（循环播放列表）
        /// </summary>
        public void PlayNextSong()
        {
            // 检查是否可以切换歌曲（播放列表有效且有当前歌曲）
            if (!CanChangeTrack()) return;

            // 获取当前歌曲在播放列表中的索引
            int currentIndex = _currentPlaylist.IndexOf(CurrentSong);
            // 索引无效则退出
            if (currentIndex == -1) return;

            // 计算下一首索引（循环逻辑：最后一首的下一首是第一首）
            int nextIndex = (currentIndex + 1) % _currentPlaylist.Count;
            // 播放下一首歌曲
            RequestPlay(_currentPlaylist[nextIndex]);
        }

        /// <summary>
        /// 播放上一首歌曲（循环播放列表）
        /// </summary>
        public void PlayPreviousSong()
        {
            // 检查是否可以切换歌曲
            if (!CanChangeTrack()) return;

            // 获取当前歌曲索引
            int currentIndex = _currentPlaylist.IndexOf(CurrentSong);
            if (currentIndex == -1) return;

            // 计算上一首索引（循环逻辑：第一首的上一首是最后一首）
            int previousIndex = (currentIndex - 1 + _currentPlaylist.Count) % _currentPlaylist.Count;
            // 播放上一首歌曲
            RequestPlay(_currentPlaylist[previousIndex]);
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="newVolume">新音量值（会被限制在0.0-1.0之间）</param>
        public void SetVolume(double newVolume)
        {
            // 触发音量变更事件，传递限制后的音量值
            VolumeChanged?.Invoke(Math.Clamp(newVolume, 0.0, 1.0));
        }

        /// <summary>
        /// 调整播放进度
        /// </summary>
        /// <param name="percentage">进度百分比（0.0-1.0之间）</param>
        public void Seek(double percentage)
        {
            // 触发seek请求事件
            SeekRequested?.Invoke(percentage);
        }

        /// <summary>
        /// 检查是否可以切换歌曲（辅助方法）
        /// </summary>
        /// <returns>是否可以切换歌曲</returns>
        private bool CanChangeTrack()
        {
            // 播放列表不为空、有歌曲且存在当前播放的歌曲时，允许切换
            return _currentPlaylist != null && _currentPlaylist.Count > 0 && CurrentSong != null;
        }

        /// <summary>
        /// 处理播放请求（核心方法）
        /// 负责更新内部状态并触发相关事件
        /// </summary>
        /// <param name="songToPlay">要播放的歌曲</param>
        private void RequestPlay(Song songToPlay)
        {
            // 校验歌曲有效性
            if (songToPlay == null) return;

            // 处理上一首歌曲状态：如果切换了歌曲，将上一首标记为未播放
            if (CurrentSong != null && CurrentSong != songToPlay)
            {
                CurrentSong.IsPlaying = false;
            }

            // 更新内部状态
            CurrentSong = songToPlay; // 设置当前歌曲为目标歌曲
            CurrentStatus = PlaybackStatus.Playing; // 更新状态为播放中
            CurrentSong.IsPlaying = true; // 标记当前歌曲为播放中

            // 触发相关事件（通知外部状态变更）
            CurrentSongChanged?.Invoke(CurrentSong); // 通知当前歌曲已变更
            PlaybackStatusChanged?.Invoke(); // 通知播放状态已变更
            PlayRequested?.Invoke(CurrentSong); // 通知需要播放该歌曲
        }
    }
}