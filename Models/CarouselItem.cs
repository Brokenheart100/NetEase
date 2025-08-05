using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Models
{
    public class CarouselItem
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string MainText { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; } // 专辑封面
        public string BackgroundImageUrl { get; set; } // 背景大图
        public string Tag { get; set; } // "新歌首发"
    }
}
