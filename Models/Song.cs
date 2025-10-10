using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using NetEase.Services;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NetEase.Models
{
    public partial class Song : ObservableObject
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string Title { get; set; } = "Default";
        public string Subtitle { get; set; }
        public List<SongTag> Tags { get; set; }
        public string Artist { get; set; } = "DefaultSinger";
        public string Album { get; set; }
        public bool IsLiked { get; set; } = false;
        public string Duration { get; set; }
        public string FilePath { get; set; }
        public string AlbumTitle { get; set; }
        public string ArtistName { get; set; }
        //图片
        [ObservableProperty]
        private bool _isPlaying;
        [ObservableProperty]
        private bool _isDownloaded;
        [ObservableProperty]
        private bool _isLike;

        [ObservableProperty]
        private string _coverImageUrl; // 这是【网络】URL

        [ObservableProperty]
        [JsonIgnore] // 确保这个属性不会被序列化
        private ImageSource _coverImage; // 这是【UI】绑定的图片源

        // 当 CoverImageUrl 变化时，触发异步加载
        //async partial void OnCoverImageUrlChanged(string value)
        //{
        //    var cacheService = App.ServiceProvider.GetRequiredService<CacheService>();
        //    string localPath = await cacheService.GetFileAsync(value, CacheType.Image);
        //    if (localPath == null) return;
        //    var bitmap = new BitmapImage();
        //    bitmap.BeginInit();
        //    bitmap.UriSource = new Uri(localPath);
        //    bitmap.CacheOption = BitmapCacheOption.OnLoad;
        //    bitmap.EndInit();
        //    CoverImage = bitmap; // 更新UI绑定的属性
        //}
        // 3. 【核心】利用自动生成的钩子方法，在 CoverImageUrl 变化时加载图片
        //    这个方法需要一个能访问 CacheService 的地方，我们将在ViewModel中调用一个包装器
        private async Task LoadCoverImageAsync(ICacheService cacheService)
        {
            // a. 检查URL是否有效
            if (string.IsNullOrWhiteSpace(CoverImageUrl))
            {
                CoverImage = null; // 清空图片
                return;
            }

            // b. 调用缓存服务
            string? localPath = await cacheService.GetFileAsync(CoverImageUrl);

            // c. 如果成功，在UI线程上创建并设置图片
            if (localPath != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(localPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    CoverImage = bitmap; // 【修正】使用正确的属性名
                });
            }
            else
            {
                CoverImage = null; // 【修正】加载失败，也使用正确的属性名
            }
        }

        // 4. (推荐) 创建一个公共的包装器方法
        //    这样 ViewModel 的调用代码会更清晰
        public Task StartImageLoadingAsync(ICacheService cacheService)
        {
            return LoadCoverImageAsync(cacheService);
        }
    }
    public class SongTag
    {
        public string Text { get; set; } = "default";
        public Brush Background { get; set; }
        public Brush Foreground { get; set; }
        public Brush BorderBrush { get; set; }
    }

}
