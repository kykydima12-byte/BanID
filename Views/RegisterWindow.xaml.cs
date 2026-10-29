using BanID.Services;
using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;

namespace BanID.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LoginBox.Text))
            {
                MessageBox.Show("Введите логин!");
                return;
            }
            if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Введите пароль!");
                return;
            }
            if (string.IsNullOrEmpty(FullNameBox.Text))
            {
                MessageBox.Show("Введите ФИО!");
                return;
            }
            if (string.IsNullOrEmpty(PhoneBox.Text))
            {
                MessageBox.Show("Введите телефон!");
                return;
            }

            using (var db = new BanIDDBEntities())
            {
                if (db.Users.Any(u => u.Login == LoginBox.Text))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!");
                    return;
                }

                try
                {
                    Users user = new Users
                    {
                        Login = LoginBox.Text,
                        PasswordHash = EncryptionService.Encrypt(PasswordBox.Password),
                        FullName = FullNameBox.Text,
                        Phone = PhoneBox.Text,
                        CreatedAt = DateTime.Now
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    MessageBox.Show("Регистрация успешна!");
                    LoginWindow login = new LoginWindow();
                    login.Show();
                    this.Close();
                }
                catch (DbEntityValidationException ex)
                {
                    string errorMessage = "Ошибка валидации:\n";
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            errorMessage += $"Поле: {validationError.PropertyName} - Ошибка: {validationError.ErrorMessage}\n";
                        }
                    }
                    MessageBox.Show(errorMessage);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }
    }
}