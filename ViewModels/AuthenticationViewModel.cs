using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NetEase.Models;
using NetEase.Properties;
using NetEase.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NetEase.ViewModels
{
    // 这个 ViewModel 现在统一管理登录和注册
    public partial class AuthenticationViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        // --- 视图状态控制 ---
        [ObservableProperty]
        private bool _isRegistering = false; // false = 显示登录, true = 显示注册

        // --- 表单数据 ---
        // 'Username' 和 'Password' 用于【登录】和【注册】
        [ObservableProperty]
        private string _username; // 在登录时代表 Email, 在注册时也可以用作用户名

        [ObservableProperty]
        private string _password;

        // 仅在【注册】时需要的额外字段
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _mobileNumber;

        [ObservableProperty]
        private string _email; // 注册时专用的 Email 字段

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _isProcessing; // 通用的“处理中”状态，用于登录或注册
        [ObservableProperty]
        private bool _rememberUsername;

        [ObservableProperty]
        private bool _rememberPassword;
        // --- 命令 ---
        public ICommand ShowRegisterViewCommand { get; }
        public ICommand ShowLoginViewCommand { get; }
        public IAsyncRelayCommand LoginCommand { get; }
        public IAsyncRelayCommand RegisterCommand { get; }

        public AuthenticationViewModel(AuthService authService)
        {
            _authService = authService;

            // 初始化命令
            ShowRegisterViewCommand = new RelayCommand(() => IsRegistering = true);
            ShowLoginViewCommand = new RelayCommand(() => IsRegistering = false);
            LoginCommand = new AsyncRelayCommand(LoginAsync);
            RegisterCommand = new AsyncRelayCommand(RegisterAsync);
            //loginTest();
            //LoadLoginSettings();
        }

        public async Task loginTest()
        {
            Debug.WriteLine("Enter loginTest()");
            IsProcessing = true;
            ErrorMessage = string.Empty;

            // 登录时使用 Username 属性作为邮箱
            var (success, response, errorMessage) = await _authService.LoginAsync("281338225@qq.com", "123456789");

            IsProcessing = false;

            if (success)
            {
                MessageBox.Show($"Welcome back, {response.User.Name}!", "Login Successful");
                //CloseOverlay();
                SaveLoginSettings();
            }
            else
            {
                MessageBox.Show($"ri, {response?.User.Name}!", "Login failed");
                ErrorMessage = errorMessage; // 在界面上显示错误信息
            }
        }
        private async Task LoginAsync()
        {
            Debug.WriteLine("Enter LoginAsync()");
            IsProcessing = true;
            ErrorMessage = string.Empty;
            var (success, response, errorMessage) = await _authService.LoginAsync("281338225@qq.com", "123456789");
            //Debug.WriteLine($"{Email}");
            //var (success, response, errorMessage) = await _authService.LoginAsync(Email, Password);

            IsProcessing = false;

            if (success)
            {
                MessageBox.Show($"Welcome back, {response.User.Name}!", "Login Successful");
                CloseOverlay();
                SaveLoginSettings();
            }
            else
            {
                MessageBox.Show($"shit!", "Login failed");
                ErrorMessage = errorMessage; // 在界面上显示错误信息
            }
        }

        private async Task RegisterAsync()
        {
            IsProcessing = true;
            ErrorMessage = string.Empty;

            var (success, errorMessage) = await _authService.RegisterAsync(Name, MobileNumber, Email, Password);

            IsProcessing = false;

            if (success)
            {
                MessageBox.Show("Registration successful! You can now log in.", "Success");
                IsRegistering = false; // 注册成功后切换回登录视图
            }
            else
            {
                ErrorMessage = errorMessage;
            }
        }

        private void CloseOverlay()
        {
            if (Application.Current.MainWindow?.DataContext is MainViewModel mainVM)
            {
                mainVM.HideOverlayCommand?.Execute(null);
            }
        }

        private void LoadLoginSettings()
        {
            try
            {
                RememberUsername = Settings.Default.RememberUsername;
                if (RememberUsername)
                {
                    Username = Settings.Default.SavedUsername;
                }
            }
            catch (Exception ex)
            {
                // 在首次运行时，配置文件可能不存在，添加异常处理更健壮
                Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }
        }

        private void SaveLoginSettings()
        {
            // 在这里将数据保存到本地设置
            // 例如：
            Settings.Default.RememberUsername = RememberUsername;
            Settings.Default.SavedUsername = RememberUsername ? Username : string.Empty;

            // 别忘了还有 RememberPassword
            Settings.Default.RememberPassword = RememberPassword;

            Settings.Default.Save();
        }
    }
}