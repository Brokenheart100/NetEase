using NetEase.Dtos;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

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
            Debug.WriteLine($"{loginData.Email},{loginData.Password}");
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginData);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (!string.IsNullOrEmpty(loginResponse?.Token))
                    {
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
       
    }
}