using System;
using System.Collections.Generic;
using System.Text;

namespace NetEase.Services
{
    public interface ICacheService
    {
        /// <summary>
        /// 异步获取一个远程URL对应的本地缓存文件路径。
        /// 如果文件已缓存，则直接返回路径；否则，下载、缓存并返回路径。
        /// </summary>
        /// <param name="url">远程资源的URL</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>成功则返回本地文件绝对路径，失败则返回null。</returns>
        Task<string?> GetFileAsync(string url, CancellationToken cancellationToken = default);

        /// <summary>
        /// 初始化服务，例如创建目录和启动后台清理任务。
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// 手动触发一次缓存清理。
        /// </summary>
        Task PruneCacheAsync();
    }
}
