using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace CourseProjectSem4
{
    class AdminSession : INotifyPropertyChanged
    {
        public static AdminSession Instance { get; } = new();
        private AdminSession() { }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        private string _password = "admin123";

        public void LoadPassword(string fromDb) => _password = fromDb;

        private bool _isAdmin;
        public bool IsAdmin
        {
            get => _isAdmin;
            private set
            {
                _isAdmin = value;
                Notify();
                Notify(nameof(ModeLabel));
                Notify(nameof(BadgeColor));
                Notify(nameof(ToggleLabel));
            }
        }

        public string ModeLabel   => _isAdmin ? "Адміністратор" : "Режим: Гість";
        public string ToggleLabel => _isAdmin ? "Вийти"         : "Увійти";
        public Color  BadgeColor  => _isAdmin
            ? Color.FromRgb(39, 174, 96)
            : Color.FromRgb(96, 125, 139);

        public bool TryLogin(string password)
        {
            if (password != _password) return false;
            IsAdmin = true;
            return true;
        }

        public bool VerifyPassword(string password) => password == _password;

        public bool TryChangePassword(string current, string newPassword)
        {
            if (current != _password) return false;
            _password = newPassword;
            return true;
        }

        public void Logout() => IsAdmin = false;
    }
}
