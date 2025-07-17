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

namespace NetEase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            // 'sender' 参数就是被点击的那个按钮对象
            // 我们可以用它来获取按钮的信息，但这里我们只显示一个通用消息

            MessageBox.Show("按钮已经点击");
        }

    }
}