using System.Windows;
using BanID.Services;

namespace BanID.Views
{
    public partial class CaptchaWindow : Window
    {
        private Users user;
        private string captchaText;

        public CaptchaWindow(Users user)
        {
            InitializeComponent();
            this.user = user;
            GenerateNewCaptcha();
        }

        private void GenerateNewCaptcha()
        {
            captchaText = CaptchaService.GenerateCaptchaText();
            CaptchaTextBlock.Text = captchaText;
        }

        private void Verify_Click(object sender, RoutedEventArgs e)
        {
            if (CaptchaService.Verify(CaptchaInput.Text, captchaText))
            {
                PersonalAccountWindow account = new PersonalAccountWindow(user);
                account.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный код!");
                GenerateNewCaptcha();
                CaptchaInput.Clear();
            }
        }
    }
}