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
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public List<SongTag> Tags { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Duration { get; set; }
        public string FilePath { get; set; }
        //图片
        [ObservableProperty]
        private bool _isPlaying;
        [ObservableProperty]
        private bool _isDownloaded;
        //public bool IsLiked { get; set; }
        [ObservableProperty]
        private bool _isLiked;

        [ObservableProperty]
        private string _coverImageUrl; // 这是【网络】URL

        [ObservableProperty]
        [JsonIgnore] // 确保这个属性不会被序列化
        private ImageSource _coverImage; // 这是【UI】绑定的图片源

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

        public Task StartImageLoadingAsync(ICacheService cacheService)
        {
            return LoadCoverImageAsync(cacheService);
        }
    }
    public class SongTag
    {
        public string Text { get; set; }
        public Brush Background { get; set; }
        public Brush Foreground { get; set; }
        public Brush BorderBrush { get; set; }
    }

}
