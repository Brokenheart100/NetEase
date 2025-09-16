using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Models
{
    public partial class Comment : ObservableObject
    {
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        public string Content { get; set; }
        public string Timestamp { get; set; }
    
        [ObservableProperty]
        private int _likeCount;

        [ObservableProperty]
        private bool _isLiked;
        public string VipLevel { get; set; }
    }
}
