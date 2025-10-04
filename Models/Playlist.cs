using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace NetEase.Models
{
    public partial class Playlist : ObservableObject
    {
        public int Id { get; set; }
        public string CoverImageUrl { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; } // 副标题，用于换行
        public bool IsVip { get; set; } // 是否有VIP角标
        public ObservableCollection<Song> Songs { get; set; } // 新增：存储这个歌单里的所有歌曲
        public int TrackCount { get; set; }
        public string? Description { get; internal set; }

        [ObservableProperty]
        private bool _isSelected;
    }
}
