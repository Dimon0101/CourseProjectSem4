using System.Windows;

namespace CourseProjectSem4
{
    public partial class ChangePasswordDialog : Window
    {
        public string NewPassword { get; private set; } = "";

        public ChangePasswordDialog()
        {
            InitializeComponent();
            Loaded += (_, _) => CurrentPasswordBox.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string current  = CurrentPasswordBox.Password;
            string newPass  = NewPasswordBox.Password;
            string confirm  = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(current))
            { ShowError("Введіть поточний пароль."); return; }

            if (!AdminSession.Instance.VerifyPassword(current))
            { ShowError("Поточний пароль невірний."); return; }

            if (string.IsNullOrWhiteSpace(newPass))
            { ShowError("Введіть новий пароль."); return; }

            if (newPass.Length < 4)
            { ShowError("Новий пароль має бути не менше 4 символів."); return; }

            if (newPass != confirm)
            { ShowError("Паролі не співпадають."); return; }

            AdminSession.Instance.TryChangePassword(current, newPass);
            NewPassword  = newPass;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;

        private void ShowError(string msg)
        {
            ErrorLabel.Text       = msg;
            ErrorLabel.Visibility = Visibility.Visible;
        }
    }
}
