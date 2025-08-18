using Microsoft.Extensions.DependencyInjection;
using NetEase.Services;
using NetEase.ViewModels;
using System.Net.Http;
using System.Windows;
using NetEase.Views;

namespace NetEase
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // --- HTTP Client ---
            // ✅ 确认已注册
            services.AddSingleton<HttpClient>(sp => new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5215/")
            });

            // --- Services ---
            // ✅ 确认已注册所有服务
            services.AddTransient<AuthService>();
            services.AddTransient<PlaylistService>();
            services.AddSingleton<PlayerService>();
            services.AddSingleton<MediaPlayerService>(); // 确认这个也注册了

            // --- ViewModels ---
            // ✅ 确认已注册所有 ViewModel
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<TitleBarViewModel>();
            services.AddSingleton<PlayerControlViewModel>();
            services.AddSingleton<AuthenticationViewModel>();

            services.AddTransient<MyFavoriteMusicViewModel>();
            services.AddTransient<LocalMusicViewModel>(); // 确认已注册
            services.AddTransient<PodcastViewModel>();    // 确认已注册

            // --- Views ---
            // ✅ 确认已注册 MainWindow
            services.AddSingleton<MainWindow>();

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 从 DI 容器中“解析”出主窗口的实例
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();

            // DI 容器会自动为 MainWindow 及其所有依赖项 (如 MainViewModel) 创建实例并注入
            mainWindow.Show();
        }
    }

}
