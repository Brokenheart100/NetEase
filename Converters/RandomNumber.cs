using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Converters
{
    public static class RandomNumber
    {
        public static readonly Random _random = new Random();
        public static string GetRandomAvatarUrl()
        {
            // 生成一个 0 到 50 之间（包含 0 和 50）的随机整数
            int imageNumber = _random.Next(0, 51);

            // 将随机数拼接成完整的文件路径
            // 请确保您的图片格式是 .jpg，如果不是，请修改后缀
            return $"E:\\Computer\\VS\\NetEase\\CoverImage\\{imageNumber}.jpg";
        }
    }
}
