        using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Models
{
    public class Playlist
    {
        public string CoverImageUrl { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; } // 副标题，用于换行
        public bool IsVip { get; set; } // 是否有VIP角标
        public ObservableCollection<Song> Songs { get; set; } // 新增：存储这个歌单里的所有歌曲
    }
}
