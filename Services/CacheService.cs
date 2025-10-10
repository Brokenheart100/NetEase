using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CacheService> _logger;
        public CacheService(HttpClient httpClient, ILogger<CacheService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // 1. 定义缓存根目录
            string baseCacheDirectory = GetBaseCachePath();

            // 2. 创建不同类型的子目录
            _imageCacheDirectory = Path.Combine(baseCacheDirectory, "Images");
            _songCacheDirectory = Path.Combine(baseCacheDirectory, "Songs");

            Directory.CreateDirectory(_imageCacheDirectory);
            Directory.CreateDirectory(_songCacheDirectory);

            _logger.LogInformation("CacheService initialized. Image cache: {ImagePath}, Song cache: {SongPath}",
                _imageCacheDirectory, _songCacheDirectory);
        }
        private string GetBaseCachePath()
        {
#if DEBUG
            // --- 在调试 (DEBUG) 模式下 ---
            // 目标：找到解决方案根目录 (包含 .sln 文件的目录)
            string? currentDir = AppDomain.CurrentDomain.BaseDirectory;
            // 循环向上查找，直到找到 .sln 文件
            while (currentDir != null && Directory.GetFiles(currentDir, "*.sln").Length == 0)
            {
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            // 如果找到了解决方案目录，就在它下面创建 .cache 文件夹
            // 如果没找到，就使用当前工作目录
            var rootPath = currentDir ?? Directory.GetCurrentDirectory();
            return Path.Combine(rootPath, ".cache"); // 使用 .cache 文件夹，符合现代工具的习惯

#else
            // --- 在发布 (RELEASE) 模式下 ---
            // 使用 AppData\Local，这是生产环境的最佳实践
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appDataPath, "NetEaseApp", "Cache");
#endif
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
                _logger.LogWarning("Invalid or empty URL provided to GetFileAsync: {Url}", url);
                return null;
            }

            string targetDirectory = cacheType == CacheType.Image ? _imageCacheDirectory : _songCacheDirectory;
            string localFilePath = GetLocalPathFromUrl(url, targetDirectory);

            // ======================= 4. 添加详细的日志打印 =======================

            if (File.Exists(localFilePath) && new FileInfo(localFilePath).Length > 0)
            {
                _logger.LogInformation("Cache HIT for URL '{Url}'. Using local file: '{FilePath}'", url, localFilePath);
                return localFilePath;
            }

            _logger.LogInformation("Cache MISS for URL '{Url}'. Starting download...", url);

            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                _logger.LogDebug("Successfully connected to download URL '{Url}'. Status: {StatusCode}", url, response.StatusCode);

                await using Stream contentStream = await response.Content.ReadAsStreamAsync();
                await using FileStream fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                await contentStream.CopyToAsync(fileStream);

                _logger.LogInformation("Successfully downloaded and cached file for URL '{Url}' to '{FilePath}'", url, localFilePath);

                return localFilePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CacheService] Failed to download and cache file from '{Url}'.", url);

                if (File.Exists(localFilePath))
                {
                    try
                    {
                        File.Delete(localFilePath);
                        _logger.LogWarning("Deleted partially downloaded file: '{FilePath}'", localFilePath);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogError(deleteEx, "Failed to delete partially downloaded file: '{FilePath}'", localFilePath);
                    }
                }
                return null;
            }
            // ====================================================================
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
