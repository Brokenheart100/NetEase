using NetEase.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;

namespace NetEase.Behaviors
{
    public static class ImageCacheBehavior
    {
        // 2. 获取 CacheService 的一个单例实例
        private static readonly CacheService _cacheService =
            App.ServiceProvider?.GetRequiredService<CacheService>();

        // 3. 定义一个附加属性 "SourceUrl"
        public static readonly DependencyProperty SourceUrlProperty =
            DependencyProperty.RegisterAttached(
                "SourceUrl",
                typeof(string),
                typeof(ImageCacheBehavior),
                new PropertyMetadata(null, OnSourceUrlChanged));

        public static string GetSourceUrl(DependencyObject obj)
        {
            return (string)obj.GetValue(SourceUrlProperty);
        }

        public static void SetSourceUrl(DependencyObject obj, string value)
        {
            obj.SetValue(SourceUrlProperty, value);
        }

        // 4. 当 SourceUrl 属性发生变化时，此方法会被调用
        private static async void OnSourceUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (_cacheService == null || e.NewValue is not string imageUrl)
            {
                return;
            }

            // 获取我们附加到的目标控件
            Image? imageControl = d as Image;
            Border? borderControl = d as Border;
            if (imageControl == null && borderControl == null) return;

            // a. 立即设置一个默认/占位图 (可选)
            // ...

            // b. 在后台异步获取缓存或下载图片
            string localPath = await _cacheService.GetFileAsync(imageUrl, CacheType.Image);

            if (localPath != null)
            {
                // c. 加载成功，创建一个新的 BitmapImage 并设置给控件
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(localPath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // 加载后立即释放文件句柄
                bitmap.EndInit();

                if (imageControl != null)
                {
                    imageControl.Source = bitmap;
                }
                else if (borderControl != null && borderControl.Background is ImageBrush brush)
                {
                    brush.ImageSource = bitmap;
                }
                else if (borderControl != null) // 如果Border还没有ImageBrush
                {
                    borderControl.Background = new ImageBrush(bitmap);
                }
            }
            else
            {
                // d. 加载失败，可以设置一个“加载失败”的图片
                // ...
            }
        }
    }
}
