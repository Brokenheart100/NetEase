using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using NetEase.Services;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NetEase.Models
{
    public partial class Playlist : ObservableObject
    {
        public int Id { get; set; }
        [ObservableProperty]
        private string _coverImageUrl; // 这是【网络】URL

        [ObservableProperty]
        [JsonIgnore] // 确保这个属性不会被序列化
        private ImageSource _coverImageSource; // 这是【UI】绑定的图片源
        private async Task LoadCoverImageAsync(ICacheService cacheService)
        {
            // a. 检查URL是否有效
            if (string.IsNullOrWhiteSpace(CoverImageUrl))
            {
                CoverImageSource = null; // 清空图片
                return;
            }

            // b. 调用缓存服务
            var localPath = await cacheService.GetFileAsync(CoverImageUrl);

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
                    CoverImageSource = bitmap; // 【修正】使用正确的属性名
                });
            }
            else
            {
                CoverImageSource = null; // 【修正】加载失败，也使用正确的属性名
            }
        }

        public Task StartImageLoadingAsync(ICacheService cacheService)
        {
            return LoadCoverImageAsync(cacheService);
        }
        public string Title { get; set; }
        public string Subtitle { get; set; } // 副标题，用于换行
        public bool IsVip { get; set; } // 是否有VIP角标
        public ObservableCollection<Song> Songs { get; set; } // 新增：存储这个歌单里的所有歌曲
        public int TrackCount { get; set; }
        public string? Description { get; internal set; }

        [ObservableProperty]
        private bool _isSelected;
    }
}
