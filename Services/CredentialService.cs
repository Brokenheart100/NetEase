using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NetEase.Services
{
    // 用于在JSON中存储凭据的模型
    public class StoredCredential
    {
        public string Email { get; set; }
        // 密码将以Base64字符串的形式存储加密后的byte[]
        public string EncryptedPassword { get; set; }
    }

    public class CredentialService
    {
        private readonly string _filePath;

        // DPAPI需要一个额外的熵（entropy）值，它是一个“盐”，增加了安全性。
        // 这个值需要保存在代码中，即使被反编译，也只有结合当前Windows用户才能解密。
        private static readonly byte[] s_entropy = Encoding.Unicode.GetBytes("NetEaseMusicAppSecretSalt");

        public CredentialService()
        {
            // 【核心修正】: 使用与 UserProfileService 相同的逻辑来确定路径
            _filePath = GetCredentialPath();
            Debug.WriteLine($"Credentials file path set to: {_filePath}");
        }
        private string GetCredentialPath()
        {
#if DEBUG
            // --- 仅在调试模式下执行 ---
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            int levelsUp = 5;
            for (int i = 0; i < levelsUp; i++)
            {
                var parentDir = Directory.GetParent(currentDir);
                if (parentDir != null)
                {
                    currentDir = parentDir.FullName;
                    if (Directory.GetFiles(currentDir, "*.csproj").Any() || Directory.GetFiles(currentDir, "*.sln").Any())
                    {
                        // 找到了项目或解决方案根目录
                        return Path.Combine(currentDir, "credentials.dat");
                    }
                }
                else { break; }
            }
            // 如果没找到，退回到在运行目录下创建
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.dat");
#else
            // --- 在发布 (Release) 模式下执行 ---
            // 依然使用安全的 AppData 文件夹
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "NetEaseMusicApp");
            Directory.CreateDirectory(appFolder);
            return Path.Combine(appFolder, "credentials.dat");
#endif
        }
        // 加密并保存凭据
        public void SaveCredentials(string email, string password)
        {
            try
            {
                var credentialsList = LoadAllStoredCredentials(); // 加载现有列表

                var existingCredential = credentialsList.FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                byte[] passwordBytes = Encoding.Unicode.GetBytes(password);
                byte[] encryptedPasswordBytes = ProtectedData.Protect(passwordBytes, s_entropy, DataProtectionScope.CurrentUser);
                string encryptedPasswordBase64 = Convert.ToBase64String(encryptedPasswordBytes);

                if (existingCredential != null)
                {
                    // 更新密码
                    existingCredential.EncryptedPassword = encryptedPasswordBase64;
                }
                else
                {
                    // 添加新凭据
                    credentialsList.Add(new StoredCredential
                    {
                        Email = email,
                        EncryptedPassword = encryptedPasswordBase64
                    });
                }

                var json = JsonSerializer.Serialize(credentialsList);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save credentials: {ex.Message}");
            }
        }

        // 加载并解密凭据
        public StoredCredential LoadCredentials()
        {
            if (!File.Exists(_filePath)) return null;

            try
            {
                var json = File.ReadAllText(_filePath);
                var storedCredential = JsonSerializer.Deserialize<StoredCredential>(json);

                if (storedCredential == null || string.IsNullOrEmpty(storedCredential.EncryptedPassword))
                {
                    return null;
                }

                // 1. 将Base64字符串转换回加密的byte[]
                byte[] encryptedPasswordBytes = Convert.FromBase64String(storedCredential.EncryptedPassword);

                // 2. 使用DPAPI进行解密
                byte[] passwordBytes = ProtectedData.Unprotect(encryptedPasswordBytes, s_entropy, DataProtectionScope.CurrentUser);

                // 3. 将解密后的byte[]转换回明文密码字符串
                string password = Encoding.Unicode.GetString(passwordBytes);

                // 只返回包含了明文密码的临时对象，不要修改并存回EncryptedPassword字段
                return new StoredCredential { Email = storedCredential.Email, EncryptedPassword = password };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load or decrypt credentials: {ex.Message}");
                // 解密失败（比如换了Windows用户），删除无效的凭据文件
                //ClearCredentials();
                return null;
            }
        }
        // 【新增】根据Email查找并解密密码
        public string GetPasswordForEmail(string email)
        {
            Debug.WriteLine($"Enter GetPasswordForEmail({email})");
            if (string.IsNullOrEmpty(email) || !File.Exists(_filePath))
            {
                return null;
            }
            try
            {
                var json = File.ReadAllText(_filePath);
                // 注意：我们的JSON文件现在是一个用户列表
                var credentialsList = JsonSerializer.Deserialize<List<StoredCredential>>(json);

                // 查找匹配的凭据
                var storedCredential = credentialsList?.FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                if (storedCredential == null || string.IsNullOrEmpty(storedCredential.EncryptedPassword))
                {
                    return null;
                }

                byte[] encryptedPasswordBytes = Convert.FromBase64String(storedCredential.EncryptedPassword);
                byte[] passwordBytes = ProtectedData.Unprotect(encryptedPasswordBytes, s_entropy, DataProtectionScope.CurrentUser);
                return Encoding.Unicode.GetString(passwordBytes);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get password for {email}: {ex.Message}");
                return null;
            }
        }

        private List<StoredCredential> LoadAllStoredCredentials()
        {
            if (!File.Exists(_filePath)) return new List<StoredCredential>();
            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<StoredCredential>>(json) ?? new List<StoredCredential>();
            }
            catch { return new List<StoredCredential>(); }
        }
        // 清除已保存的凭据
        public void ClearCredentials()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}
