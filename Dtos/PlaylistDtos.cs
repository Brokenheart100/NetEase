using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Dtos
{
    // --- SongDto ---
    // 这个类用于表示从 API 获取到的单首歌曲的详细信息
    public class SongDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }

        // --- 在这里添加所有缺失的属性 ---
        public string Album { get; set; }
        public string Duration { get; set; }
        public string FilePath { get; set; }
        public string CoverImageUrl { get; set; }
    }

    // --- PlaylistDetailDto ---
    // 这个类用于表示从 API 获取到的播放列表的完整信息
    public class PlaylistDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }

        // --- 在这里添加所有缺失的属性 ---
        public string UserName { get; set; }
        public DateTime CreateDate { get; set; }

        public List<SongDto> Songs { get; set; }
    }

    // --- PlaylistSummaryDto (用于列表显示) ---
    public class PlaylistSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CoverImageUrl { get; set; }
    }
}
