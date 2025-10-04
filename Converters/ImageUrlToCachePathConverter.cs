using Microsoft.Extensions.DependencyInjection;
using NetEase.Services;
using System.Globalization;
using System.Windows.Data;

namespace NetEase.Converters
{
    public class ImageUrlToCachePathConverter : IValueConverter
    {
        // 懒加载模式获取服务实例
        private static ImageCacheService _cacheService;
        private static ImageCacheService CacheService =>
            _cacheService ??= App.ServiceProvider.GetRequiredService<ImageCacheService>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imageUrl)
            {
                // 注意：转换器是同步的，但我们的服务是异步的。
                // 这是一个常见的WPF MVVM问题，有多种解决方案。
                // 最简单的是 "fire-and-forget" 模式，但这不理想。
                // 更好的方式是使用一个支持异步绑定的库或自定义一个附加属性。

                // 为了演示，我们先用一个简化的同步等待（在UI线程上阻塞，不推荐用于生产）
                // 仅用于理解概念！
                try
                {
                    return CacheService.GetImageAsync(imageUrl).Result;
                }
                catch
                {
                    return null; // 返回null，让FallbackValue生效
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}