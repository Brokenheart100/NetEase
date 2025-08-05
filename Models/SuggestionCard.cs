using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Models
{
    public class SuggestionCard
    {
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string Tag { get; set; } // "播客" 或 "歌单"
    }
}
