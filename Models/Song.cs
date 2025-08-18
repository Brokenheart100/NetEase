using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NetEase.Models
{
    public partial class Song : ObservableObject
    {
        public int Index { get; set; }
        public string CoverImageUrl { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; } // 括号里的副标题
        public List<SongTag> Tags { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public bool IsLiked { get; set; }
        public string Duration { get; set; }
        public string FilePath { get; set; } // 新增：存储文件在磁盘上的真实路径
        // 将 CoverImageUrl 从 string 修改为 ImageSource
        public ImageSource CoverImage { get; set; }
        [ObservableProperty]
        private bool _isPlaying; // 当此属性改变时，会自动通知 UI
    }
}
