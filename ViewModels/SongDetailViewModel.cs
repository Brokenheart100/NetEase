using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NetEase.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.ViewModels
{
    // 定义一个歌词行模型
    public partial class LyricLine : ObservableObject
    {
        public string OriginalText { get; set; }
        public string TranslatedText { get; set; }

        [ObservableProperty]
        private bool _isCurrentLine; // 标记是否是当前播放的行
    }

    public partial class SongDetailViewModel : BaseViewModel
    {
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

        public SongDetailViewModel()
        {
            // 填充示例数据，方便UI设计
            CurrentSong = new Song
            {
                Title = "ダイヤモンドの純度 ~ Yukino Ballad",
                Artist = "早見沙織",
                Album = "雪ノ下雪乃 (CV.早見沙織)",
                CoverImageUrl = "..." // 封面图URL
            };

            LoadLyrics();
        }


        public void UpdateSong(Song newSong)
        {
            CurrentSong = newSong;
            // TODO: 根据 newSong.Id 从服务加载真实的歌词
            LoadLyrics();
        }

        private void LoadLyrics()
        {
            Lyrics.Clear();
            // 示例歌词
            Lyrics.Add(new LyricLine { OriginalText = "作词: 藤林聖子", IsCurrentLine = true }); // 假设第一行是当前行
            Lyrics.Add(new LyricLine { OriginalText = "作曲: 黒須克彦" });
            Lyrics.Add(new LyricLine { OriginalText = "編曲: 安瀬聖" });
            Lyrics.Add(new LyricLine { OriginalText = "" });
            Lyrics.Add(new LyricLine { OriginalText = "君の横顔が", TranslatedText = "你的侧脸" });
            Lyrics.Add(new LyricLine { OriginalText = "見つめているのは", TranslatedText = "正出神地凝望着" });
            Lyrics.Add(new LyricLine { OriginalText = "次の季節だと", TranslatedText = "下一个季节" });
        }

        // 这个方法会被播放器服务定时调用，以更新当前高亮的歌词行
        public void UpdateCurrentLyricLine(int lineIndex)
        {
            for (int i = 0; i < Lyrics.Count; i++)
            {
                Lyrics[i].IsCurrentLine = (i == lineIndex);
            }
        }
    }
}
