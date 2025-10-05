using NetEase.Services;
using NetEase.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace NetEase.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel, MediaPlayerService mediaPlayerService)
        {
            InitializeComponent();
            DataContext = viewModel;

            // 在窗口加载后，将 MediaElement 传递给服务
            this.Loaded += (s, e) =>
            {
                mediaPlayerService.Initialize(this.MediaPlayer);
            };
       
        }
       
        private void SignUpView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 将事件标记为“已处理”
            // 这样它就不会再向上“冒泡”到父控件（如SignUpOverlayGrid）
            e.Handled = true;
        }
    }
}