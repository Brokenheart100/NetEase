using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using NetEase.Services;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NetEase.Models
{
    public class SavedUserProfile
    {
        public string Name { get; set; }
        public string Email { get; set; }

        //[ObservableProperty]
        public string? AvatarUrl { get; set; } // 将来可以保存真实的头像URL

        //[ObservableProperty]
        //[JsonIgnore]
        //private ImageSource? _avatarImage; // 这是【UI】绑定的图片源
    }
}
