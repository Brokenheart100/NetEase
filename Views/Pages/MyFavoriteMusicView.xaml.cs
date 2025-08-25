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
        // 这是事件处理器
        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 1. 直接从 UserControl 获取 ViewModel
            if (this.DataContext is MyFavoriteMusicViewModel viewModel)
            {
                // 2. 从 ListView 获取选中的数据项
                if (sender is ListView listView && listView.SelectedItem is Song selectedSong)
                {
                    // 3. 手动执行 ViewModel 上的命令
                    if (viewModel.PlaySongCommand?.CanExecute(selectedSong) == true)
                    {
                        viewModel.PlaySongCommand.Execute(selectedSong);
                    }
                }
            }
        }
    }
}
