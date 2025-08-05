using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Models
{
    // 定义歌曲状态的枚举
    public enum RankStatus { None, New, Up, Down }

    public class RankedSong
    {
        public int Rank { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public RankStatus Status { get; set; } = RankStatus.New;
    }
}
