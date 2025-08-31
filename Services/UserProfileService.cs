using NetEase.Dtos;
using NetEase.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetEase.Services
{
    public class UserProfileService
    {
        // 定义配置文件的保存路径
        private readonly string _filePath;

        public UserProfileService()
        {
            // 将文件保存在 AppData 文件夹中，这是一个标准做法
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "NetEaseMusicApp");
            Directory.CreateDirectory(appFolder); // 确保文件夹存在
            _filePath = Path.Combine(appFolder, "user_profiles.json");
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
                // existingProfile.AvatarUrl = ... 
            }
            else
            {
                // 添加新用户
                profiles.Add(new SavedUserProfile
                {
                    Name = loginResponse.User.Name,
                    Email = loginResponse.User.Email,
                    AvatarUrl = "CoverImage/26.jpg" // 默认头像
                });
            }

            var json = JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}
