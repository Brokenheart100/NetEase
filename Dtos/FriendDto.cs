namespace NetEase.Dtos
{
    public class FriendDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsOnline { get; set; } // 可以在未来实现
    }
}
