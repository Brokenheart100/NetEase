using NetEase.Dtos.NetEase.Dtos;
using System.ComponentModel.DataAnnotations;

namespace NetEase.Dtos
{
    public class CreatePlaylistDto
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }
        public string AuthorAvatarUrl { get; set; }
    }

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
        public string AuthorAvatarUrl { get; set; }
    }

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
        public string AuthorAvatarUrl { get; set; }
    }
}
