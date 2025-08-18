using NetEase.Dtos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Services
{
    public class PlaylistService
    {
        private readonly HttpClient _httpClient;

        // 它也从构造函数接收 HttpClient
        public PlaylistService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<PlaylistSummaryDto>> GetMyPlaylistsAsync()
        {
            try
            {
                // 发送 GET 请求到受保护的端点 /api/playlists/my
                var playlists = await _httpClient.GetFromJsonAsync<List<PlaylistSummaryDto>>("api/playlists/my");
                return playlists ?? new List<PlaylistSummaryDto>();
            }
            catch (HttpRequestException ex)
            {
                // 如果请求失败 (例如 401 Unauthorized 或网络错误)，记录日志并返回空列表
                Debug.WriteLine($"Failed to get playlists: {ex.Message}");
                return new List<PlaylistSummaryDto>();
            }
        }

        // 未来可以在这里添加 GetPlaylistDetailAsync, CreatePlaylistAsync 等方法
    }
}
