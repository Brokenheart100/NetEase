using System;
using System.Collections.Generic;
using System.Text;

namespace NetEase.Services
{
    public class CacheSettings
    {
        public string CacheDirectory { get; set; } = "Cache/Images"; // 默认缓存子目录
        public long MaxSizeInBytes { get; set; } = 1024 * 1024 * 200; // 默认200MB
        public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromDays(1); // 默认30天过期
    }
}
