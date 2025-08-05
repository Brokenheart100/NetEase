using NetEase.Models;
using NetEase.Services; // 引入服务命名空间
using NetEase.ViewModels;
using System;
using System.Diagnostics; // 引入 Uri
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NetEase.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();

            //SongListView.ItemsSource = Songs;
            // 将主窗口的数据源设置为MainViewModel的实例
            DataContext = new MainViewModel();
            
            PlayerService.Instance.PlayRequested += OnPlayRequested;
            //MediaPlayer.Source = new Uri("E:\\Computer\\VS\\NetEase\\music\\Clear Sky - Call of Silence.flac");
            //MediaPlayer.Play();
            PlayerService.Instance.SeekRequested += OnSeekRequested; // 新增订阅

            // --- 配置计时器 ---
            _timer.Interval = TimeSpan.FromMilliseconds(500); // 每秒更新两次
            _timer.Tick += Timer_Tick;

            // --- 添加 MediaEnded 事件，用于在歌曲播放结束时停止计时器 ---
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }

        // 当播放请求事件被触发时，执行此方法
        private void OnPlayRequested(Song songToPlay)
        {
            // 为新歌曲重置 ViewModel 中的播放器状态
            (DataContext as MainViewModel)?.PlayerControlVM.ResetForNewSong();

            if (songToPlay != null && !string.IsNullOrEmpty(songToPlay.FilePath))
            {
                try
                {
                    MediaPlayer.Source = new Uri(songToPlay.FilePath, UriKind.Absolute);
                    MediaPlayer.Play();
                    _timer.Start(); // 启动进度计时器
                }
                catch (Exception ex)
                {
                    _timer.Stop();
                    MessageBox.Show($"播放出错: {ex.Message}");
                }
            }
            else
            {
                _timer.Stop(); // 如果歌曲无效，停止计时器
            }
        }

        // --- 新增：计时器触发事件的处理方法 ---
        private void Timer_Tick(object sender, EventArgs e)
        {
            // 确保播放器已准备好
            if (MediaPlayer.Source == null || !MediaPlayer.NaturalDuration.HasTimeSpan)
                return;

            var mainViewModel = DataContext as MainViewModel;
            if (mainViewModel == null) return;

            var playerVM = mainViewModel.PlayerControlVM;

            // 如果用户【没有】正在拖动滑块，才由计时器更新进度
            if (!playerVM.IsDragging)
            {
                playerVM.CurrentProgress = MediaPlayer.Position.TotalSeconds / MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds * 100;
                playerVM.CurrentTime = MediaPlayer.Position.ToString(@"mm\:ss");
                playerVM.TotalTime = MediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            }
        }

        // --- 新增：寻道请求的处理方法 ---
        private void OnSeekRequested(double percentage)
        {
            if (MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                double newPositionSeconds = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds * (percentage / 100.0);
                MediaPlayer.Position = TimeSpan.FromSeconds(newPositionSeconds);
            }
        }

        // --- 新增：歌曲结束时停止计时器 ---
        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            // 可选：将进度设置为100%，时间显示为总时长
            var playerVM = (DataContext as MainViewModel)?.PlayerControlVM;
            if (playerVM != null)
            {
                playerVM.CurrentProgress = 100;
                playerVM.CurrentTime = playerVM.TotalTime;
            }
        }

        private void OnVolumeChanged(double newVolume)
        {
            MediaPlayer.Volume = newVolume;
        }

    }
}