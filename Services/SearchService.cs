using NetEase.Dtos;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;

namespace NetEase.Services
{
    public class SearchService
    {
        private readonly HttpClient _httpClient;

        // 构造函数接收一个已经配置好 BaseAddress 的 HttpClient
        public SearchService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SearchResponseDto> SearchAsync(string query, string type = "song", int page = 1, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }

            try
            {
                // 构建请求的相对URL，会自动与BaseAddress拼接
                // 例如: "https://localhost:7268" + "/api/v1/search?q=..."
                var requestUri = $"api/v1/search?q={Uri.EscapeDataString(query)}&type={type}&page={page}&pageSize={pageSize}";
                //var requestUri = $"api/v1/search?q={Uri.EscapeDataString(query)}";
                Debug.WriteLine($"Sending search request to: {_httpClient.BaseAddress}{requestUri}");

                // GetFromJsonAsync 会自动发送GET请求，并将返回的JSON反序列化为 SearchResponseDto 对象
                var result = await _httpClient.GetFromJsonAsync<SearchResponseDto>(requestUri);
                return result;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[SearchService] HTTP request failed: {ex.Message}");
                // 在这里可以处理网络错误、服务器错误等
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SearchService] An unexpected error occurred: {ex.Message}");
                return null;
            }
        }
    }
}
