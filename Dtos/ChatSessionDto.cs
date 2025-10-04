namespace NetEase.Dtos
{
    public class ChatSessionDto
    {
        public int ContactId { get; set; } // 对方的ID
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }
}
