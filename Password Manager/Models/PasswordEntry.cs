using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Password_Manager.Models
{
    public class PasswordEntry : INotifyPropertyChanged
    {
        public string Service { get; set; }
        public string Username { get; set; }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayPassword));
                }
            }
        }

        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                if (_isPasswordVisible != value)
                {
                    _isPasswordVisible = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayPassword));
                }
            }
        }

        public string DisplayPassword => IsPasswordVisible ? Password : new string('•', Password?.Length ?? 0);

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
