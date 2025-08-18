using NetEase.Dtos;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetEase.Services
{
    // 这个类现在非常干净，只负责与认证相关的 API 通信
    public class AuthService
    {
        // _httpClient 是通过构造函数由 DI 容器注入的
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, string ErrorMessage)> RegisterAsync(string name, string mobile, string email, string password)
        {
            var registerData = new { Name = name, MobileNumber = mobile, Email = email, Password = password };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerData);

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                else
                {
                    // 错误处理逻辑保持不变，但可以做得更健壮
                    return (false, await ParseErrorResponse(response));
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"API request failed: {ex.Message}");
                return (false, "Could not connect to the server.");
            }
        }

        public async Task<(bool Success, LoginResponse Response, string ErrorMessage)> LoginAsync(string email, string password)
        {
            var loginData = new { Email = email, Password = password };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginData);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (!string.IsNullOrEmpty(loginResponse?.Token))
                    {
                        // 关键修改：直接更新注入的 HttpClient 实例的默认请求头。
                        // 这个 HttpClient 实例是在 App.xaml.cs 中注册为单例的，
                        // 所以这个设置将对整个应用程序的后续请求生效。
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);
                    }
                    return (true, loginResponse, null);
                }
                else
                {
                    return (false, null, await ParseErrorResponse(response));
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"API request failed: {ex.Message}");
                return (false, null, "Could not connect to the server.");
            }
        }

        /// <summary>
        /// 辅助方法，用于从失败的 HttpResponseMessage 中解析错误信息。
        /// </summary>
        private async Task<string> ParseErrorResponse(HttpResponseMessage response)
        {
            string errorMessage = $"An error occurred. Status code: {response.StatusCode}";
            if (response.Content != null)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (!string.IsNullOrEmpty(errorResponse?.Message))
                    {
                        errorMessage = errorResponse.Message;
                    }
                }
                catch { /* 忽略反序列化失败，使用默认消息 */ }
            }
            return errorMessage;
        }
        public async Task<List<PlaylistSummaryDto>> GetMyPlaylistsAsync()
        {
            try
            {
                // 我们先获取 HttpResponseMessage，以便可以检查状态码
                var response = await _httpClient.GetAsync("api/playlists/my");

                if (response.IsSuccessStatusCode)
                {
                    // 如果成功 (2xx)，则反序列化内容
                    var playlists = await response.Content.ReadFromJsonAsync<List<PlaylistSummaryDto>>();
                    return playlists ?? new List<PlaylistSummaryDto>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // 如果是 401 未授权，说明 Token 无效或已过期
                    Debug.WriteLine("Unauthorized access to get playlists. Token might be invalid or expired.");
                    // 在真实应用中，这里可能会触发一个全局事件，要求用户重新登录
                }
                else
                {
                    // 其他 HTTP 错误
                    Debug.WriteLine($"Failed to get playlists. Status code: {response.StatusCode}");
                }

                // 对于所有失败情况，都返回一个空列表
                return new List<PlaylistSummaryDto>();
            }
            catch (HttpRequestException ex)
            {
                // 网络连接层面的错误
                Debug.WriteLine($"Network error while getting playlists: {ex.Message}");
                return new List<PlaylistSummaryDto>();
            }
        }
    }
}