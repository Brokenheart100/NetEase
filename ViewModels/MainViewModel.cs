using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.WindowsAPICodePack.Dialogs; // 引入文件夹选择对话框
using NetEase.Models;
using NetEase.ViewModels;
using NetEase.Views.Pages; // ViewModel 不应该直接引用 View
using System; // 这些 using 不再是必需的，可以清理
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TagLib; // 引入TagLib#来读取音频元数据

namespace NetEase.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        // --- 属性 ---
        [ObservableProperty]
        private BaseViewModel _currentView; // 用于承载【页面 ViewModel】

        [ObservableProperty]
        private bool _isOverlayVisible;

        // --- 子 ViewModel (通过构造函数注入) ---
        public TitleBarViewModel TitleBarVM { get; }
        public PlayerControlViewModel PlayerControlVM { get; }
        public AuthenticationViewModel AuthVM { get; }

        // --- 命令 ---
        public IRelayCommand NavigateFavoriteCommand { get; }
        public IRelayCommand NavigateLocalMusicCommand { get; }
        public ICommand ShowSignUpCommand { get; }
        public ICommand HideOverlayCommand { get; }


        public MainViewModel(TitleBarViewModel titleBarVM, PlayerControlViewModel playerControlVM, AuthenticationViewModel authVM)
        {
            // 1. 保存注入的常驻依赖
            TitleBarVM = titleBarVM;
            PlayerControlVM = playerControlVM;
            AuthVM = authVM;

            // 2. 初始化命令
            ShowSignUpCommand = new RelayCommand(() => IsOverlayVisible = true);
            HideOverlayCommand = new RelayCommand(() => IsOverlayVisible = false);

            NavigateFavoriteCommand = new RelayCommand(NavigateToFavorite);
            NavigateLocalMusicCommand = new RelayCommand(NavigateToLocalMusic);

            // 3. 设置默认视图
            NavigateToFavorite();
        }

        // --- 导航方法 ---
        private void NavigateToFavorite()
        {
            // 关键：在导航时，才从 DI 容器中动态地【创建】页面 ViewModel
            CurrentView = App.ServiceProvider.GetRequiredService<MyFavoriteMusicViewModel>();
        }

        private void NavigateToLocalMusic()
        {
            CurrentView = App.ServiceProvider.GetRequiredService<LocalMusicViewModel>();
        }

    }
}