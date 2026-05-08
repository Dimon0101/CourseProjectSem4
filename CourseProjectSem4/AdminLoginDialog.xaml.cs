using System.Windows;
using System.Windows.Input;

namespace CourseProjectSem4
{
    public partial class AdminLoginDialog : Window
    {
        public string Password => PasswordBox.Password;

        public AdminLoginDialog()
        {
            InitializeComponent();
            Loaded += (_, _) => PasswordBox.Focus();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ErrorLabel.Text       = "Введіть пароль.";
                ErrorLabel.Visibility = Visibility.Visible;
                return;
            }
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Login_Click(sender, e);
        }
    }
}
