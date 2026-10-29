using System.Linq;
using System.Windows;
using BanID.Services;

namespace BanID.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new BanIDDBEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == LoginBox.Text);
                if (user != null && EncryptionService.Decrypt(user.PasswordHash) == PasswordBox.Password)
                {
                    CaptchaWindow captcha = new CaptchaWindow(user);
                    captcha.Show();
                    this.Close();
                }
                else
                    MessageBox.Show("Неверный логин или пароль!");
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow register = new RegisterWindow();
            register.Show();
            this.Close();
        }
    }
}