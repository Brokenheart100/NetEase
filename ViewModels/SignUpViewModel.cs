// 引入MVVM工具包核心类（ObservableObject实现属性通知，RelayCommand实现命令绑定）
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32; // 引入文件对话框
// 引入登录/注册相关的数据传输对象（DTO）
using NetEase.Dtos;
using NetEase.Models;
// 引入应用程序设置（用于保存"记住用户名"等配置）
using NetEase.Properties;
// 引入授权服务（处理实际的登录/注册业务逻辑）
using NetEase.Services;
// 系统基础类（事件、调试、任务、窗口等）
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

// 命名空间：视图模型层（处理登录/注册相关的UI逻辑）
namespace NetEase.ViewModels
{
    public enum AuthViewState
    {
        ProfileSelection, // 用户选择界面
        Login,            // 登录表单
        Register          // 注册表单
    }
    /// <summary>
    /// 登录成功事件的参数类：用于在登录成功时传递用户信息
    /// 继承EventArgs，符合.NET事件参数规范
    /// </summary>
    public class LoginSuccessEventArgs : EventArgs
    {
        /// <summary>
        /// 登录成功后返回的用户信息（包含用户ID、名称等）
        /// </summary>
        public LoginResponse UserLoginInfo { get; }

        /// <summary>
        /// 构造函数：初始化登录成功事件参数
        /// </summary>
        /// <param name="loginInfo">登录响应数据（从服务层获取）</param>
        public LoginSuccessEventArgs(LoginResponse loginInfo)
        {
            UserLoginInfo = loginInfo;
        }
    }

    /// <summary>
    /// 认证视图模型：处理登录、注册的UI逻辑，绑定到登录/注册界面
    /// 继承BaseViewModel（自定义基类，可能包含公共属性如加载状态等）
    /// </summary>
    public partial class SignUpViewModel : BaseViewModel
    {
        // 依赖注入的授权服务：实际处理登录/注册的业务逻辑（与后端API交互）
        private readonly AuthService _authService;
        private readonly UserProfileService _profileService; // 注入新服务
        private readonly CredentialService _credentialService; // <-- 注入新服务

        /// <summary>
        /// 登录成功事件：当登录成功时触发，通知其他组件（如主窗口）切换状态
        /// </summary>
        public event EventHandler<LoginSuccessEventArgs> LoginSuccess;
        [ObservableProperty]
        private AuthViewState _currentState = AuthViewState.ProfileSelection;
        public ObservableCollection<SavedUserProfile> SavedUsers { get; } = new();


        // --- 表单数据属性（绑定到UI输入框，存储用户输入） ---
        /// <summary>
        /// 邮箱地址（登录和注册的共用账号字段，绑定到邮箱输入框）
        /// </summary>
        [ObservableProperty] private string _email;

        /// <summary>
        /// 密码（绑定到密码输入框，登录和注册均需）
        /// </summary>
        [ObservableProperty] private string _password;

        /// <summary>
        /// 用户名（仅注册时需要，绑定到注册表单的姓名输入框）
        /// </summary>
        [ObservableProperty] private string _name;

        /// <summary>
        /// 手机号码（仅注册时需要，绑定到注册表单的手机号输入框）
        /// </summary>
        [ObservableProperty] private string _mobileNumber;

        /// <summary>
        /// 错误信息（绑定到UI的错误提示区域，显示登录/注册失败原因）
        /// </summary>
        [ObservableProperty] private string _errorMessage;

        /// <summary>
        /// 是否正在处理（绑定到UI的加载指示器，登录/注册过程中显示加载状态）
        /// </summary>
        [ObservableProperty] private bool _isProcessing;
        /// <summary>
        /// 用于在UI上实时预览用户选择的头像
        /// </summary>
        [ObservableProperty]
        private string _avatarPreview;

        /// <summary>
        /// 存储用户选择的头像文件的完整本地路径
        /// </summary>
        private string _selectedAvatarFilePath;
        /// <summary>
        /// 是否记住用户名（绑定到"记住我"复选框，控制是否保存邮箱到本地设置）
        /// </summary>
        [ObservableProperty] private bool _rememberUsername;
        [ObservableProperty] private bool _rememberPassword;
        /// <summary>
        /// 构造函数：通过依赖注入初始化授权服务，加载登录设置，触发自动登录
        /// </summary>
        /// <param name="authService">授权服务实例（由DI容器注入）</param>
        public SignUpViewModel(AuthService authService, UserProfileService profileService, CredentialService credentialService)
        {
            _authService = authService;
            _profileService = profileService;
            _credentialService = credentialService;
            LoadSavedProfiles();
            LoadLoginSettings();

            // 自动执行登录（可能用于"记住登录状态"的场景，此处暂时直接调用）
            _ = Login();
        }
        [RelayCommand]
        private void ChooseAvatar()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择头像图片",
                Filter = "图片文件 (*.jpg; *.jpeg; *.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // 用户选择了文件
                _selectedAvatarFilePath = openFileDialog.FileName;

