using NetEase.Dtos;
using NetEase.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;


namespace NetEase.Services
{
    public class CommentService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService; // 注入AuthService以获取Token

        // 构造函数接收一个已经配置好 BaseAddress 的 HttpClient
        public CommentService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        /// <summary>
        /// 异步获取指定主题的评论列表
        /// </summary>
        /// <param name="subjectId">主题ID (e.g., "playlist_123")</param>
        /// <returns>评论模型列表</returns>
        public async Task<List<Comment>> GetCommentsAsync(string subjectId)
        {
            if (string.IsNullOrEmpty(subjectId)) return new List<Comment>();

            try
            {
                // 构建请求的相对URL，例如: api/comments/playlist_123
                var requestUri = $"api/comments/{subjectId}";
                Debug.WriteLine($"[CommentService] Getting comments from: {_httpClient.BaseAddress}{requestUri}");

                // GetFromJsonAsync 会自动发送GET请求，并将返回的JSON反序列化
                var commentDtos = await _httpClient.GetFromJsonAsync<List<CommentDto>>(requestUri);
                // 将 DTO 列表转换为前端的 Model 列表
                var comments = new List<Comment>();
                if (commentDtos != null)
                {
                    foreach (var dto in commentDtos)
                    {
                        // TODO: 完善从 CommentDto 到 Comment 模型的映射
                        comments.Add(new Comment
                        {
                            Id = dto.Id,
                            Content = dto.Content,
                            UserName = dto.User?.Nickname,
                            AvatarUrl = dto.User?.AvatarUrl,
                            LikeCount = dto.LikeCount,
                            CreatedAt = dto.CreatedAt
                            // ... 映射 replies
                        });
                    }
                }
                return comments;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CommentService] Failed to get comments: {ex.Message}");
                return new List<Comment>();
            }
        }

        /// <summary>
        /// 发表一条新评论
        /// </summary>
        /// <param name="subjectId">主题ID</param>
        /// <param name="content">评论内容</param>
        /// <returns>发表成功后的评论模型</returns>
        public async Task<Comment> PostCommentAsync(string subjectId, string content)
        {
            if (string.IsNullOrEmpty(subjectId) || string.IsNullOrEmpty(content)) return null;

            // 发表评论需要认证，确保我们有Token
            //if (!_authService.IsLoggedIn)
            //{
            //    Debug.WriteLine("[CommentService] User not logged in. Cannot post comment.");
            //    return null;
            //}

            // 为请求添加认证头
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.Token);

            // 从 subjectId 中解析出 subjectType (e.g., "playlist")
            var subjectType = subjectId.Split('_')[0];

            var createCommentDto = new CreateCommentDto
            {
                SubjectId = subjectId,
                SubjectType = subjectType,
                Content = content
            };

            try
            {
                var requestUri = "api/comments";
                Debug.WriteLine($"[CommentService] Posting comment to: {_httpClient.BaseAddress}{requestUri}");

                // 发送POST请求，请求体为JSON格式的createCommentDto
                var response = await _httpClient.PostAsJsonAsync(requestUri, createCommentDto);

                if (response.IsSuccessStatusCode)
                {
                    // 如果成功，API会返回新创建的评论对象
                    var createdCommentDto = await response.Content.ReadFromJsonAsync<CommentDto>();
                    if (createdCommentDto != null)
                    {
                        // 将返回的DTO转换为前端Model
                        return new Comment
                        {
                            Id = createdCommentDto.Id,
                            Content = createdCommentDto.Content,
                            UserName = createdCommentDto.User?.Nickname,
                            AvatarUrl = createdCommentDto.User?.AvatarUrl,
                            LikeCount = createdCommentDto.LikeCount,
                            //CreatedAt = createdCommentDto.CreatedAt
                        };
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[CommentService] Failed to post comment. Status: {response.StatusCode}, Content: {errorContent}");
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CommentService] An exception occurred while posting comment: {ex.Message}");
                return null;
            }
        }
    }
}
