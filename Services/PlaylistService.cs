// 引入数据传输对象(DTO)命名空间，用于接收API返回的数据
using NetEase.Dtos;
using System;
using System.Collections.Generic;
using System.Diagnostics;  // 用于调试输出
using System.Linq;
using System.Net.Http;     // 用于HTTP请求
using System.Net.Http.Json; // 提供HTTP客户端的JSON序列化/反序列化扩展方法
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Services
{
    /// <summary>
    /// 播放列表服务类，负责与后端API交互，处理播放列表相关的网络请求
    /// </summary>
    public class PlaylistService
    {
        // HTTP客户端实例，用于发送网络请求到后端API
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 构造函数，通过依赖注入获取HttpClient实例
        /// </summary>
        /// <param name="httpClient">用于发送HTTP请求的客户端</param>
        public PlaylistService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// 异步获取当前登录用户的播放列表摘要信息
        /// </summary>
        /// <returns>包含播放列表摘要的列表，如果请求失败则返回空列表</returns>
        public async Task<List<PlaylistSummaryDto>> GetMyPlaylistsAsync()
        {
            try
            {
                // 发送GET请求到后端API的"api/playlists/my"端点
                // GetFromJsonAsync<T>：自动将API返回的JSON响应反序列化为List<PlaylistSummaryDto>类型
                var playlists = await _httpClient.GetFromJsonAsync<List<PlaylistSummaryDto>>("api/playlists/my");

                // 处理API返回null的情况：如果为null则返回空列表，避免后续出现空引用异常
                return playlists ?? new List<PlaylistSummaryDto>();
            }
            catch (HttpRequestException ex)
            {
                // 捕获HTTP请求相关异常（如网络错误、401未授权、500服务器错误等）
                // 输出错误信息到调试窗口，方便开发时排查问题
                Debug.WriteLine($"获取播放列表失败: {ex.Message}");

                // 失败时返回空列表，保证调用方不会收到异常，便于前端友好处理
                return new List<PlaylistSummaryDto>();
            }
        }

        /// <summary>
        /// 异步获取指定ID的播放列表详细信息
        /// </summary>
        /// <param name="playlistId">要查询的播放列表ID</param>
        /// <returns>播放列表详情对象，如果请求失败则返回null</returns>
        public async Task<PlaylistDetailDto> GetPlaylistDetailAsync(int playlistId)
        {
            try
            {
                // 发送GET请求到"api/playlists/{playlistId}"端点
                // 使用字符串插值动态生成带参数的URL（例如：api/playlists/123）
                return await _httpClient.GetFromJsonAsync<PlaylistDetailDto>($"api/playlists/{playlistId}");
            }
            catch (HttpRequestException ex)
            {
                // 捕获并记录HTTP请求异常
                Debug.WriteLine($"获取播放列表详情失败: {ex.Message}");

                // 失败时返回null，由调用方处理空值情况
                return null;
            }
        }

        public async Task<PlaylistSummaryDto> CreatePlaylistAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var createDto = new { Name = name }; // 后端 CreatePlaylistDto 只需要 Name

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/playlists", createDto);

                if (response.IsSuccessStatusCode)
                {
                    // 如果成功，后端会返回新创建的播放列表对象
                    return await response.Content.ReadFromJsonAsync<PlaylistSummaryDto>();
                }

                Debug.WriteLine($"Failed to create playlist. Status: {response.StatusCode}");
                return null;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error creating playlist: {ex.Message}");
                return null;
            }
        }
    }
    // 预留：未来可以添加更多方法，如创建播放列表、添加歌曲到播放列表等
    // 例如：
    // public async Task<PlaylistDetailDto> CreatePlaylistAsync(CreatePlaylistDto dto)
    // public async Task<bool> AddSongToPlaylistAsync(int playlistId, int songId)
}
