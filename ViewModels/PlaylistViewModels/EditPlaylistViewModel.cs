using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using NetEase.Dtos;
using NetEase.Models;
using NetEase.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace NetEase.ViewModels.PlaylistViewModels
{
    public partial class EditPlaylistViewModel : BaseViewModel
    {
        private readonly PlaylistService _playlistService;
        private Playlist _originalPlaylist; // 保存原始歌单数据，用于取消操作
        // --- UI状态 ---
        [ObservableProperty]
        private bool _isBusy; // 用于在保存或上传时显示加载指示器

        // --- 表单数据 ---
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private string _coverImageUrl;

        [ObservableProperty]
        private Tag _selectedTag;

        private string _newCoverImageRelativePath;


        public ObservableCollection<Tag> AvailableTags { get; } = [];

        public string NameCharCount => $"{Name?.Length ?? 0}/40";
        public string DescriptionCharCount => $"{Description?.Length ?? 0}/1000";
        // 用于通知 MainViewModel 导航回上一个页面
        public event Action NavigationRequestCompleted;

        public EditPlaylistViewModel(PlaylistService playlistService)
        {
            _playlistService = playlistService;
        }
        [RelayCommand(CanExecute = nameof(CanChangeCover))]
        private async Task ChangeCoverAsync()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择新的封面图片",
                Filter = "图片文件 (*.jpg; *.jpeg; *.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                IsBusy = true; // 显示加载状态
                SaveCommand.NotifyCanExecuteChanged();
                ChangeCoverCommand.NotifyCanExecuteChanged();
                try
                {
                    // 调用服务上传新封面
                    var relativePath = await _playlistService.UploadCoverAsync(openFileDialog.FileName);

                    if (!string.IsNullOrEmpty(relativePath))
                    {
                        // 1. 保存【相对路径】以备保存时使用
                        _newCoverImageRelativePath = relativePath;

                        // 2. 更新UI预览
                        //    需要一个方法来获取API网关的基地址
                        var gatewayBaseUrl = "http://localhost:5240"; // 应该从配置中读取
                        CoverImageUrl = $"{gatewayBaseUrl}/media/{relativePath}";
                    }
                    else
                    {
                        MessageBox.Show("封面上传失败。");
                    }
                }
                finally
                {
                    IsBusy = false; // 隐藏加载状态
                    SaveCommand.NotifyCanExecuteChanged();
                    ChangeCoverCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private bool CanChangeCover() => !IsBusy;
        private static string GetRelativePathFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
           
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                // uri.AbsolutePath 会返回 "/media/covers/guid.jpg"
                const string prefix = "/media/";
                if (uri.AbsolutePath.StartsWith(prefix))
                {
                    return uri.AbsolutePath.Substring(prefix.Length);
                }
            }
            return url; // 如果格式不符或解析失败，返回原始值
        }
        /// <summary>
        /// 核心方法：加载要编辑的歌单数据
        /// </summary>
        public void LoadPlaylist(Playlist playlistToEdit)
        {
            _originalPlaylist = playlistToEdit;
            Name = playlistToEdit.Title;
            Description = playlistToEdit.Description; // 确保Playlist模型有Description属性
            CoverImageUrl = playlistToEdit.CoverImageUrl;
            _newCoverImageRelativePath = null; // 每次加载时重置
        }

        partial void OnNameChanged(string value) => OnPropertyChanged(nameof(NameCharCount));
        partial void OnDescriptionChanged(string value) => OnPropertyChanged(nameof(DescriptionCharCount));

        private bool CanExecuteAsyncCommands() => !IsBusy;
        [RelayCommand(CanExecute = nameof(CanExecuteAsyncCommands))]
        private async Task SaveAsync()
        {
            IsBusy = true;
            SaveCommand.NotifyCanExecuteChanged(); // 通知UI更新按钮状态
            ChangeCoverCommand.NotifyCanExecuteChanged();
            try
            {
                var updateDto = new UpdatePlaylistDto
                {
                    Name = this.Name,
                    Description = this.Description,
                    CoverImageUrl = _newCoverImageRelativePath ?? GetRelativePathFromUrl(_originalPlaylist.CoverImageUrl)
                };

                var success = await _playlistService.UpdatePlaylistAsync(_originalPlaylist.Id, updateDto);

                Debug.WriteLine($"SaveAsync:{success}");
                if (success)
                {
                    // 更新 MainViewModel 中缓存的歌单列表项
                    _originalPlaylist.Title = this.Name;
                    _originalPlaylist.Description = this.Description;
                    _originalPlaylist.CoverImageUrl = this.CoverImageUrl; // UI已是最新

                    MessageBox.Show("歌单信息已保存！");
                    NavigationRequestCompleted?.Invoke();
                }
                else
                {
                    MessageBox.Show("保存失败，请稍后重试。");

                }
            }
            finally
            {
                IsBusy = false;
                SaveCommand.NotifyCanExecuteChanged();
                ChangeCoverCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            // 直接请求导航回上一个页面，不保存任何更改
            NavigationRequestCompleted?.Invoke();
        }


    }
}
