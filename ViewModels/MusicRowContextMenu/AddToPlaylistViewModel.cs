using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetEase.Models;
using NetEase.Services;

namespace NetEase.ViewModels.MusicRowContextMenu
{
    public partial class AddToPlaylistViewModel : BaseViewModel
    {
        private readonly PlaylistService _playlistService;
        private readonly Song _songToAdd; // 要被添加的歌曲

        [ObservableProperty]
        private bool _isLoading = true;

        public ObservableCollection<Playlist> UserPlaylists { get; } = new();

        // 当用户在列表中选择一个播放列表时，执行此命令
        public IAsyncRelayCommand<Playlist> AddToPlaylistCommand { get; }

        public AddToPlaylistViewModel(PlaylistService playlistService, Song songToAdd)
        {
            _playlistService = playlistService;
            _songToAdd = songToAdd;

            AddToPlaylistCommand = new AsyncRelayCommand<Playlist>(AddToPlaylistAsync);
            LoadUserPlaylistsAsync();
        }

        private async Task LoadUserPlaylistsAsync()
        {
            IsLoading = true;
            // 从 API 加载当前用户的所有播放列表
            var playlists = await _playlistService.GetMyPlaylistsAsync();
            if (playlists != null)
            {
                UserPlaylists.Clear();
                foreach (var p in playlists)
                {
                    // 将 DTO 转换为本地模型
                    UserPlaylists.Add(new Playlist { Id = p.Id, Title = p.Name, CoverImageUrl = p.CoverImageUrl });
                }
            }
            IsLoading = false;
        }

        private async Task AddToPlaylistAsync(Playlist selectedPlaylist)
        {
            if (selectedPlaylist == null || _songToAdd == null) return;

            // 调用服务，将 _songToAdd.Id 添加到 selectedPlaylist.Id
            var (success, errorMessage) = await _playlistService.AddSongToPlaylistAsync(selectedPlaylist.Id, _songToAdd.Index);

            if (success)
            {
                // Growl.Success("添加成功！");
                // TODO: 关闭窗口
            }
            else
            {
                // Growl.Error($"添加失败: {errorMessage}");
            }
        }
    }
}
