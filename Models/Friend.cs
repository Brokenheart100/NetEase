using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NetEase.Converters.RandomNumber;

namespace NetEase.Models
{
    public partial class Friend : ObservableObject
    {
        public int Id { get; set; }
        [ObservableProperty] 
        private string _name;
    
        [ObservableProperty] 
        private string _avatarUrl;
        [ObservableProperty] 
        private bool _isOnline;
    }
}
