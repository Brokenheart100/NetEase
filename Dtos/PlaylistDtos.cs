using NetEase.Dtos.NetEase.Dtos;
using System.ComponentModel.DataAnnotations;

namespace NetEase.Dtos
{
    public class CreatePlaylistDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }

    // --- PlaylistDetailDto ---
    // 这个类用于表示从 API 获取到的播放列表的完整信息
    public class PlaylistDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public string UserName { get; set; }
        public DateTime CreateDate { get; set; }
        public List<SongDto> Songs { get; set; }
        public int TrackCount { get; set; }
    }

    // --- PlaylistSummaryDto (用于列表显示) ---
    public class PlaylistSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }
        public string UserName { get; set; }
        public DateTime CreateDate { get; set; }
        public List<SongDto> Songs { get; set; }
        public int TrackCount { get; set; }
    }
}
