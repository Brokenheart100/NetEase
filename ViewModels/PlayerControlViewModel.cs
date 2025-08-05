using CommunityToolkit.Mvvm.ComponentModel;
using NetEase.Models;
using NetEase.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.ViewModels
{
    public partial class PlayerControlViewModel : BaseViewModel
    {
        // 使用 [ObservableProperty] 自动生成属性和通知
        [ObservableProperty]
        private Song _currentSong;

        [ObservableProperty]
        private double _currentProgress;

        [ObservableProperty]
        private string _currentTime="00:00";

        [ObservableProperty]
        private string _totalTime = "00:00"; // 歌曲总时长

        [ObservableProperty]
        private bool _isPlaying;

        // 关键标志位：表示用户是否正在拖动滑块
        public bool IsDragging { get; set; } = false;

        // 当 CurrentProgress 属性值改变时，CommunityToolkit 会自动调用此方法
        // 我们利用它来在用户拖动滑块时发送“寻道”请求
        partial void OnCurrentProgressChanged(double value)
        {
            // 仅当是用户拖动行为导致的值变化时，才发送寻道请求
            if (IsDragging)
            {
                PlayerService.Instance.Seek(value);
            }
        }

        // --- 新增：当新歌曲开始播放时，重置状态的方法 ---
        public void ResetForNewSong()
        {
            CurrentProgress = 0;
            CurrentTime = "00:00";
            TotalTime = "00:00";
        }

        public PlayerControlViewModel()
        {
            // 初始化示例数据
            CurrentSong = new Song
            {
                CoverImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\36.jpg",
                Title = "Call of Silence",
                Artist = "Clear Sky",
                Duration = "05:10"
            };

            CurrentProgress = 30; // 假设进度为30%
            CurrentTime = "00:00 / 05:10";
            IsPlaying = true;
        }
    }
}
