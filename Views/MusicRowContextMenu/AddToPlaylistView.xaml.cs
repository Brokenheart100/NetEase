using NetEase.ViewModels.MusicRowContextMenu;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace NetEase.Views.MusicRowContextMenu
{
    /// <summary>
    /// AddToPlaylistView.xaml 的交互逻辑
    /// </summary>
    public partial class AddToPlaylistView : Window
    {
        public AddToPlaylistView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // 当新的DataContext是AddToPlaylistViewModel时
            if (e.NewValue is AddToPlaylistViewModel vm)
            {
                // 将ViewModel中的CloseWindow委托指向本窗口的Close方法
                vm.CloseWindow = this.Close;
            }
        }
    }
}
