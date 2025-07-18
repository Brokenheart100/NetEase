using System.Collections.Generic;

namespace NetEase
{
    // 定义歌曲标签的类型，用于在界面上显示不同颜色
    public enum TagType { Master, VIP, Trial, MV, Other }

    // 代表一个小标签，如 "VIP", "MV"
    public class SongTag
    {
        public string Text { get; set; }
        public TagType Type { get; set; }
    }

    // 代表一整首歌曲的数据
    public class Song
    {
        public string Index { get; set; } // 序号，用字符串 ताकि "01", "02" 显示
        public string CoverImagePath { get; set; } // 封面图片路径
        public string Title { get; set; } // 歌名
        public List<SongTag> Tags { get; set; } // 标签列表
        public string Artist { get; set; } // 艺术家
        public string Album { get; set; } // 专辑名
        public string Duration { get; set; } // 时长
        public bool IsLiked { get; set; } // 是否喜欢
    }
}