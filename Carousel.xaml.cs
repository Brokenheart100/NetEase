using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace NetEase
{
    public partial class Carousel : UserControl
    {
        private readonly List<string> _imageSources;
        private readonly DispatcherTimer _autoPlayTimer;
        private int _currentIndex = -1;

        public Carousel()
        {
            InitializeComponent();
            _imageSources = new List<string>
            {
                "E:\\Computer\\VS\\NetEase\\CoverImage\\0.jpg",
                "E:\\Computer\\VS\\NetEase\\CoverImage\\1.jpg", // 模拟第二张图
                "E:\\Computer\\VS\\NetEase\\CoverImage\\2.jpg", // 模拟第三张图
                "E:\\Computer\\VS\\NetEase\\CoverImage\\3.jpg", // ...
            };


            // 2. 将数据源绑定到两个 ItemsControl
            ImageItemsControl.ItemsSource = _imageSources;
            IndicatorItemsControl.ItemsSource = _imageSources;
            // 3. 初始化并配置计时器
            _autoPlayTimer = new DispatcherTimer
            {
                // 设置间隔为3秒
                Interval = TimeSpan.FromSeconds(3)
            };
            // 每次计时器到达时间点，就调用 Timer_Tick 方法
            _autoPlayTimer.Tick += Timer_Tick;

            // 4. 开始轮播
            if (_imageSources.Any())
            {
                // 首次加载，显示第一张图片
                ShowImage(0);
                // 启动计时器
                _autoPlayTimer.Start();
            }
        }
        private void ShowImage(int index)
        {
            if (index < 0 || index >= _imageSources.Count) return;

            // 如果当前有图片正在显示，先隐藏它
            if (_currentIndex != -1)
            {
                var prevImage = GetImageControl(_currentIndex);
                if (prevImage != null) prevImage.Visibility = Visibility.Collapsed;

                var prevDot = GetIndicatorControl(_currentIndex);
                if (prevDot != null) prevDot.Background = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF)); // 恢复为半透明
            }

            // 更新当前索引
            _currentIndex = index;

            // 显示新的图片
            var currentImage = GetImageControl(_currentIndex);
            if (currentImage != null) currentImage.Visibility = Visibility.Visible;

            // 高亮新的圆点
            var currentDot = GetIndicatorControl(_currentIndex);
            if (currentDot != null) currentDot.Background = Brushes.White; // 设置为纯白色
        }

        // 计时器触发时调用的方法
        private void Timer_Tick(object sender, EventArgs e)
        {
            // 计算下一张图片的索引
            // 使用 % (取模) 运算符实现循环
            int nextIndex = (_currentIndex + 1) % _imageSources.Count;
            ShowImage(nextIndex);
        }

        #region Helper Methods (辅助方法)

        // 辅助方法：根据索引获取对应的Image控件
        private Image GetImageControl(int index)
        {
            if (index < 0 || index >= ImageItemsControl.Items.Count) return null;
            // 获取 ItemsControl 中指定索引的容器
            var container = ImageItemsControl.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
            if (container == null) return null;
            // 在容器中查找 Image 控件
            return FindVisualChild<Image>(container);
        }

        // 辅助方法：根据索引获取对应的圆点Border控件
        private Border GetIndicatorControl(int index)
        {
            if (index < 0 || index >= IndicatorItemsControl.Items.Count) return null;
            var container = IndicatorItemsControl.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
            if (container == null) return null;
            return FindVisualChild<Border>(container);
        }

        // 泛型辅助方法：在可视化树中查找指定类型的子控件
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        #endregion
    }
}