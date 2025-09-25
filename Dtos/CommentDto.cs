using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetEase.Dtos
{
    // 用于从API接收评论数据
    public class CommentDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("user")]
        public UserInfoDto User { get; set; }

        [JsonPropertyName("likeCount")]
        public int LikeCount { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("replies")]
        public List<ReplyDto> Replies { get; set; }
    }

    public class UserInfoDto
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }
        [JsonPropertyName("avatarUrl")]
        public string AvatarUrl { get; set; }
    }

    public class ReplyDto { /* ... */ }

    // 用于向API发送创建评论的请求
    public class CreateCommentDto
    {
        public string SubjectId { get; set; }
        public string SubjectType { get; set; }
        public string Content { get; set; }
    }
}
