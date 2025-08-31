using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Models
{
    public class SavedUserProfile
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; } // 将来可以保存真实的头像URL
    }
}
