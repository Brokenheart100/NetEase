using NetEase.Services;
using NetEase.ViewModels;
using System.Windows;

namespace NetEase.Views
{
    public partial class MainWindow : Window
    {
        // 构造函数接收 MainViewModel 和 MediaPlayerService
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
    }
}