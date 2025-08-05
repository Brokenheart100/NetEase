using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NetEase.Models
{
    public class SongTag
    {
        public string Text { get; set; }
        public Brush Background { get; set; }
        public Brush Foreground { get; set; }
        public Brush BorderBrush { get; set; }
    }
}
