using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TagLib;

namespace NetEase.Helpers
{
    public static class ImageHelper
    {
        // 默认封面图片的URI，设为静态只读字段
        private static readonly Uri DefaultCoverUri = new Uri("E:/Computer/VS/NetEase/CoverImage/20.jpg");

        // 缓存默认封面图片，避免重复加载
        private static readonly BitmapImage DefaultCoverImage = new BitmapImage(DefaultCoverUri);

        /// <summary>
        /// 从 TagLib 的 IPicture 数据创建一个 WPF BitmapImage。
        /// </summary>
        /// <param name="picture">TagLib 的图片对象。</param>
        /// <returns>一个可供WPF使用的 BitmapImage，如果转换失败则返回 null。</returns>
        public static BitmapImage CreateImageFromPicture(IPicture picture)
        {
            if (picture == null || picture.Data.Count == 0)
            {
                return null;
            }

            try
            {
                using (var stream = new MemoryStream(picture.Data.Data))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad; // 确保图片数据被完全加载
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze(); // 冻结对象，使其可以在其他线程中安全访问
                    return image;
                }
            }
            catch
            {
                // 如果图片数据损坏或格式不支持，则转换失败
                return null;
            }
        }

        /// <summary>
        /// 获取默认的封面图片。
        /// </summary>
        public static ImageSource GetDefaultCover()
        {
            return DefaultCoverImage;
        }
    }
}
