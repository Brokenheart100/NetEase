using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace NetEase.ViewModels
{
    public partial class TitleBarViewModel : BaseViewModel
    {
        // 搜索框文本属性
        [ObservableProperty]
        private string _searchText;

        // --- 窗口控制命令 ---
        public IRelayCommand<Window> DragWindowCommand { get; }
        public IRelayCommand<Window> MinimizeWindowCommand { get; }
        public IRelayCommand<Window> MaximizeWindowCommand { get; }
        public IRelayCommand<Window> CloseWindowCommand { get; }

        public TitleBarViewModel()
        {
            DragWindowCommand = new RelayCommand<Window>(DragWindow);
            MinimizeWindowCommand = new RelayCommand<Window>(MinimizeWindow);
            MaximizeWindowCommand = new RelayCommand<Window>(MaximizeWindow);
            CloseWindowCommand = new RelayCommand<Window>(CloseWindow);
        }

        private void DragWindow(Window window)
        {
            if (window != null)
            {
                // 注意：这里需要检查鼠标左键状态，避免右键等也触发拖动
                if (System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                {
                    window.DragMove();
                }
            }
        }

        private void MinimizeWindow(Window window)
        {
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        private void MaximizeWindow(Window window)
        {
            if (window != null)
            {
                window.WindowState = window.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
        }

        private void CloseWindow(Window window)
        {
            if (window != null)
            {
                window.Close();
            }
        }
    }
}