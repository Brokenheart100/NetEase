using NetEase.Dtos;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace NetEase.Services
{
    // 这个类现在非常干净，只负责与认证相关的 API 通信
    public class AuthService
    {
        private string _token;
        public string Token { get; private set; } // 将_token改为公共属性
        // _httpClient 是通过构造函数由 DI 容器注入的
        private readonly HttpClient _httpClient;
        private int? _currentUserId; // 登录后保存用户ID
        private string _currentUserName; // 顺便也保存一下用户名
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
                    if (loginResponse?.User != null && !string.IsNullOrEmpty(loginResponse?.Token))
                    {
                        _token = loginResponse.Token; // <-- 保存Token到字段
                        Token = loginResponse.Token; // 赋值给公共属性
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);
                        _currentUserId = loginResponse.User.Id;
                        _currentUserName = loginResponse.User.Name;

                        Debug.WriteLine($"NetEase User logged in. ID: {_currentUserId}, Name: {_currentUserName}");
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

        public async Task<(bool, LoginResponse)> TryAutoLoginAsync()
        {
            // 在真实应用中，这里会读取本地安全存储的Token
            // 然后调用一个后端API如 /api/auth/validate 来验证Token并获取用户信息
            // 我们暂时简化为：如果没有token就失败
            if (string.IsNullOrEmpty(_token))
            {
                return (false, null);
            }

            // 假设Token有效，但没有用户信息，所以还是返回失败，强制用户手动登录
            // 这是一个待完善的点
            return (false, null);
        }
        public void Logout()
        {
            _token = null;
            _currentUserId = null;
            _currentUserName = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            Debug.WriteLine("User state cleared (logged out).");
        }

        public int? GetCurrentUserId()
        {
            return _currentUserId;
        }

        public string GetCurrentUserName()
        {
            return _currentUserName;
        }

        // 这个是您之前在App.xaml.cs中需要的，这里补充一下
        public void SetCurrentUser(int userId, string userName)
        {
            _currentUserId = userId;
            _currentUserName = userName;
        }
    }
}