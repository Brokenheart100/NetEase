using NetEase.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace NetEase.Views.Components
{
    /// <summary>
    /// PlayerControlView.xaml 的交互逻辑
    /// </summary>
    public partial class PlayerControlView : UserControl
    {
        public PlayerControlView()
        {
            InitializeComponent();
        }
        // 当用户点击或开始拖动滑块时
        private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PlayerControlViewModel vm)
            {
                vm.IsDragging = true;
            }
        }

        // 当用户释放鼠标时
        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PlayerControlViewModel vm)
            {
                // 用滑块的最终值来触发一次寻道
                if (sender is Slider slider)
                {
                    vm.CurrentProgress = slider.Value;
                }
                vm.IsDragging = false;
            }
        }
    }
}
