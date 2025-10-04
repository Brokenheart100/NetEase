namespace NetEase.Dtos
{
    public class ChatMessageDto
    {
        public long Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public string MimeType { get; set; } // 【新增】存储图片类型，如 "image/png"
        public string ImageDataBase64 { get; set; }
        public bool IsRead { get; set; } // 【新增】确保这个字段存在
    }

    public class SendMessageDto
    {
        public int ReceiverId { get; set; }
        public string Content { get; set; }
        public string? MimeType { get; set; } // 如果是图片，需要提供MimeType
    }
}
