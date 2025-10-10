using Microsoft.Extensions.Logging;       // 日志记录功能
using Microsoft.Extensions.Options;       // 配置选项处理
using System;
using System.Collections.Concurrent;      // 并发集合（处理多线程安全）
using System.IO;                         // 文件操作
using System.Linq;                       // 集合查询
using System.Net.Http;                   // HTTP请求
using System.Security.Cryptography;       // 加密算法（用于生成缓存键）
using System.Text;                       // 字符串编码
using System.Threading;                  // 线程操作
using System.Threading.Tasks;            // 异步任务

namespace NetEase.Services
{
    /// <summary>
    /// 文件缓存服务，实现ICacheService接口
    /// 功能：通过HTTP下载文件并本地缓存，支持缓存过期清理、大小限制，避免重复下载
    /// </summary>
    public class FileCacheService : ICacheService
    {
        // 日志记录器，用于记录服务运行中的信息、警告和错误
        private readonly ILogger<FileCacheService> _logger;
        // HTTP客户端，用于从网络下载文件
        private readonly HttpClient _httpClient;
        // 缓存配置项（如缓存目录、过期时间、最大大小等）
        private readonly CacheSettings _settings;
        // 缓存文件的根目录路径（根据运行模式动态确定）
        private readonly string _cacheBasePath;

        /// <summary>
        /// 并发字典，跟踪正在进行的下载任务
        /// 作用：避免同一URL被多个线程同时下载（重复请求），提高效率
        /// 键：文件URL，值：下载任务（返回缓存文件路径）
        /// </summary>
        private readonly ConcurrentDictionary<string, Task<string?>> _activeDownloads = new();

        /// <summary>
        /// 构造函数，通过依赖注入获取所需服务
        /// </summary>
        /// <param name="logger">日志服务</param>
        /// <param name="httpClient">HTTP客户端</param>
        /// <param name="settings">缓存配置选项（通过IOptions包装）</param>
        public FileCacheService(
            ILogger<FileCacheService> logger,
            HttpClient httpClient,
            IOptions<CacheSettings> settings)
        {
            _logger = logger;
            _httpClient = httpClient;
            _settings = settings.Value;  // 从配置选项中获取实际的CacheSettings实例

            // 核心逻辑：根据编译模式（Debug/Release）决定缓存根目录
            string rootPath;

#if DEBUG
            // 调试模式：缓存目录位于程序运行目录（如...\bin\Debug\net8.0-windows\）
            // 优点：开发时便于查看和清理缓存；缺点：不适合生产环境（可能被误删或权限不足）
            rootPath = AppDomain.CurrentDomain.BaseDirectory;
            _logger.LogWarning("应用程序处于调试模式，缓存将存储在程序根目录中。这不应用于生产环境！{rootPath}",rootPath);
#else
            // 发布模式：使用系统标准的用户本地应用数据目录
            // 路径通常为：C:\Users\[用户名]\AppData\Local\NetEase\
            // 优点：符合Windows应用规范，权限安全，不易被误删
            rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetEase");
#endif

            // 最终缓存路径 = 根目录 + 配置中的子目录（从CacheSettings获取）
            _cacheBasePath = Path.Combine(rootPath, _settings.CacheDirectory);
        }

        /// <summary>
        /// 初始化缓存服务
        /// 功能：创建缓存目录 + 启动定期清理缓存的后台任务
        /// </summary>
        /// <returns>异步任务</returns>
        public Task InitializeAsync()
        {
            try
            {
                // 确保缓存目录存在（不存在则创建，包括多级目录）
                Directory.CreateDirectory(_cacheBasePath);
                _logger.LogInformation("缓存服务初始化成功，目录: {CachePath}", _cacheBasePath);

                // 启动一个后台任务：每6小时执行一次缓存清理（非阻塞，使用Task.Run避免阻塞初始化）
                _ = Task.Run(async () =>
                {
                    while (true)  // 无限循环，持续运行
                    {
                        // 等待6小时（从配置中获取的清理间隔）
                        await Task.Delay(TimeSpan.FromHours(6));
                        // 执行缓存清理
                        await PruneCacheAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "缓存服务初始化失败。");
            }
            return Task.CompletedTask;  // 初始化完成（即使有异常也返回完成状态）
        }

        /// <summary>
        /// 获取文件（优先从缓存获取，缓存未命中则下载并缓存）
        /// </summary>
        /// <param name="url">文件的网络URL</param>
        /// <param name="cancellationToken">取消令牌（用于终止操作）</param>
        /// <returns>缓存文件的本地路径（失败返回null）</returns>
        public Task<string?> GetFileAsync(string url, CancellationToken cancellationToken = default)
        {
            // 验证URL有效性：为空或格式不正确则直接返回null
            if (string.IsNullOrWhiteSpace(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return Task.FromResult<string?>(null);
            }

            // 核心逻辑：使用ConcurrentDictionary的GetOrAdd方法
            // 作用：如果URL已在下载中（字典中有记录），则直接返回已存在的任务；否则添加新的下载任务
            // 避免同一URL被多个线程重复下载，提高效率并保证线程安全
            return _activeDownloads.GetOrAdd(url, (key) => DownloadAndCacheFileAsync(key, cancellationToken));
        }

        /// <summary>
        /// 下载文件并缓存到本地（私有方法，供GetFileAsync调用）
        /// </summary>
        /// <param name="url">文件的网络URL</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>缓存文件的本地路径（失败返回null）</returns>
        private async Task<string?> DownloadAndCacheFileAsync(string url, CancellationToken cancellationToken)
        {
            string cacheKey = GenerateCacheKey(url);
            string filePath = Path.Combine(_cacheBasePath, cacheKey);
            string tempFilePath = Path.Combine(_cacheBasePath, Guid.NewGuid().ToString() + ".tmp");

            try
            {
                if (File.Exists(filePath))
                {
                    File.SetLastAccessTimeUtc(filePath, DateTime.UtcNow);
                    return filePath;
                }

                _logger.LogDebug("缓存未命中，开始下载: {Url}", url);

                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                {
                    await using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                    {
                        await contentStream.CopyToAsync(fileStream, cancellationToken);
                    }
                }

                // --- 【核心修改】增加重试逻辑 ---
                int retries = 3;
                while (retries > 0)
                {
                    try
                    {
                        // 尝试移动文件
                        File.Move(tempFilePath, filePath, true);
                        _logger.LogInformation("缓存成功: {Url} -> {FilePath}", url, filePath);
                        return filePath; // 成功后立即返回
                    }
                    catch (IOException) // 只捕获IO异常，这通常是文件锁定的信号
                    {
                        retries--;
                        if (retries == 0) throw; // 如果重试耗尽，则重新抛出异常

                        _logger.LogWarning("移动文件时发生IO锁定，将在100ms后重试... 剩余次数: {Retries}", retries);
                        await Task.Delay(100, cancellationToken); // 等待100毫秒
                    }
                }

                // 如果循环结束仍未成功（理论上不会到这里，因为上面会抛异常），返回null
                return null;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("下载被取消: {Url}", url);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "下载或缓存文件时发生错误: {Url}", url);
                return null;
            }
            finally
            {
                // 确保无论如何都尝试清理临时文件（如果它还存在）
                if (File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); }
                    catch (Exception ex) { _logger.LogWarning(ex, "清理临时文件失败: {TempFilePath}", tempFilePath); }
                }
                _activeDownloads.TryRemove(url, out _);
            }
        }

