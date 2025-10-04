using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetEase.Models;
using NetEase.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace NetEase.ViewModels
{
    public partial class SearchResultViewModel : BaseViewModel
    {
        private readonly SearchService _searchService;
        private readonly PlayerService _playerService;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _searchQuery;

        [ObservableProperty]
        private string _resultCountText;

        public ObservableCollection<Song> Songs { get; } = [];

        public SearchResultViewModel(SearchService searchService, PlayerService playerService)
        {
            _searchService = searchService;
            _playerService = playerService;
        }

        /// <summary>
        /// 核心方法：执行搜索。由外部（如MainViewModel）调用。
        /// </summary>
        public async Task PerformSearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return;

            IsLoading = true;
            SearchQuery = query;
            Songs.Clear();
            ResultCountText = ""; // 清空上次结果

            try
            {
                var searchResultDto = await _searchService.SearchAsync(query, "song");
                if (searchResultDto?.Results?.Songs?.Items != null)
                {
                    int index = 1;
                    foreach (var songDto in searchResultDto.Results.Songs.Items)
                    {
                        // 将后端返回的 DTO 转换为前端的 Song 模型
                        Songs.Add(new Song
                        {
                            Id = songDto.Id,
                            Index = index++,
                            Title = songDto.Title,
                            Artist = songDto.ArtistName,
                            Album = songDto.AlbumTitle,
                            Duration = songDto.Duration,
                            CoverImageUrl = songDto.CoverImageUrl,
                            FilePath = songDto.FilePath,
                        });
                        Debug.WriteLine($"Loaded song from Search result : {songDto.CoverImageUrl} by {songDto.FilePath} Duration：{songDto.Duration}");

                    }
                    ResultCountText = $"找到 {Songs.Count} 首单曲";
                }
                else
                {
                    ResultCountText = "未能找到相关的单曲";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PerformSearchAsync({query}) 错误 {ex}");
                // 最好能记录日志
                ResultCountText = "搜索时发生错误";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void PlaySong(Song song)
        {
            Debug.WriteLine($"PlaySong {song.Title} FilePath:{song.FilePath}");
            if (song != null)
            {
                _playerService.StartPlayback(song, Songs);
            }
        }
        [ObservableProperty]
        private string _noResultsMessage = "正在搜索，请稍候...";
        public void UpdateResults(List<Song> songs)
        {
            Songs.Clear();
            if (songs != null && songs.Count > 0)
            {
                foreach (var song in songs)
                {
                    Songs.Add(song);
                }
            }
            else
            {
                NoResultsMessage = "未能找到相关的单曲";
            }
        }
    }
}
