using System.Globalization;
using System.Windows.Data;

namespace NetEase.Converters
{
    /// <summary>
    /// 将ScrollViewer的滚动偏移量 (VerticalOffset) 转换为一个 Opacity 值 (1.0 -> 0.0)。
    /// 当滚动距离达到 FadeOutRange 时，Opacity 变为 0。
    /// </summary>
    public class ScrollToOpacityConverter : IValueConverter
    {
        /// <summary>
        /// 定义一个距离，当滚动偏移量从 0 增加到这个距离时，透明度会从 1.0 线性渐变到 0.0。
        /// 默认值为 300，可以在XAML中进行修改。
        /// </summary>
        public double FadeOutRange { get; set; } = 300.0;

        /// <summary>
        /// 执行从源到目标的转换。
        /// </summary>
        /// <param name="value">绑定源产生的值 (这里是 ScrollViewer.VerticalOffset, double 类型)。</param>
        /// <param name="targetType">绑定目标属性的类型 (这里是 Opacity, double 类型)。</param>
        /// <param name="parameter">转换器参数 (未使用)。</param>
        /// <param name="culture">转换时要使用的区域性信息 (未使用)。</param>
        /// <returns>转换后的 Opacity 值。</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 检查传入的值是否是 double 类型
            if (value is double verticalOffset)
            {
                // 计算当前滚动位置在渐变范围内的比例
                // 例如：如果滚动了 150，范围是 300，则比例是 0.5
                double ratio = verticalOffset / FadeOutRange;

                // 透明度与比例成反比：滚动越多，越透明
                // opacity = 1.0 (完全不透明) - ratio
                double opacity = 1.0 - ratio;

                // 使用 Math.Max 和 Math.Min 确保最终的 opacity 值被限制在 [0.0, 1.0] 的有效范围内
                // 防止因滚动超出 FadeOutRange 而导致 opacity 变为负数
                return Math.Max(0.0, Math.Min(1.0, opacity));
            }

            // 如果传入的值不是 double，则返回默认值 1.0 (完全不透明)
            return 1.0;
        }

        /// <summary>
        /// 执行从目标到源的转换 (在我们的场景中不需要)。
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 直接抛出异常，因为我们不支持反向转换
            throw new NotImplementedException();
        }
    }
}
