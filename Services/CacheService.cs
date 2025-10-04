using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace NetEase.Services
{
    public enum CacheType
    {
        Image,
        Song
    }

    public class CacheService
    {
        private readonly HttpClient _httpClient;
        private readonly string _imageCacheDirectory;
        private readonly string _songCacheDirectory;

        public CacheService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // 1. 定义缓存根目录
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string baseCacheDirectory = Path.Combine(appDataPath, "NetEaseApp", "Cache");

            // 2. 创建不同类型的子目录
            _imageCacheDirectory = Path.Combine(baseCacheDirectory, "Images");
            _songCacheDirectory = Path.Combine(baseCacheDirectory, "Songs");

            Directory.CreateDirectory(_imageCacheDirectory);
            Directory.CreateDirectory(_songCacheDirectory);
        }

        /// <summary>
        /// 核心公共方法：根据网络URL获取资源的本地文件路径。
        /// 如果缓存存在，则直接返回路径；如果不存在，则先下载再返回。
        /// </summary>
        /// <param name="url">资源的完整网络URL</param>
        /// <param name="cacheType">要使用的缓存类型（图片或歌曲）</param>
        /// <returns>一个包含本地文件路径的Task，如果失败则Task结果为null</returns>
        public async Task<string> GetFileAsync(string url, CacheType cacheType)
        {
            if (string.IsNullOrEmpty(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }

            string targetDirectory = cacheType == CacheType.Image ? _imageCacheDirectory : _songCacheDirectory;
            string localFilePath = GetLocalPathFromUrl(url, targetDirectory);

            // 检查本地缓存是否存在且文件大小 > 0 (防止空文件)
            if (File.Exists(localFilePath) && new FileInfo(localFilePath).Length > 0)
            {
                // 缓存命中！
                return localFilePath;
            }

            // 缓存未命中，从网络下载
            try
            {
                // 使用 GetAsync 和流式写入，对大文件更友好
                using HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                await using Stream contentStream = await response.Content.ReadAsStreamAsync();
                await using FileStream fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                await contentStream.CopyToAsync(fileStream);

                return localFilePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CacheService] Failed to download file from {url}. Error: {ex.Message}");
                // 如果下载失败，尝试删除可能已创建的不完整文件
                if (File.Exists(localFilePath))
                {
                    File.Delete(localFilePath);
                }
                return null;
            }
        }

        /// <summary>
        /// 根据URL和目标目录，生成本地缓存文件的完整物理路径。
        /// </summary>
        private string GetLocalPathFromUrl(string url, string targetDirectory)
        {
            // 使用URL的MD5哈希值作为主文件名，保留原始扩展名
            var fileName = GetMd5Hash(url) + Path.GetExtension(url);
            return Path.Combine(targetDirectory, fileName);
        }

        private static string GetMd5Hash(string input)
        {
            using MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
