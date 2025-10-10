using System;
using System.Collections.Generic;
using System.Text;

namespace NetEase.Models
{
    // 用于请求导航到编辑页面的消息
    public class NavigateToEditPlaylistMessage
    {
        public Playlist PlaylistToEdit { get; }
        public NavigateToEditPlaylistMessage(Playlist playlist) => PlaylistToEdit = playlist;
    }
    /// <summary>
    /// 请求导航回上一个页面的消息
    /// </summary>
    public class GoBackNavigationMessage { }

    /// <summary>
    /// 当歌单信息被成功更新后，广播此消息以同步UI
    /// </summary>
    public class PlaylistUpdatedMessage
    {
        public int PlaylistId { get; set; }
        public string NewName { get; set; }
        public string NewDescription { get; set; }
        public string NewCoverImageUrl { get; set; } // 必须是完整的HTTP URL
    }
}
        