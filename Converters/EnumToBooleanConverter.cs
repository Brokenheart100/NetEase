using System.Globalization;
using System.Windows.Data;

namespace NetEase.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 检查绑定的枚举值是否与参数字符串表示的枚举值相同
            return value?.ToString().Equals(parameter?.ToString(), StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 当RadioButton被选中时(value=true)，将参数字符串转换回枚举值
            if (value is true)
            {
                return Enum.Parse(targetType, parameter.ToString(), true);
            }
            return Binding.DoNothing;
        }
    }
}
