using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using NetEase.Dtos;
using NetEase.Dtos.NetEase.Dtos;

namespace NetEase.Services
{
    public class SongService
    {
        private readonly HttpClient _httpClient;

        public SongService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SongDto>> GetAllSongsAsync()
        {
            try
            {
                // 通过API网关调用Catalog服务的 /api/songs 端点
                return await _httpClient.GetFromJsonAsync<List<SongDto>>("api/songs");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get all songs: {ex.Message}");
                return []; // 失败时返回空列表
            }
        }
    }
}
