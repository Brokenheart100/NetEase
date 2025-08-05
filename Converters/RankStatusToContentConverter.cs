using NetEase.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace NetEase.Converters
{
    public class StatusDisplay
    {
        public string Text { get; set; }
        public Brush Color { get; set; }
    }

    public class RankStatusToContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RankStatus status)
            {
                return status switch
                {
                    RankStatus.New => new StatusDisplay { Text = "新", Color = Brushes.LawnGreen },
                    RankStatus.Up => new StatusDisplay { Text = "▲", Color = Brushes.Red },
                    RankStatus.Down => new StatusDisplay { Text = "▼", Color = Brushes.CornflowerBlue },
                    _ => new StatusDisplay { Text = "", Color = Brushes.Transparent }
                };
            }

            return new StatusDisplay { Text = "", Color = Brushes.Transparent };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
