using Microsoft.Extensions.DependencyInjection;
using NetEase.Services;
using NetEase.ViewModels;
using NetEase.ViewModels.ChatViewModels;
using NetEase.ViewModels.MusicRowContextMenu;
using NetEase.Views;
using System.Net.Http;
using System.Windows;

namespace NetEase
{
    /// <summary>
    /// 应用程序入口点的交互逻辑类
    /// 继承自WPF的Application类，负责应用程序的生命周期管理和全局配置
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 静态服务提供者，全局可访问的依赖注入容器
        /// 用于在应用程序的任何地方获取已注册的服务实例
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// 应用程序构造函数
        /// 初始化依赖注入容器并配置服务
        /// </summary>
        public App()
        {
            // 创建服务集合（依赖注入的服务注册表）
            var serviceCollection = new ServiceCollection();

            // 配置服务（向服务集合中注册各类服务、视图模型、视图等）
            ConfigureServices(serviceCollection);

            // 构建服务提供者（完成依赖注入容器的初始化）
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// 配置依赖注入服务
        /// 向服务集合中注册应用所需的所有服务、视图模型和视图
        /// </summary>
        /// <param name="services">服务集合（用于注册服务的容器）</param>
        private void ConfigureServices(IServiceCollection services)
        {
            // --- HTTP 客户端配置 ---
            // 注册HttpClient单例实例，用于网络请求
            // 配置基础地址为后端API服务地址（此处为本地开发环境地址）
            //http://localhost:5215
            services.AddSingleton<HttpClient>(sp => new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5240/")

            });

            // --- 业务服务注册 ---
            // 注册应用的核心业务服务，按功能划分，生命周期根据需求设置

            // 认证服务（单例：全局共享一个认证状态实例）
            services.AddSingleton<AuthService>();
            // 播放列表服务（ transient：每次请求创建新实例，适合轻量级、无状态操作）
            services.AddTransient<PlaylistService>();
            // 播放器服务（单例：全局唯一的播放器控制实例）
            services.AddSingleton<PlayerService>();
            // 媒体播放服务（单例：处理媒体文件播放的核心服务）
            services.AddSingleton<MediaPlayerService>();
            // 好友服务（单例：管理好友关系的全局服务）
            services.AddSingleton<FriendService>();
            // SignalR服务（单例：实时通信连接管理，全局唯一连接）
            services.AddSingleton<SignalRService>();
            services.AddSingleton<CredentialService>();
            services.AddTransient<LyricService>();

            // --- 视图模型（ViewModel）注册 ---
            // 注册MVVM模式中的视图模型，负责数据处理和视图交互逻辑

            // 主窗口视图模型（单例：与应用生命周期一致的主数据上下文）
            services.AddSingleton<MainViewModel>();
            // 标题栏视图模型（单例：全局标题栏状态管理）
            services.AddSingleton<TitleBarViewModel>();
            // 播放器控制视图模型（单例：全局播放器控制逻辑）
            services.AddSingleton<PlayerControlViewModel>();
            // 聊天服务（单例：管理聊天逻辑的核心服务）
            services.AddSingleton<ChatService>();
            // 认证视图模型（单例：登录/注册等认证相关逻辑）
            services.AddSingleton<AuthenticationViewModel>();
            // 文件服务（单例：处理文件操作的全局服务）
            services.AddSingleton<FileService>();
            // 重复注册的SignalR服务（注意：实际开发中应避免重复注册，可能导致冲突）
            services.AddSingleton<SignalRService>();
            services.AddSingleton<UserProfileService>();

            // 我的收藏音乐视图模型（transient：每次打开页面创建新实例）
            services.AddTransient<MyFavoriteMusicViewModel>();
            // 本地音乐视图模型（transient：每次访问创建新实例，适合频繁刷新的场景）
            services.AddTransient<LocalMusicViewModel>();
            // 播客视图模型（transient：按需创建，减轻内存占用）
            services.AddTransient<PodcastViewModel>();
            // 推荐内容视图模型（transient：内容频繁更新，每次加载最新数据）
            services.AddTransient<FeaturedViewModel>();
            // 聊天视图模型（transient：每个聊天窗口可能需要独立实例）
            services.AddTransient<ChatViewModel>();
            // 好友视图模型（transient：好友列表页面的视图逻辑）
            services.AddTransient<FriendsViewModel>();
            // 重复注册的本地音乐视图模型（注意：重复注册可能是代码冗余，建议清理）
            services.AddTransient<LocalMusicViewModel>();
            // 添加到播放列表视图模型（transient：弹窗类视图模型，用完即销毁）
            services.AddTransient<AddToPlaylistViewModel>();
            // 联系人视图模型（transient：联系人页面的交互逻辑）
            services.AddTransient<ContactsViewModel>();
            // 重复注册的聊天视图模型（注意：需检查是否必要，避免资源浪费）
            services.AddTransient<ChatViewModel>();
            // 会话视图模型（transient：管理聊天会话的临时数据）
            services.AddTransient<SessionsViewModel>();
            // 用户资料视图模型（transient：用户资料页面的交互逻辑）
            services.AddTransient<UserProfileViewModel>();
            services.AddTransient<SongDetailViewModel>();

            // --- 视图（View）注册 ---
            // 注册WPF窗口，用于通过依赖注入创建视图实例

            // 主窗口（单例：应用程序唯一的主窗口）
            services.AddSingleton<MainWindow>();

        }

        /// <summary>
        /// 重写应用程序启动方法
        /// 在应用启动时从依赖注入容器中获取主窗口并显示
        /// </summary>
        /// <param name="e">启动事件参数（包含命令行参数等）</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // 调用基类启动逻辑
            base.OnStartup(e);

            // 从依赖注入容器中获取主窗口实例
            // 容器会自动解析MainWindow的所有依赖（如构造函数中的MainViewModel）
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();

            // 显示主窗口
            mainWindow.Show();
        }
    }
}