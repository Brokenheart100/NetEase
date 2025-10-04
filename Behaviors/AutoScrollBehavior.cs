using NetEase.ViewModels;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace NetEase.Behaviors
{
    /// <summary>
    /// 自动滚动行为类，用于实现ItemsControl（如ListBox、ListView等）中
    /// 当特定数据项（如歌词行）变为"当前项"时，自动滚动到该项目并使其居中显示
    /// 采用WPF附加行为模式，可通过XAML附加属性启用
    /// </summary>
    public static class AutoScrollBehavior
    {
        /// <summary>
        /// 附加属性：控制是否启用自动滚动功能
        /// 当设置为true时，激活行为逻辑；false时关闭
        /// </summary>
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached(
                "AutoScroll",                   // 属性名称
                typeof(bool),                   // 属性类型
                typeof(AutoScrollBehavior),     // 所属类型
                new UIPropertyMetadata(false, OnAutoScrollChanged)); // 默认值及变化回调

        /// <summary>
        /// 获取AutoScroll附加属性的值
        /// </summary>
        /// <param name="obj">附加了该属性的依赖对象（通常是ItemsControl）</param>
        /// <returns>当前是否启用自动滚动</returns>
        public static bool GetAutoScroll(DependencyObject obj) => (bool)obj.GetValue(AutoScrollProperty);

        /// <summary>
        /// 设置AutoScroll附加属性的值
        /// </summary>
        /// <param name="obj">附加了该属性的依赖对象</param>
        /// <param name="value">是否启用自动滚动</param>
        public static void SetAutoScroll(DependencyObject obj, bool value) => obj.SetValue(AutoScrollProperty, value);

        /// <summary>
        /// AutoScroll属性值变化时的回调方法
        /// 负责开启或关闭自动滚动的监听逻辑
        /// </summary>
        /// <param name="d">附加了该属性的依赖对象（ItemsControl）</param>
        /// <param name="e">属性变化事件参数（包含新旧值）</param>
        private static void OnAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 确保依赖对象是ItemsControl（如ListBox等可显示列表项的控件）
            if (d is ItemsControl itemsControl)
            {
                // 当属性值设为true时，启用自动滚动逻辑
                if ((bool)e.NewValue)
                {
                    // 监听数据源的变化（如添加/删除项）
                    // 若ItemsSource实现了INotifyCollectionChanged（如ObservableCollection）
                    if (itemsControl.ItemsSource is INotifyCollectionChanged collection)
                    {
                        // 订阅集合变化事件，当集合项变化时处理新项/旧项的事件订阅
                        collection.CollectionChanged += (s, args) => HookOrUnhookItemPropertyChanged(itemsControl, args);
                    }
                    // 为已存在于ItemsSource中的项挂钩属性变化事件
                    HookUpToExistingItems(itemsControl);
                }
                else
                {
                    // 当属性值设为false时，可在此处添加清理逻辑
                    // 如取消所有事件订阅，避免内存泄漏
                }
            }
        }

        /// <summary>
        /// 为ItemsControl中已存在的所有数据项挂钩属性变化事件
        /// 确保初始化时已有的项也能触发自动滚动
        /// </summary>
        /// <param name="itemsControl">目标ItemsControl</param>
        private static void HookUpToExistingItems(ItemsControl itemsControl)
        {
            // 若数据源为空，则直接返回
            if (itemsControl.ItemsSource == null) return;

            // 遍历所有已存在的数据项
            foreach (var item in itemsControl.ItemsSource)
            {
                // 若项实现了INotifyPropertyChanged（支持属性变化通知）
                if (item is INotifyPropertyChanged notify)
                {
                    // 订阅其PropertyChanged事件，当属性变化时触发处理方法
                    notify.PropertyChanged += (s, args) => OnItemPropertyChanged(itemsControl, s, args);
                }
            }
        }

        /// <summary>
        /// 处理集合变化时的项事件订阅/取消订阅
        /// 确保新添加的项被监听，移除的项被取消监听
        /// </summary>
        /// <param name="itemsControl">目标ItemsControl</param>
        /// <param name="e">集合变化事件参数</param>
        private static void HookOrUnhookItemPropertyChanged(ItemsControl itemsControl, NotifyCollectionChangedEventArgs e)
        {
            // 处理新添加的项：为其订阅PropertyChanged事件
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is INotifyPropertyChanged notify)
                    {
                        notify.PropertyChanged += (s, args) => OnItemPropertyChanged(itemsControl, s, args);
                    }
                }
            }

            // 可在此处添加对移除项的处理：取消订阅其PropertyChanged事件
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is INotifyPropertyChanged notify)
                    {
                        notify.PropertyChanged -= (s, args) => OnItemPropertyChanged(itemsControl, s, args);
                    }
                }
            }
        }

        /// <summary>
        /// 数据项属性变化时的处理方法
        /// 检查是否是目标属性变化，若为当前项则触发滚动
        /// </summary>
        /// <param name="itemsControl">目标ItemsControl</param>
        /// <param name="item">发生属性变化的数据项</param>
        /// <param name="e">属性变化事件参数（包含变化的属性名）</param>
        private static void OnItemPropertyChanged(ItemsControl itemsControl, object item, PropertyChangedEventArgs e)
        {
            // 仅关注LyricLine（歌词行）的IsCurrentLine属性变化
            // 当该属性变为true时，表示这是当前需要高亮显示的歌词行
            if (e.PropertyName == nameof(LyricLine.IsCurrentLine))
            {
                // 将数据项转换为LyricLine类型
                var lyricLine = item as LyricLine;
                // 确认是歌词行且已标记为当前行
                if (lyricLine != null && lyricLine.IsCurrentLine)
                {
                    // 执行滚动逻辑，将当前行滚动到可视区域中心
                    ScrollToItem(itemsControl, item);
                }
            }
        }

        /// <summary>
        /// 核心滚动逻辑：将指定数据项滚动到ItemsControl的可视区域中心
        /// </summary>
        /// <param name="itemsControl">目标ItemsControl</param>
        /// <param name="item">需要滚动到的目标数据项</param>
        private static void ScrollToItem(ItemsControl itemsControl, object item)
        {
            // 查找ItemsControl的父级ScrollViewer（负责实际滚动的控件）
            var scrollViewer = FindAncestor<ScrollViewer>(itemsControl);
            if (scrollViewer == null) return; // 若未找到ScrollViewer，则无法滚动

            // 延迟执行滚动操作：通过Dispatcher以DataBind优先级异步执行
            // 确保UI已完成渲染，从而能正确获取控件位置和尺寸
            itemsControl.Dispatcher.InvokeAsync(() =>
            {
                // 获取数据项对应的UI容器（如ListBoxItem、ContentPresenter等）
                var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (container != null) // 确保容器已生成
                {
                    // 计算容器相对于ScrollViewer的坐标转换
                    var transform = container.TransformToAncestor(scrollViewer);
                    // 获取容器在ScrollViewer中的顶部Y坐标
                    var itemTop = transform.Transform(new Point(0, 0)).Y;

                    // 计算使容器居中的偏移量：
                    // 容器顶部位置 + 容器高度的一半 - 可视区域高度的一半
                    var centerOffset = itemTop + (container.ActualHeight / 2) - (scrollViewer.ViewportHeight / 2);

                    // 执行滚动：当前垂直偏移量 + 居中偏移量
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + centerOffset);
                }
            }, DispatcherPriority.DataBind); // 优先级设为DataBind，确保在数据绑定更新后执行
        }

        /// <summary>
        /// 辅助方法：从指定元素向上遍历视觉树，查找指定类型的祖先元素
        /// </summary>
        /// <typeparam name="T">要查找的祖先元素类型（必须是DependencyObject）</typeparam>
        /// <param name="current">起始元素</param>
        /// <returns>找到的祖先元素，若未找到则返回null</returns>
        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                // 若当前元素是目标类型，则返回
                if (current is T ancestor) return ancestor;
                // 否则向上查找父级元素
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null); // 直到根元素（null）停止
            return null;
        }
    }
}