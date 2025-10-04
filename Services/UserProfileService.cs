using NetEase.Dtos;
using NetEase.Models;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using static NetEase.Converters.RandomNumber;

namespace NetEase.Services
{
    public class AppState
    {
        public int? LastActiveSessionId { get; set; }
        // 以后还可以添加音量、播放模式等
    }
    public class UserProfileService
    {
        // 定义配置文件的保存路径
        private readonly string _filePath;
        private readonly string _profileFilePath;
        private readonly string _stateFilePath; // <-- 新增状态文件路径
        private readonly HttpClient _httpClient;
        public UserProfileService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _filePath = GetProfilePath();
            Debug.WriteLine($"User profiles path set to: {_filePath}");

            string rootPath = GetAppRootPath(); // 抽取出获取根路径的逻辑
            _profileFilePath = Path.Combine(rootPath, "user_profiles.json");
            _stateFilePath = Path.Combine(rootPath, "app_state.json");
            Debug.WriteLine($"User profiles path: {_profileFilePath}");
            Debug.WriteLine($"App state path: {_stateFilePath}");
        }
        private string GetAppRootPath()
        {
#if DEBUG
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            for (int i = 0; i < 5; i++)
            {
                var parentDir = Directory.GetParent(currentDir);
                if (parentDir != null)
                {
                    currentDir = parentDir.FullName;
                    if (Directory.GetFiles(currentDir, "*.csproj").Any() || Directory.GetFiles(currentDir, "*.sln").Any())
                    {
                        return currentDir;
                    }
                }
                else break;
            }
            return AppDomain.CurrentDomain.BaseDirectory;
#else
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "NetEaseMusicApp");
            Directory.CreateDirectory(appFolder);
            return appFolder;
#endif
        }
        private string GetProfilePath()
        {
#if DEBUG
            // --- 仅在调试模式下执行 ---
            // 目标：找到项目根目录 (包含 .csproj 文件的目录)
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            // 从 bin/Debug/... 目录向上查找
            // 通常需要向上查找3到5层，取决于您的项目结构和.NET版本
            int levelsUp = 5;
            for (int i = 0; i < levelsUp; i++)
            {
                var parentDir = Directory.GetParent(currentDir);
                if (parentDir != null)
                {
                    currentDir = parentDir.FullName;
                    // 检查当前目录是否是项目根目录（通过查找 .sln 或 .csproj 文件）
                    if (Directory.GetFiles(currentDir, "*.csproj").Any() || Directory.GetFiles(currentDir, "*.sln").Any())
                    {
                        // 找到了！返回根目录下的文件路径
                        return Path.Combine(currentDir, "user_profiles.json");
                    }
                }
                else
                {
                    break; // 到达了磁盘根目录，停止查找
                }
            }
            // 如果没找到，退回到在运行目录下创建
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user_profiles.json");
#else
            // --- 在发布 (Release) 模式下执行 ---
            // 使用 AppData 文件夹，这是生产环境的最佳实践
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "NetEaseMusicApp");
            Directory.CreateDirectory(appFolder);
            return Path.Combine(appFolder, "user_profiles.json");
#endif
        }
        public void SaveLastActiveSession(int? sessionId)
        {
            try
            {
                var state = new AppState { LastActiveSessionId = sessionId };
                var json = JsonSerializer.Serialize(state);
                File.WriteAllText(_stateFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save app state: {ex.Message}");
            }
        }
        public AppState LoadAppState()
        {
            if (!File.Exists(_stateFilePath))
            {
                return new AppState(); // 返回一个默认状态
            }
            try
            {
                var json = File.ReadAllText(_stateFilePath);
                return JsonSerializer.Deserialize<AppState>(json) ?? new AppState();
            }
            catch
            {
                return new AppState();
            }
        }
        // 从JSON文件加载所有已保存的用户
        public List<SavedUserProfile> LoadProfiles()
        {
            if (!File.Exists(_filePath))
            {
                return new List<SavedUserProfile>();
            }
            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<SavedUserProfile>>(json) ?? new List<SavedUserProfile>();
            }
            catch
            {
                return new List<SavedUserProfile>(); // 文件损坏则返回空列表
            }
        }

        // 保存一个新登录的用户（如果已存在则更新）
        public void SaveProfile(LoginResponse loginResponse)
        {
            if (loginResponse?.User == null) return;

            var profiles = LoadProfiles();

            // 查找是否已存在该用户
            var existingProfile = profiles.FirstOrDefault(p => p.Email.Equals(loginResponse.User.Email, StringComparison.OrdinalIgnoreCase));

            if (existingProfile != null)
            {
                // 更新信息
                existingProfile.Name = loginResponse.User.Name;
            }
            else
            {
                // 添加新用户
                profiles.Add(new SavedUserProfile
                {
                    Name = loginResponse.User.Name,
                    Email = loginResponse.User.Email,
                    AvatarUrl = GetRandomAvatarUrl() // 默认头像
                });
            }

            var json = JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_profileFilePath, json);
        }
        /// <summary>
        /// 上传用户头像到后端API
        /// </summary>
        /// <param name="localFilePath">用户在本地选择的图片文件路径</param>
        /// <returns>上传成功后，由服务器返回的新的头像URL</returns>
        public async Task<string> UploadAvatarAsync(string localFilePath)
        {
            if (!File.Exists(localFilePath))
            {
                return null;
            }

            // 【重要】替换为您的头像上传API的实际地址
            // 这个地址应该指向您的 Auth.API 或一个专门的用户服务
            var requestUri = "/api/users/avatar";

            // 使用 multipart/form-data 来构建请求体
            using var multipartFormContent = new MultipartFormDataContent();

            // 1. 读取文件内容
            var fileStreamContent = new StreamContent(File.OpenRead(localFilePath));

            // 2. 设置文件的 MIME 类型
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png"); // 可以根据文件扩展名动态设置

            // 3. 将文件内容添加到表单中
            // "avatarFile" 是后端API期望接收的表单字段名
            // Path.GetFileName(localFilePath) 是上传到服务器的文件名
            multipartFormContent.Add(fileStreamContent, name: "avatarFile", fileName: Path.GetFileName(localFilePath));

            // (可选) 如果API需要其他参数，也可以添加
            // multipartFormContent.Add(new StringContent("some_value"), name: "some_key");

            try
            {
                // 4. 发送 POST 请求
                var response = await _httpClient.PostAsync(requestUri, multipartFormContent);
                response.EnsureSuccessStatusCode(); // 如果响应不是2xx，则抛出异常

                // 5. 解析服务器返回的JSON，获取新的URL
                var responseBody = await response.Content.ReadAsStringAsync();

                // 假设后端返回一个JSON对象，如: { "avatarUrl": "https://.../new_avatar.jpg" }
                using var jsonDoc = JsonDocument.Parse(responseBody);
                return jsonDoc.RootElement.GetProperty("avatarUrl").GetString();
            }
            catch (HttpRequestException ex)
            {
                // 处理网络或API错误
                System.Diagnostics.Debug.WriteLine($"Avatar upload failed: {ex.Message}");
                return null;
            }
        }
    }
}
