using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Models
{
    public partial class Comment : ObservableObject
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        public string Content { get; set; }
        public string Timestamp { get; set; }
        public DateTime CreatedAt { get; set; }
        [ObservableProperty]
        private int _likeCount;

        [ObservableProperty]
        private bool _isLiked;
        public bool IsVip { get; set; }
        public string VipLevel { get; set; }
        // (可选) 为了显示 "5分钟前" 这样的格式
        public string TimeAgo => GetTimeAgo(CreatedAt);

        // (可选) 用于未来显示回复
        public ObservableCollection<Comment> Replies { get; } = new();
        // ===============================================================

        private string GetTimeAgo(DateTime dateTime)
        {
            // 实现一个计算时间差的辅助方法
            var timeSpan = DateTime.UtcNow - dateTime;
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}分钟前";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}小时前";
            return dateTime.ToLocalTime().ToString("yyyy-MM-dd");
        }
    }
}