                // 更新 AvatarPreview 属性，UI上的ImageBrush会自动刷新显示这张图片
                AvatarPreview = _selectedAvatarFilePath;
            }
        }
        private void LoadSavedProfiles()
        {
            SavedUsers.Clear();
            var profiles = _profileService.LoadProfiles();
            if (profiles.Count != 0)
            {
                foreach (var profile in profiles)
                {
                    SavedUsers.Add(profile);
                }
                CurrentState = AuthViewState.ProfileSelection;
            }
            else
            {
                // 如果没有已保存的用户，直接显示登录界面
                CurrentState = AuthViewState.Login;
            }
        }

        [RelayCommand]
        private void Close()
        {
            CloseOverlay();
        }

        /// <summary>
        /// 显示注册视图命令：绑定到"注册"按钮，点击后切换到注册表单
        /// </summary>
        [RelayCommand]
        private void ShowRegisterView()
        {
            CurrentState = AuthViewState.Register;
        }

        /// <summary>
        /// 显示登录视图命令：绑定到"登录"按钮（注册表单中），点击后切换回登录表单
        /// </summary>
        [RelayCommand]
        private void ShowLoginView()
        {
            CurrentState = AuthViewState.Login;
        }

        /// <summary>
        /// 登录命令：绑定到登录表单的"登录"按钮，异步处理登录逻辑
        /// </summary>
        [RelayCommand]
        private async Task Login()
        {
            Debug.WriteLine("进入Login()");
            // 登录过程中：设置处理状态（UI显示加载），清空之前的错误信息
            IsProcessing = true;
            ErrorMessage = string.Empty;
            // 调用授权服务的登录方法（此处暂时硬编码测试账号，实际应使用用户输入的Email和Password）
            //var (success, response, errorMessage) = await _authService.LoginAsync(Email, Password);
            var (success, response, errorMessage) = await _authService.LoginAsync("test@test.com", "123456");

            if (success)
            {
                // 登录成功：触发LoginSuccess事件（通知主窗口切换界面）
                LoginSuccess?.Invoke(this, new LoginSuccessEventArgs(response));
                _profileService.SaveProfile(response);
                // 显示欢迎消息框
                MessageBox.Show($"欢迎回来，{response.User.Name}！", "登录成功");
                // 保存登录设置（如是否记住用户名）
                SaveLoginSettings();

                // 关闭登录/注册覆盖层（回到主界面）
                CloseOverlay();
            }
            else
            {
                // 登录失败：显示错误消息（消息框和UI错误提示区）
                MessageBox.Show($"登录失败：{errorMessage}", "登录失败");
                Debug.WriteLine($"登录失败：{errorMessage}");

                ErrorMessage = errorMessage;
            }
        }
        // 【新增】当用户点击一个已保存的头像时
        [RelayCommand]
        private void SelectProfile(SavedUserProfile profile)
        {
            if (profile == null) return;
            // 自动填充邮箱，并切换到登录表单
            Email = profile.Email;
            Password = string.Empty; // 清空密码框
            CurrentState = AuthViewState.Login;
        }

        // 【新增】当用户点击"+"号或需要用新账号登录时
        [RelayCommand]
        private void ShowLoginForNewUser()
        {
            Email = string.Empty;
            Password = string.Empty;
            CurrentState = AuthViewState.Login;
        }
        /// <summary>
        /// 注册命令：绑定到注册表单的"注册"按钮，异步处理注册逻辑
        /// </summary>
        [RelayCommand]
        private async Task Register()
        {
            // 注册过程中：设置处理状态（UI显示加载），清空之前的错误信息
            IsProcessing = true;
            ErrorMessage = string.Empty;

            // 调用授权服务的注册方法（传递用户输入的注册信息）
            //var (success, errorMessage) = await _authService.RegisterAsync(Name, MobileNumber, Email, Password);
            var (success, errorMessage) = await _authService.RegisterAsync(
               Name,
               MobileNumber,
               Email,
               Password,
               _selectedAvatarFilePath // <-- 新增参数
           );
            // 注册处理完成：关闭加载状态
            IsProcessing = false;

            if (success)
            {
                // 注册成功：显示成功消息，切换回登录视图
                MessageBox.Show("注册成功！您可以登录了。", "成功");
                CurrentState = AuthViewState.Login;
                AvatarPreview = null;
                _selectedAvatarFilePath = null;

            }
            else
            {
                // 注册失败：显示错误信息（UI错误提示区）
                ErrorMessage = errorMessage;
            }
        }

        // --- 私有辅助方法 ---

        /// <summary>
        /// 关闭登录/注册覆盖层：通知主窗口隐藏登录界面，显示主内容
        /// </summary>
        private void CloseOverlay()
        {
            // 获取主窗口的DataContext（假设是MainViewModel）
            if (Application.Current.MainWindow?.DataContext is MainViewModel mainVM)
            {
                // 执行主视图模型的隐藏覆盖层命令（关闭登录界面）
                mainVM.HideOverlayCommand?.Execute(null);
            }
        }

        /// <summary>
        /// 加载登录设置：从应用程序配置中读取"记住用户名"选项和保存的邮箱
        /// </summary>
        private void LoadLoginSettings()
        {
            // 从旧的配置文件加载“记住用户名”选项
            try
            {
                // 1. 加载“记住用户名”选项和已保存的Email
                RememberUsername = Settings.Default.RememberUsername;
                if (RememberUsername)
                {
                    Email = Settings.Default.SavedUsername;
                }

                // 【核心修正】
                // 2. 如果Email被成功加载，就直接尝试为这个Email获取密码
                if (!string.IsNullOrEmpty(Email))
                {
                    string password = _credentialService.GetPasswordForEmail(Email);
                    if (!string.IsNullOrEmpty(password))
                    {
                        // 如果成功获取到密码
                        RememberPassword = true;
                        Password = password; // 填充密码框
                    }
                    else
                    {
                        // 找不到密码，确保“记住密码”是未勾选状态
                        RememberPassword = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载设置失败：{ex.Message}");
            }
        }
        [RelayCommand]
        private async Task AutoLogin(SavedUserProfile profile)
        {
            Debug.WriteLine($"进入AutoLogin(){profile}");
            if (profile == null) return;

            // 1. 尝试从安全存储中获取该用户的密码
            string password = _credentialService.GetPasswordForEmail(profile.Email);

            if (string.IsNullOrEmpty(password))
            {
                // 如果找不到密码（例如，用户上次登录时没有勾选“记住密码”），
                // 就执行原来的逻辑：跳转到登录页面让用户手动输入密码。
                SelectProfile(profile);
                return;
            }

            // 2. 如果找到了密码，直接使用它和用户的Email发起登录
            IsProcessing = true;
            ErrorMessage = string.Empty;

            var (success, response, errorMessage) = await _authService.LoginAsync(profile.Email, password);

            IsProcessing = false;

            if (success)
            {
                // 登录成功后的逻辑与手动登录完全一样
                LoginSuccess?.Invoke(this, new LoginSuccessEventArgs(response));
                _profileService.SaveProfile(response); // 刷新用户信息

                // 确保“记住用户名”和“记住密码”的状态也保存
                RememberUsername = true;
                RememberPassword = true;
                Email = profile.Email; // 更新Email字段以便SaveLoginSettings能正确保存
                Password = password; // 更新Password字段
                SaveLoginSettings();

                CloseOverlay();
            }
            else
            {
                // 如果自动登录失败（例如，密码已在别处更改），
                // 则跳转到登录页面，并提示错误。
                SelectProfile(profile);
                ErrorMessage = $"自动登录失败: {errorMessage}";
            }
        }

        /// <summary>
        /// 保存登录设置：将"记住用户名"选项和邮箱保存到应用程序配置
        /// </summary>
        private void SaveLoginSettings()
        {

            // 保存“记住用户名”选项到旧的配置文件
            Properties.Settings.Default.RememberUsername = RememberUsername;
            Properties.Settings.Default.SavedUsername = RememberUsername ? Email : string.Empty;
            Properties.Settings.Default.Save();

            // 根据“记住密码”选项来保存或清除安全凭据
            if (RememberPassword && RememberUsername)
            {
                // 只有在“记住用户名”也勾选时才保存密码才有意义
                _credentialService.SaveCredentials(Email, Password);
            }
            else
            {
                _credentialService.ClearCredentials();
            }
        }
    }
}