using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NetEase.Converters
{
    public class NegativeMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double height)
            {
                // 返回一个Thickness，其中Top边距是负的高度
                return new Thickness(0, -height, 0, 0);
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
