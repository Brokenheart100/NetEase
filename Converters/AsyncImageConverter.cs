using Microsoft.Extensions.DependencyInjection;
using NetEase.Services;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace NetEase.Converters
{
    public class AsyncImageConverter : IValueConverter
    {
        private static CacheService _cacheService;
        private static CacheService CacheService =>
            _cacheService ??= App.ServiceProvider.GetRequiredService<CacheService>();

        // 默认图片，当加载失败或URL为空时显示
        private static readonly BitmapImage DefaultImage =
            new(new Uri("/CoverImage/2.jpg", UriKind.Relative));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string imageUrl || string.IsNullOrEmpty(imageUrl))
            {
                return DefaultImage;
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            // 先设置为默认图片，避免UI空白
            bitmap.UriSource = DefaultImage.UriSource;
            bitmap.EndInit();

            // 在后台异步加载真正的图片
            _ = LoadImageAsync(bitmap, imageUrl);

            return bitmap;
        }

        private async Task LoadImageAsync(BitmapImage image, string url)
        {
            string localPath = await CacheService.GetFileAsync(url, CacheType.Image);

            if (localPath != null)
            {
                // 在UI线程上更新图片源
                App.Current.Dispatcher.Invoke(() =>
                {
                    image.BeginInit();
                    image.UriSource = new Uri(localPath);
                    image.CacheOption = BitmapCacheOption.OnLoad; // 确保加载后释放文件句柄
                    image.EndInit();
                });
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
