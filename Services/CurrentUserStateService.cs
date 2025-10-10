using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using NetEase.Dtos;

namespace NetEase.Services
{
    public partial class CurrentUserStateService : ObservableObject
    {
        [ObservableProperty]
        private bool _isLoggedIn;

        [ObservableProperty]
        private int _userId;

        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private string _email;

        // 【核心】我们在这里管理头像的 BitmapImage
        [ObservableProperty]
        private BitmapImage _avatar;

        private static readonly BitmapImage DefaultAvatar =
            new BitmapImage(new Uri("pack://application:,,,/CoverImage/1.jpg"));

        public CurrentUserStateService()
        {
            // 初始化时，用户未登录，显示默认头像
            ClearState();
        }

        /// <summary>
        /// 当用户成功登录时，调用此方法来更新全局状态
        /// </summary>
        public void SetLoggedInUser(UserDto user, string token)
        {
            if (user == null) return;

            IsLoggedIn = true;
            UserId = user.Id;
            UserName = user.Name;
            Email = user.Email;

            UpdateAvatar(user.AvatarUrl);
        }

        /// <summary>
        /// 当用户登出时，调用此方法来清除所有状态
        /// </summary>
        public void ClearState()
        {
            IsLoggedIn = false;
            UserId = 0;
            UserName = "未登录";
            Email = string.Empty;
            Avatar = DefaultAvatar;
        }

        /// <summary>
        /// 异步更新头像（这个方法也可以保留在这里）
        /// </summary>
        private void UpdateAvatar(string avatarUrl)
        {
            if (string.IsNullOrEmpty(avatarUrl))
            {
                Avatar = DefaultAvatar;
                return;
            }

            try
            {
                var newAvatar = new BitmapImage();
                newAvatar.BeginInit();
                newAvatar.UriSource = new Uri(avatarUrl);
                newAvatar.CacheOption = BitmapCacheOption.OnLoad;
                newAvatar.EndInit();
                Avatar = newAvatar;
            }
            catch
            {
                Avatar = DefaultAvatar;
            }
        }
    }
}
