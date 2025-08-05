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
    public class TagToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 如果Tag不为空（比如是"免费听"），则给标题一个左边距
            if (value is string tag && !string.IsNullOrEmpty(tag))
            {
                return new Thickness(45, 0, 0, 0);
            }
            // 否则，没有边距
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
