        using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

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
        // --- 新增：添加 IsSelected 可通知属性 ---
        [ObservableProperty]
        private bool _isSelected;
    }
}
