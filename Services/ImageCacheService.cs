using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace NetEase.Services
{
    public class ImageCacheService
    {
        private readonly HttpClient _httpClient;
        private readonly string _cacheDirectory;

        public ImageCacheService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // 1. 定义缓存目录路径
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _cacheDirectory = Path.Combine(appDataPath, "NetEaseApp", "ImageCache");

            // 2. 确保缓存目录存在
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }

        /// <summary>
        /// 核心方法：根据URL获取本地缓存的图片路径。如果缓存不存在，则下载它。
        /// </summary>
        /// <param name="imageUrl">要加载的图片的网络URL</param>
        /// <returns>可供UI直接使用的本地文件路径，如果失败则返回null</returns>
        public async Task<string> GetImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || !Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                return null; // 无效的URL
            }

            // 3. 根据URL生成缓存文件名 (使用MD5哈希)
            var cacheFileName = GetMd5Hash(imageUrl) + Path.GetExtension(imageUrl);
            var localFilePath = Path.Combine(_cacheDirectory, cacheFileName);

            // 4. 检查本地缓存是否存在
            if (File.Exists(localFilePath))
            {
                // 缓存命中！直接返回本地路径
                return localFilePath;
            }
            else
            {
                // 5. 缓存未命中，从网络下载
                try
                {
                    var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        // 6. 保存到本地缓存
                        await File.WriteAllBytesAsync(localFilePath, imageBytes);
                        return localFilePath; // 返回新创建的本地文件路径
                    }
                }
                catch (Exception ex)
                {
                    // 处理下载失败的情况
                    System.Diagnostics.Debug.WriteLine($"Failed to download image: {imageUrl}, Error: {ex.Message}");
                    return null;
                }
            }

            return null;
        }

        // 辅助方法：计算字符串的MD5哈希值
        private static string GetMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }
    }
}