using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEase.ViewModels
{
    public partial class SessionsViewModel : BaseViewModel
    {
        public ObservableCollection<ChatSession> Sessions { get; }
        public SessionsViewModel() { /* ... 填充 Sessions 的逻辑 ... */ }
    }
}
