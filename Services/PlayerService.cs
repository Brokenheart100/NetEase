using NetEase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Services
{
    // 使用单例模式，确保整个应用只有一个播放服务实例
    public class PlayerService
    {
        private static readonly PlayerService _instance = new PlayerService();
        public static PlayerService Instance => _instance;

        // 定义一个事件，当需要播放歌曲时触发
        // 它将传递需要播放的 Song 对象
        public event Action<Song> PlayRequested;
        public event Action<double> VolumeChanged;
        // --- 新增：寻道请求事件和对应的方法 ---
        public event Action<double> SeekRequested;

        public void Seek(double percentage)
        {
            // 广播寻道请求事件，参数为百分比
            SeekRequested?.Invoke(percentage);
        }
        private PlayerService() { }

        // ViewModel 将调用这个方法来请求播放
        public void PlaySong(Song song)
        {
            // 触发事件，通知任何监听者（也就是MainWindow）
            PlayRequested?.Invoke(song);
        }
    }
}
