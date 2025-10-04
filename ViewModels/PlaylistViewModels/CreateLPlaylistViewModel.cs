using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace NetEase.ViewModels.PlaylistViewModels
{
    public partial class CreateLPlaylistViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string _title = "创建歌单";

        [ObservableProperty]
        private string _message;

        [ObservableProperty]
        private string _inputText;
        [ObservableProperty]
        private bool _isPrivate; // 用于绑定 CheckBox
        public string RemainingCharsText => $"{MaxLength - (InputText?.Length ?? 0)}";
        // 用于显示剩余可输入字数
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RemainingCharsText))]
        private int _maxLength = 40;


        public CreateLPlaylistViewModel()
        {

        }
        // 当 InputText 变化时，手动通知 RemainingCharsText 也变化
        partial void OnInputTextChanged(string value)
        {
            OnPropertyChanged(nameof(RemainingCharsText));
        }

        [RelayCommand]
        private void Ok(Window window)
        {
            if (window != null)
            {
                // 只有当用户输入了内容时，点击确定才有效
                if (!string.IsNullOrWhiteSpace(InputText))
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
        }

        [RelayCommand]
        private static void Cancel(Window window)
        {
            if (window != null)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
    }
}
