// 引入MVVM工具包的组件基类（提供属性通知等功能）
using CommunityToolkit.Mvvm.ComponentModel;
// 引入MVVM工具包的命令特性（用于声明RelayCommand）
using CommunityToolkit.Mvvm.Input;
// 引入项目中的模型类（如Song、Playlist等前端展示模型）
using NetEase.Models;
// 引入项目中的服务类（处理业务逻辑和数据访问）
using NetEase.Services;
// 引入可观察集合（用于数据绑定，集合变化时自动通知UI）
using System.Collections.ObjectModel;
// 引入调试工具（用于输出调试信息）
using System.Diagnostics;
// 引入异步任务相关类型
using System.Threading.Tasks;
// 引入WPF的UI组件（如MessageBox）
using System.Windows;
// 引入数据传输对象（DTO，用于服务层与视图模型间的数据传递）
using NetEase.Dtos;

// 命名空间：属于音乐行上下文菜单相关的视图模型
// 遵循MVVM模式，ViewModel层负责处理业务逻辑并为View提供数据
namespace NetEase.ViewModels.MusicRowContextMenu
{
    /// <summary>
    /// 用于处理"添加到播放列表"功能的视图模型
    /// 关联视图通常为对话框或上下文菜单，负责协调视图与服务层的交互
    /// </summary>
    public partial class AddToPlaylistViewModel : ObservableObject
    {
        // 播放列表服务实例（通过依赖注入获取），负责处理播放列表相关的业务逻辑
        private readonly PlaylistService _playlistService;
        // 需要添加到播放列表的歌曲实例
        private readonly Song _songToAdd;
        private static readonly ObservableCollection<Playlist> playlists = [];

        /// <summary>
        /// 绑定到视图中ListBox的播放列表集合
        /// 使用ObservableCollection确保集合变化时（如添加/删除元素）能自动通知UI更新
        /// </summary>
        public ObservableCollection<Playlist> Playlists { get; } = playlists;

        /// <summary>
        /// 用于从视图模型关闭关联窗口的委托
        /// 由视图（如Window）赋值，实现ViewModel对视图关闭操作的控制
        /// </summary>
        public Action CloseWindow { get; set; }

        /// <summary>
        /// 构造函数（通过依赖注入初始化服务和待添加歌曲）
        /// </summary>
        /// <param name="playlistService">播放列表服务实例（处理数据访问和业务逻辑）</param>
        /// <param name="songToAdd">需要添加到播放列表的歌曲对象</param>
        public AddToPlaylistViewModel(PlaylistService playlistService, Song songToAdd)
        {
            _playlistService = playlistService;
            _songToAdd = songToAdd;
            // 初始化时加载用户的播放列表数据
            LoadPlaylistsAsync();
        }

        /// <summary>
        /// 异步加载用户的播放列表数据
        /// 从服务层获取DTO，转换为前端模型后填充到绑定集合中
        /// </summary>
        private async Task LoadPlaylistsAsync()
        {
            // 调用服务层方法获取用户的播放列表DTO（数据传输对象）
            // DTO通常用于服务层与ViewModel之间的数据传递，隔离数据访问层与前端模型
            var playlistDtos = await _playlistService.GetMyPlaylistsAsync();

            // 输出调试信息，方便开发时查看数据加载状态
            Debug.WriteLine($"Enter LoadPlaylistsAsync() 加载到的播放列表DTO数量: {playlistDtos?.Count ?? 0}");

            // 若获取到有效数据，则更新播放列表集合
            if (playlistDtos != null)
            {
                // 清空现有数据（避免重复加载）
                Playlists.Clear();

                // 将DTO转换为前端展示用的Playlist模型（Model）
                // 转换原因：DTO可能包含敏感字段或冗余信息，Model仅保留前端所需字段
                foreach (var dto in playlistDtos)
                {
                    Playlists.Add(new Playlist
                    {
                        Id = dto.Id,       // 播放列表ID（用于后续添加歌曲的标识）
                        Title = dto.Name,  // 播放列表名称（用于UI展示）
                        TrackCount = dto.TrackCount
                    });
                }
            }
        }

        /// <summary>
        /// 添加上下文菜单命令：将歌曲添加到选中的播放列表
        /// 标记为[RelayCommand]后，可直接在XAML中绑定到Button或MenuItem的Command属性
        /// </summary>
        /// <param name="targetPlaylist">用户选中的目标播放列表</param>
        [RelayCommand]
        private async Task AddToPlaylist(Playlist targetPlaylist)
        {
            // 参数校验：若目标播放列表或待添加歌曲为空，则直接返回（避免空引用异常）
            if (targetPlaylist == null || _songToAdd == null) return;

            // 调用服务层方法，将歌曲添加到目标播放列表
            // 传递播放列表ID和歌曲ID作为参数，服务层负责执行实际的添加逻辑（如API调用或数据库操作）
            var (success, message) = await _playlistService.AddSongToPlaylistAsync(
                targetPlaylist.Id,  // 目标播放列表ID
                _songToAdd.Id       // 待添加歌曲ID
            );

            // 根据操作结果显示相应的用户反馈
            if (success)
            {
                // 成功时提示用户，并关闭当前窗口
                MessageBox.Show($"已成功将{_songToAdd.Title}添加到歌单“{targetPlaylist.Title}”");
                CloseWindow?.Invoke(); // 调用委托关闭窗口（使用空值传播运算符避免空引用）
            }
            else
            {
                // 失败时显示错误信息（如网络异常、权限不足等）
                MessageBox.Show($"添加失败: {message}");
            }
        }
    }
}