using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NetEase.Converters
{
    /// <summary>
    /// 将ScrollViewer的滚动偏移量 (VerticalOffset) 转换为一个用于视差效果的平移距离。
    /// </summary>
    public class ScrollToParallaxConverter : IValueConverter
    {
        /// <summary>
        /// 视差因子。这个值决定了元素移动的速度相对于滚动速度的比例。
        /// - 0.0: 完全不移动。
        /// - -0.5: 以滚动速度的一半向上移动。
        /// - -1.0: 与滚动速度同步向上移动。
        /// 默认值为 -0.5，这是一个常见的视差效果值。
        /// </summary>
        public double Factor { get; set; } = -0.5;

        /// <summary>
        /// 执行从源到目标的转换。
        /// </summary>
        /// <param name="value">绑定源产生的值 (ScrollViewer.VerticalOffset, double)。</param>
        /// <returns>转换后的平移距离 (double)。</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double verticalOffset)
            {
                // 将滚动偏移量乘以视差因子
                // 例如：如果滚动了 100px，Factor是-0.5，则返回 -50.0
                // 这意味着元素会向上平移50px
                return verticalOffset * Factor;
            }

            // 默认返回0.0，表示不平移
            return 0.0;
        }

        /// <summary>
        /// 执行从目标到源的转换 (不需要)。
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
