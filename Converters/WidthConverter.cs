using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NetEase.Converters
{
    public class WidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value 是 ListView 的 ActualWidth
            // parameter 是我们想减去的其他列的总宽度
            if (value is double actualWidth && parameter is string fixedWidthStr)
            {
                if (double.TryParse(fixedWidthStr, out double fixedWidth))
                {
                    // 计算剩余宽度
                    double remainingWidth = actualWidth - fixedWidth;

                    // 确保宽度不为负数，并留出一点缓冲空间以防滚动条出现
                    return remainingWidth > 10 ? remainingWidth - 10 : 10;
                }
            }
            return 200; // 返回一个默认宽度以防意外
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
