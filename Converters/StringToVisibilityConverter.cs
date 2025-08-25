using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NetEase.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// 将 string 转换为 Visibility。
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 检查输入的 value 是否是一个非空字符串
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                // 如果是，返回 Visible
                return Visibility.Visible;
            }

            // 否则，返回 Collapsed
            return Visibility.Collapsed;
        }

        /// <summary>
        /// 从 Visibility 转回 string (通常不需要实现)。
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
