using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetEase.Models;
using NetEase.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using NetEase.Dtos; // 引入 DTO 命名空间

namespace NetEase.ViewModels.MusicRowContextMenu
{
    public partial class AddToPlaylistViewModel : ObservableObject
    {
        private readonly PlaylistService _playlistService;
        private readonly Song _songToAdd;

        // 这个集合将绑定到对话框的ListBox
        public ObservableCollection<Playlist> Playlists { get; } = new();

        // 用于从后台代码关闭窗口
        public Action CloseWindow { get; set; }

        public AddToPlaylistViewModel(PlaylistService playlistService, Song songToAdd)
        {
            _playlistService = playlistService;
            _songToAdd = songToAdd;
            LoadPlaylistsAsync();
        }

        private async void LoadPlaylistsAsync()
        {
            // 调用服务获取 DTO 列表
            var playlistDtos = await _playlistService.GetMyPlaylistsAsync();
            Debug.WriteLine($"Enter LoadPlaylistsAsync() {playlistDtos}");
            if (playlistDtos != null)
            {
                Playlists.Clear();
                foreach (var dto in playlistDtos)
                {
                    // 将 DTO 转换为前端的 Playlist Model
                    Playlists.Add(new Playlist { Id = dto.Id, Title = dto.Name });
                }
            }
        }

        [RelayCommand]
        private async Task AddToPlaylist(Playlist targetPlaylist)
        {
            if (targetPlaylist == null || _songToAdd == null) return;

       
            var (success, message) = await _playlistService.AddSongToPlaylistAsync(targetPlaylist.Id, _songToAdd.Id); // 修正点：应为 _songToAdd.Id

            if (success)
            {
                MessageBox.Show($"已成功将《{_songToAdd.Title}》添加到歌单“{targetPlaylist.Title}”");
                CloseWindow?.Invoke(); // 关闭窗口
            }
            else
            {
                MessageBox.Show($"添加失败: {message}");
            }
        }
    }
}