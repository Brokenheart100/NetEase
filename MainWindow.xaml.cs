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
        public List<Song> Songs { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            // 1. 创建示例数据
            Songs = new List<Song>
            {
                new Song
                {
                    Index = "01",
                    CoverImagePath = "E:\\Computer\\VS\\NetEase\\CoverImage\\0.jpg", // 确保图片在Images文件夹且Build Action为Resource
                    Title = "STAY",
                    Artist = "The Kid LAROI / Justin Bieber",
                    Album = "STAY",
                    Duration = "02:21",
                    Tags = new List<SongTag>
                    {
                        new SongTag { Text = "超清母带", Type = TagType.Master },
                        new SongTag { Text = "VIP", Type = TagType.VIP },
                        new SongTag { Text = "试听", Type = TagType.Trial },
                        new SongTag { Text = "MV", Type = TagType.MV }
                    }
                },
                new Song
                {
                    Index = "02",
                    CoverImagePath = "E:\\Computer\\VS\\NetEase\\CoverImage\\1.jpg",
                    Title = "Rolling in the Deep",
                    Artist = "Adele",
                    Album = "Rolling in the Deep",
                    Duration = "03:48",
                    Tags = new List<SongTag>
                    {
                        new SongTag { Text = "超清母带", Type = TagType.Master },
                        new SongTag { Text = "MV", Type = TagType.MV }
                    }
                },

            };

            //SongListView.ItemsSource = Songs;

        }

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            // 'sender' 参数就是被点击的那个按钮对象
            // 我们可以用它来获取按钮的信息，但这里我们只显示一个通用消息

            MessageBox.Show("按钮已经点击");
        }
     

    }
}