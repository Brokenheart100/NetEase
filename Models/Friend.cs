using CommunityToolkit.Mvvm.ComponentModel;

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
