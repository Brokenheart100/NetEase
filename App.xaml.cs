using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetEase.ServiceDefaults;
using NetEase.Services;
using NetEase.Services.NavigationService;
using NetEase.ViewModels;
using NetEase.ViewModels.ChatViewModels;
using NetEase.ViewModels.PlaylistViewModels;
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
        public static IHost AppHost { get; private set; }
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
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    ConfigureServices(services, hostContext.Configuration);
                })
                .Build();
        }
        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // 3. 【核心】添加 Aspire 服务默认值
            //    注意：这里我们是在 IServiceCollection 上调用，所以需要一个辅助方法
            //    或者我们可以直接在 builder 上配置
            //    为了简单，我们直接在这里配置HttpClient

            // 您的 AddServiceDefaults() 扩展方法可能需要 IHostApplicationBuilder
            // 所以我们换一种等价的配置方式，或者直接在 builder 上配置
            // services.AddServiceDefaults(); // 这种方式可能不存在
            var apiGatewayUrl = configuration["ApiGatewayUrl"];
            if (string.IsNullOrEmpty(apiGatewayUrl))
            {
                // 提供一个默认值以防万一
                apiGatewayUrl = "http://localhost:5240";
            }
            // a. 注册强类型配置
            services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));
            services.AddSingleton<HttpClient>(sp => new HttpClient
            {
                //BaseAddress = new Uri("http://localhost:5240/")
                BaseAddress = new Uri(apiGatewayUrl)
            });
            // b. 【核心修正】使用现代的 IHttpClientFactory 为每个服务配置 HttpClient
            services.AddHttpClient<AuthService>(client =>
            {
                client.BaseAddress = new Uri(apiGatewayUrl);
            });
            services.AddHttpClient<PlaylistService>(client =>
            {
                client.BaseAddress = new Uri(apiGatewayUrl);
            });
            services.AddHttpClient<SearchService>(client =>
            {
                client.BaseAddress = new Uri(apiGatewayUrl);
            });

            // c. FileCacheService 的 HttpClient 配置
            services.AddHttpClient<ICacheService, FileCacheService>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler();
#if DEBUG
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
#endif
                    return handler;
                });

            services.AddHttpClient<CacheService>();
            services.AddSingleton<AuthService>();
            // �����б����� transient��ÿ�����󴴽���ʵ�����ʺ�����������״̬������
            services.AddTransient<PlaylistService>();
            // ���������񣨵�����ȫ��Ψһ�Ĳ���������ʵ����
            services.AddSingleton<PlayerService>();
            // ý�岥�ŷ��񣨵���������ý���ļ����ŵĺ��ķ���
            services.AddSingleton<MediaPlayerService>();
            // ���ѷ��񣨵������������ѹ�ϵ��ȫ�ַ���
            services.AddSingleton<FriendService>();
            // SignalR���񣨵�����ʵʱͨ�����ӹ�����ȫ��Ψһ���ӣ�
            services.AddSingleton<SignalRService>();
            services.AddSingleton<CredentialService>();
            services.AddTransient<LyricService>();
            services.AddSingleton<SongService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<CurrentUserStateService>();
            services.AddSingleton<ICacheService, FileCacheService>();

            //services.AddHttpClient<ImageCacheService>();
            // --- ��ͼģ�ͣ�ViewModel��ע�� ---
            // ע��MVVMģʽ�е���ͼģ�ͣ��������ݴ�������ͼ�����߼�

            // ��������ͼģ�ͣ���������Ӧ����������һ�µ������������ģ�
            services.AddSingleton<MainViewModel>();
            // ��������ͼģ�ͣ�������ȫ�ֱ�����״̬������
            services.AddSingleton<TitleBarViewModel>();
            // ������������ͼģ�ͣ�������ȫ�ֲ����������߼���
            services.AddSingleton<PlayerControlViewModel>();
            // ������񣨵��������������߼��ĺ��ķ���
            services.AddSingleton<ChatService>();
            // ��֤��ͼģ�ͣ���������¼/ע�����֤����߼���
            services.AddSingleton<SignUpViewModel>();
            // �ļ����񣨵����������ļ�������ȫ�ַ���
            services.AddSingleton<FileService>();
            services.AddSingleton<SignalRService>();
            services.AddSingleton<UserProfileService>();
            services.AddSingleton<SearchService>();
            services.AddHttpClient<CacheService>();

            // �ҵ��ղ�������ͼģ�ͣ�transient��ÿ�δ�ҳ�洴����ʵ����
            services.AddTransient<MyFavoriteMusicViewModel>();
            // ����������ͼģ�ͣ�transient��ÿ�η��ʴ�����ʵ�����ʺ�Ƶ��ˢ�µĳ�����
            services.AddTransient<LocalMusicViewModel>();
            // ������ͼģ�ͣ�transient�����贴���������ڴ�ռ�ã�
            services.AddTransient<PodcastViewModel>();
            // �Ƽ�������ͼģ�ͣ�transient������Ƶ�����£�ÿ�μ����������ݣ�
            services.AddTransient<FeaturedViewModel>();
            // ������ͼģ�ͣ�transient��ÿ�����촰�ڿ�����Ҫ����ʵ����
            services.AddTransient<ChatViewModel>();
            // ������ͼģ�ͣ�transient�������б�ҳ�����ͼ�߼���
            services.AddTransient<FriendsViewModel>();
            // �ظ�ע��ı���������ͼģ�ͣ�ע�⣺�ظ�ע������Ǵ������࣬����������
            services.AddTransient<LocalMusicViewModel>();
            // ���ӵ������б���ͼģ�ͣ�transient����������ͼģ�ͣ����꼴���٣�
            services.AddTransient<AddToPlaylistViewModel>();
            // ��ϵ����ͼģ�ͣ�transient����ϵ��ҳ��Ľ����߼���
            services.AddTransient<ContactsViewModel>();
            // �ظ�ע���������ͼģ�ͣ�ע�⣺�����Ƿ��Ҫ��������Դ�˷ѣ�
            services.AddTransient<ChatViewModel>();
            // �Ự��ͼģ�ͣ�transient����������Ự����ʱ���ݣ�
            services.AddTransient<SessionsViewModel>();
            // �û�������ͼģ�ͣ�transient���û�����ҳ��Ľ����߼���
            services.AddTransient<UserProfileViewModel>();
            services.AddTransient<SongDetailViewModel>();
            services.AddTransient<PlaylistViewModel>();
            services.AddTransient<SearchResultViewModel>();
            services.AddTransient<EditPlaylistViewModel>();
            services.AddTransient<CommentViewModel>();
            services.AddTransient<CommentService>(); // ���� CommentService Ҳ��Ҫע��
            services.AddTransient<CreateLPlaylistViewModel>();

            // �����ڣ�������Ӧ�ó���Ψһ�������ڣ�
            services.AddSingleton<MainWindow>();
        }
        /// <summary>
        /// 重写应用程序启动方法
        /// 在应用启动时从依赖注入容器中获取主窗口并显示
        /// </summary>
        /// <param name="e">启动事件参数（包含命令行参数等）</param>

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            // 【修正】从 AppHost.Services 获取服务，这是唯一的服务提供者
            var cacheService = AppHost.Services.GetRequiredService<ICacheService>();
            await cacheService.InitializeAsync();

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (AppHost != null)
            {
                await AppHost.StopAsync();
            }
            base.OnExit(e);
        }
    }
}