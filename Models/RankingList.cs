using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Models
{
    public class RankingList
    {
        public string Title { get; set; } // "飙升榜"
        public string UpdateInfo { get; set; } // "刚刚更新"
        public string CoverImageUrl { get; set; }
        public List<RankedSong> TopSongs { get; set; }
    }
}