        /// <summary>
        /// 清理缓存（删除过期文件 + 超出最大大小限制时删除最旧文件）
        /// </summary>
        /// <returns>异步任务</returns>
        public Task PruneCacheAsync()
        {
            _logger.LogInformation("开始执行缓存清理...");
            // 在后台线程执行清理（避免阻塞主线程）
            return Task.Run(() =>
            {
                try
                {
                    // 1. 获取缓存目录中所有文件，并按最后访问时间排序（ oldest first ）
                    var files = new DirectoryInfo(_cacheBasePath)
                        .GetFiles()  // 获取所有文件
                        .OrderBy(f => f.LastAccessTimeUtc)  // 按最后访问时间升序（最旧的在前）
                        .ToList();

                    // 2. 计算当前缓存总大小
                    long totalSize = files.Sum(f => f.Length);

                    // 3. 第一步：删除过期文件（超过配置的过期时间）
                    foreach (var file in files.ToList())  // 使用ToList()避免迭代中修改集合
                    {
                        // 检查文件是否过期（当前时间 - 最后访问时间 > 配置的过期时间）
                        if (DateTime.UtcNow - file.LastAccessTimeUtc > _settings.DefaultExpiration)
                        {
                            try
                            {
                                file.Delete();  // 删除文件
                                files.Remove(file);  // 从列表中移除（避免后续重复处理）
                                totalSize -= file.Length;  // 更新总大小
                                _logger.LogDebug("删除过期缓存文件: {FileName}", file.Name);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "删除过期缓存文件失败: {FileName}", file.Name);
                            }
                        }
                    }

                    // 4. 第二步：如果总大小仍超过配置的最大限制，删除最旧的文件直到符合限制
                    if (totalSize > _settings.MaxSizeInBytes)
                    {
                        foreach (var file in files)  // 已按访问时间排序（最旧的在前）
                        {
                            // 若总大小已符合限制，停止删除
                            if (totalSize <= _settings.MaxSizeInBytes) break;

                            try
                            {
                                totalSize -= file.Length;  // 先更新总大小（避免删除失败后数据不一致）
                                file.Delete();
                                _logger.LogDebug("为释放空间删除缓存文件: {FileName}", file.Name);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "为释放空间删除缓存文件失败: {FileName}", file.Name);
                            }
                        }
                    }

                    // 清理完成，记录当前缓存大小（转换为MB显示）
                    _logger.LogInformation("缓存清理完成。当前大小: {CurrentSize} MB", totalSize / 1024 / 1024);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "缓存清理过程中发生严重错误。");
                }
            });
        }

        /// <summary>
        /// 根据URL生成唯一的缓存键（避免URL中的特殊字符影响文件命名）
        /// </summary>
        /// <param name="url">文件的网络URL</param>
        /// <returns>经过加密和字符替换的缓存键</returns>
        private static string GenerateCacheKey(string url)
        {
            // 1. 使用SHA256对URL进行哈希计算（生成固定长度的字节数组，避免长URL导致的文件名过长）
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(url));  // 先将URL转为UTF8字节，再计算哈希

            // 2. 将哈希字节数组转为Base64字符串（便于存储为文件名）
            // 3. 替换Base64中的特殊字符（/和+在文件名中不允许），确保文件名合法
            return Convert.ToBase64String(hashBytes).Replace('/', '_').Replace('+', '-');
        }
    }
}