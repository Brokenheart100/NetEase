using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Dtos
{
    // 这个类必须与后端 API 返回的 JSON 对象结构完全匹配
    public class PlaylistSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CoverImageUrl { get; set; }
    }
}
