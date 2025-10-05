namespace NetEase.Dtos
{
    public class UpdatePlaylistDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }
        // public List<int> TagIds { get; set; } // 未来可以添加标签
    }
}
