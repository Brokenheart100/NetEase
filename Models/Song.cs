using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NetEase.Models
{
    public partial class Song : ObservableObject
    {
        public int Id { get; set; } 
        public int Index { get; set; }
        //图片路径
        public string CoverImageUrl { get; set; }
        public string Title { get; set; } = "Default";
        public string Subtitle { get; set; } 
        public List<SongTag> Tags { get; set; }
        public string Artist { get; set; } = "DefaultSinger";
        public string Album { get; set; }
        public bool IsLiked { get; set; }=false;
        public string Duration { get; set; }
        public string FilePath { get; set; }
        public string AlbumTitle { get; set; }
        public string ArtistName { get; set; }
        //图片
        public ImageSource CoverImage { get; set; }
        [ObservableProperty]
        private bool _isPlaying;
        [ObservableProperty]
        private bool _isDownloaded;
        [ObservableProperty]
        private bool _isLike;

    }
    public class SongTag
    {
        public string Text { get; set; } = "default";
        public Brush Background { get; set; }
        public Brush Foreground { get; set; }
        public Brush BorderBrush { get; set; }
    }

}
