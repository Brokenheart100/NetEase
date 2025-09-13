using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; } // 【确保】有这个属性
    }
}
