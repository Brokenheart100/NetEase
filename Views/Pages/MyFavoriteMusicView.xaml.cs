using NetEase.Models;
using NetEase.Services;
using NetEase.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TagLib.Mpeg;

namespace NetEase.Views.Pages
{
    /// <summary>
    /// MyFavoriteMusicView.xaml 的交互逻辑
    /// </summary>
    public partial class MyFavoriteMusicView : UserControl
    {
        public MyFavoriteMusicView()
        {
            InitializeComponent();

        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 1. 获取被双击的项
            //var listView = sender as ListView;
            //if (listView == null) return;

            //var selectedItem = listView.SelectedItem;
            //if (selectedItem == null) return;
            //Debug.WriteLine("sdadasd");
            //if (selectedItem is Song selectedPerson)
            //{
            //    OnPlayRequested(selectedItem as Song);
            //}
            if (this.DataContext is MyFavoriteMusicViewModel viewModel)
            {
                // 2. 获取被双击的数据项
                if (sender is ListView listView && listView.SelectedItem is Song selectedSong)
                {
                    // 3. 检查 ViewModel 上的命令是否可用，并手动执行它
                    // 我们不再直接调用 PlaySong 方法，而是执行命令，这更符合 MVVM
                    if (viewModel.PlaySongCommand.CanExecute(selectedSong))
                    {
                        viewModel.PlaySongCommand.Execute(selectedSong);
                    }
                }
            }
        }
        // 当播放请求事件被触发时，执行此方法
        private void OnPlayRequested(Song songToPlay)
        {
            Debug.WriteLine($"[MainWindow] 收到播放请求: {songToPlay.Title}, 路径: {songToPlay.FilePath}");
            if (songToPlay != null && !string.IsNullOrEmpty(songToPlay.FilePath))
            {
                try
                {
                    // 设置 MediaElement 的源并开始播放
                    //MediaPlayer.Source = new Uri(songToPlay.FilePath);
                    //MediaPlayer.Play();
                    if (songToPlay != null)
                    {
                        // 调用播放服务来请求播放
                        PlayerService.Instance.PlaySong(songToPlay);
                        Debug.WriteLine($"正在播放歌曲: {songToPlay.Title}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"播放文件时出错: {ex.Message}");
                }
            }
        }

    }
}
