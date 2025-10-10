using System.Windows;
using NetEase.ViewModels.PlaylistViewModels;

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
