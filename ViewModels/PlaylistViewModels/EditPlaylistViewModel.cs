using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetEase.Models;
using NetEase.Services;
using System.Collections.ObjectModel;

namespace NetEase.ViewModels.PlaylistViewModels
{
    public partial class EditPlaylistViewModel : BaseViewModel
    {
        private readonly PlaylistService _playlistService;
        private Playlist _originalPlaylist; // 保存原始歌单数据，用于取消操作

        [ObservableProperty] private string _name;
        [ObservableProperty] private string _description;
        [ObservableProperty] private string _coverImageUrl;
        [ObservableProperty] private Tag _selectedTag;

        public ObservableCollection<Tag> AvailableTags { get; } = new();

        public string NameCharCount => $"{Name?.Length ?? 0}/40";
        public string DescriptionCharCount => $"{Description?.Length ?? 0}/1000";

        // 用于通知 MainViewModel 导航回上一个页面
        public event System.Action NavigationRequestCompleted;

        public EditPlaylistViewModel(PlaylistService playlistService)
        {
            _playlistService = playlistService;
            // 可以在这里加载可选的标签
            // AvailableTags.Add(new Tag { Name = "华语" });
        }

        /// <summary>
        /// 核心方法：加载要编辑的歌单数据
        /// </summary>
        public void LoadPlaylist(Playlist playlistToEdit)
        {
            _originalPlaylist = playlistToEdit;
            Name = playlistToEdit.Title;
            // Description = playlistToEdit.Description; // 假设Playlist模型有这个属性
            CoverImageUrl = playlistToEdit.CoverImageUrl;
        }

        partial void OnNameChanged(string value) => OnPropertyChanged(nameof(NameCharCount));
        partial void OnDescriptionChanged(string value) => OnPropertyChanged(nameof(DescriptionCharCount));

        [RelayCommand]
        private async Task SaveAsync()
        {
            // 1. 构建更新DTO
            // var updateDto = new UpdatePlaylistDto { Name = this.Name, Description = this.Description, ... };

            // 2. 调用服务更新后端
            // bool success = await _playlistService.UpdatePlaylistAsync(_originalPlaylist.Id, updateDto);

            // 3. (可选) 更新 MainViewModel 中缓存的歌单列表项
            _originalPlaylist.Title = this.Name;

            // 4. 请求导航回上一个页面
            NavigationRequestCompleted?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            // 直接请求导航回上一个页面，不保存任何更改
            NavigationRequestCompleted?.Invoke();
        }

        [RelayCommand]
        private void ChangeCover()
        {
            // 这里的逻辑与更换用户头像类似，打开文件选择框，调用服务上传新封面
        }
    }
}
