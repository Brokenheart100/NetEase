using Microsoft.Extensions.Logging;
using NetEase.Dtos;
using NetEase.Shared.Clients.Dtos;
using System; // <-- 引入，用于 Exception
using System.Collections.Generic; // <-- 引入
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace NetEase.Services
{
    public class PlaylistService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PlaylistService> _logger;

        public PlaylistService(HttpClient httpClient, ILogger<PlaylistService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> UpdatePlaylistAsync(int playlistId, UpdatePlaylistDto dto)
        {
            _logger.LogInformation(message: "正在更新播放列表 {PlaylistId}...", playlistId);
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/playlists/{playlistId}", dto);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("成功更新播放列表 {PlaylistId}。", playlistId);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("更新播放列表 {PlaylistId} 失败。状态码: {StatusCode}, 响应: {Response}",
                        playlistId, response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新播放列表 {PlaylistId} 时发生网络异常。", playlistId);
                return false;
            }
        }

        public async Task<string?> UploadCoverAsync(string localFilePath)
        {
            _logger.LogInformation("准备上传封面文件: '{LocalFilePath}'", localFilePath);
            if (!File.Exists(localFilePath))
            {
                _logger.LogError("上传封面失败：本地文件不存在 '{LocalFilePath}'。", localFilePath);
                return null;
            }

            var requestUri = "api/files/upload/covers";
            using var multipartFormContent = new MultipartFormDataContent();
            using var fileStreamContent = new StreamContent(File.OpenRead(localFilePath));
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // 可根据文件类型动态设置

            multipartFormContent.Add(fileStreamContent, name: "file", fileName: Path.GetFileName(localFilePath));

            try
            {
                _logger.LogInformation("正在向 '{RequestUri}' POST 封面文件...", requestUri);
                var response = await _httpClient.PostAsync(requestUri, multipartFormContent);

                if (response.IsSuccessStatusCode)
                {
                    var uploadResult = await response.Content.ReadFromJsonAsync<FileUploadResponseDto>();
                    _logger.LogInformation("封面上传成功，File.API返回相对路径: '{RelativePath}'", uploadResult?.RelativePath);
                    return uploadResult?.RelativePath;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("封面上传失败。状态码: {StatusCode}, 响应: {Response}",
                        response.StatusCode, errorContent);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传封面时发生网络异常。");
                return null;
            }
        }

        public async Task<bool> AddToFavoritesAsync(int songId)
        {
            _logger.LogInformation("正在尝试将歌曲 {SongId} 添加到“我喜欢的音乐”...", songId);
            try
            {
                var response = await _httpClient.PostAsync($"api/playlists/favorites/{songId}", null);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("成功将歌曲 {SongId} 添加到“我喜欢的音乐”。", songId);
                    return true;
                }
                _logger.LogWarning("将歌曲 {SongId} 添加到“我喜欢的音乐”失败。状态码: {StatusCode}", songId, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "将歌曲 {SongId} 添加到“我喜欢的音乐”时发生网络异常。", songId);
                return false;
            }
        }

        public async Task<bool> RemoveFromFavoritesAsync(int songId)
        {
            _logger.LogInformation("正在尝试从“我喜欢的音乐”中移除歌曲 {SongId}...", songId);
            try
            {
                var response = await _httpClient.DeleteAsync($"api/playlists/favorites/{songId}");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("成功从“我喜欢的音乐”中移除歌曲 {SongId}。", songId);
                    return true;
                }
                _logger.LogWarning("从“我喜欢的音乐”中移除歌曲 {SongId} 失败。状态码: {StatusCode}", songId, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从“我喜欢的音乐”中移除歌曲 {SongId} 时发生网络异常。", songId);
                return false;
            }
        }

        public async Task<List<PlaylistSummaryDto>> GetMyPlaylistsAsync()
        {
            _logger.LogInformation("正在获取当前用户的播放列表摘要...");
            try
            {
                var playlists = await _httpClient.GetFromJsonAsync<List<PlaylistSummaryDto>>("api/playlists/my");
                if (playlists == null)
                {
                    _logger.LogWarning("获取播放列表摘要返回了 null。");
                    return new List<PlaylistSummaryDto>();
                }
                _logger.LogInformation("成功获取到 {PlaylistCount} 个播放列表摘要。", playlists.Count);
                return playlists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 'api/playlists/my' 时发生网络异常。");
                return new List<PlaylistSummaryDto>();
            }
        }

        public async Task<PlaylistDetailDto?> GetPlaylistDetailAsync(int playlistId)
        {
            _logger.LogInformation("正在获取播放列表 {PlaylistId} 的详情...", playlistId);
            try
            {
                var playlistDetail = await _httpClient.GetFromJsonAsync<PlaylistDetailDto>($"api/playlists/{playlistId}");
                if (playlistDetail == null)
                {
                    _logger.LogWarning("获取播放列表 {PlaylistId} 详情返回了 null。", playlistId);
                }
                else
                {
                    _logger.LogInformation("成功获取播放列表 {PlaylistId} 的详情，包含 {SongCount} 首歌曲。", playlistId, playlistDetail.Songs?.Count ?? 0);
                }
                return playlistDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取播放列表 {PlaylistId} 详情时发生网络异常。", playlistId);
                return null;
            }
        }

        public async Task<PlaylistSummaryDto?> CreatePlaylistAsync(string name, bool isPrivate = false)
        {
            _logger.LogInformation("正在创建新播放列表，名称: '{PlaylistName}', 是否私密: {IsPrivate}", name, isPrivate);
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("创建播放列表失败：名称为空。");
                return null;
            }

            var createDto = new { Name = name, IsPrivate = isPrivate };
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/playlists", createDto);
                if (response.IsSuccessStatusCode)
                {
                    var newPlaylist = await response.Content.ReadFromJsonAsync<PlaylistSummaryDto>();
                    _logger.LogInformation("播放列表创建成功，新ID: {PlaylistId}", newPlaylist?.Id);
                    return newPlaylist;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("创建播放列表失败。状态码: {StatusCode}, 响应: {Response}",
                    response.StatusCode, errorContent);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建播放列表时发生网络异常。");
                return null;
            }
        }

        public async Task<(bool Success, string ErrorMessage)> AddSongToPlaylistAsync(int playlistId, int songId)
        {
            _logger.LogInformation("正在尝试将歌曲 {SongId} 添加到播放列表 {PlaylistId}...", songId, playlistId);
            try
            {
                var response = await _httpClient.PostAsync($"api/playlists/{playlistId}/songs/{songId}", null);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("成功将歌曲 {SongId} 添加到播放列表 {PlaylistId}。", songId, playlistId);
                    return (true, string.Empty);
                }
                else
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(); // 假设有这样一个DTO
                    _logger.LogWarning("添加歌曲到播放列表失败。状态码: {StatusCode}, 错误信息: {ErrorMessage}",
                        response.StatusCode, errorContent?.Message);
                    return (false, errorContent?.Message ?? "未知错误");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加歌曲 {SongId} 到播放列表 {PlaylistId} 时发生网络异常。", songId, playlistId);
                return (false, "无法连接到服务器。");
            }
        }

        public async Task<bool> RemoveSongFromPlaylistAsync(int playlistId, int songId)
        {
            _logger.LogInformation("正在尝试从播放列表 {PlaylistId} 中移除歌曲 {SongId}...", playlistId, songId);
            try
            {
                var response = await _httpClient.DeleteAsync($"api/playlists/{playlistId}/songs/{songId}");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("成功从播放列表 {PlaylistId} 中移除歌曲 {SongId}。", playlistId, songId);
                    return true;
                }
                _logger.LogWarning("从播放列表 {PlaylistId} 中移除歌曲 {SongId} 失败。状态码: {StatusCode}",
                    playlistId, songId, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从播放列表 {PlaylistId} 中移除歌曲 {SongId} 时发生网络异常。", playlistId, songId);
                return false;
            }
        }

        public async Task<List<Dtos.SongDto>?> GetAllSongsAsync()
        {
            _logger.LogInformation("正在从后端获取所有歌曲列表...");
            try
            {
                var songs = await _httpClient.GetFromJsonAsync<List<Dtos.SongDto>>("api/songs");
                _logger.LogInformation("成功获取到 {SongCount} 首歌曲。", songs?.Count ?? 0);
                return songs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从 'api/songs' 获取所有歌曲时失败。");
                return null;
            }
        }
    }

    // (推荐) 在共享的 DTOs 项目中定义一个通用的错误响应模型
    public class ErrorResponseDto
    {
        public string Message { get; set; }
    }
}