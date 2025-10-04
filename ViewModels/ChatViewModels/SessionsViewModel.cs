using System.Collections.ObjectModel;

namespace NetEase.ViewModels.ChatViewModels
{
    public partial class SessionsViewModel : BaseViewModel
    {
        public ObservableCollection<ChatSession> Sessions { get; }
        public SessionsViewModel() { /* ... 填充 Sessions 的逻辑 ... */ }
    }
}
