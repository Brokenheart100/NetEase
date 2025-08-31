using NetEase.Models;
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
    public class FriendService
    {
        private readonly HttpClient _httpClient;

        public FriendService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Friend>> GetFriendsAsync()
        {
            try
            {
                // 假设 DTO 和 Model 结构相似，可以直接反序列化
                return await _httpClient.GetFromJsonAsync<List<Friend>>("api/friends");
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HTTP请求失败: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"内部异常: {ex.InnerException.Message}");
                }
                return new List<Friend>();
            }
        }
    }
}
