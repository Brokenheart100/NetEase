using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.Models
{
    public partial class NavigationItem : ObservableObject
    {
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public Type ViewModelType { get; set; }

        [ObservableProperty]
        private bool _isSelected;
    }
}
