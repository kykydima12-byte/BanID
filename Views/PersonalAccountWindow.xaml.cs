using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.Timers;

namespace BanID.Views
{
    public partial class PersonalAccountWindow : Window
    {
        private Users currentUser;
        private Timer statusTimer;

        public PersonalAccountWindow(Users user)
        {
            InitializeComponent();
            currentUser = user;
            LoadProfile();
            LoadAccounts();
            LoadRequests();
            StartTimer();
        }

        private void LoadProfile()
        {
            FullNameText.Text = currentUser.FullName;
            PhoneText.Text = currentUser.Phone;
        }

        private void LoadAccounts()
        {
            using (var db = new BanIDDBEntities())
                AccountsGrid.ItemsSource = db.Accounts.Where(a => a.UserId == currentUser.Id).ToList();
        }

        private void LoadRequests()
        {
                RequestsGrid.ItemsSource = App.Context.Requests.Where(r => r.UserId == currentUser.Id).OrderByDescending(r => r.Date).ToList();
        }

        private void StartTimer()
        {
            statusTimer = new Timer(30000);
            statusTimer.Elapsed += (s, e) => Dispatcher.Invoke(UpdateStatuses);
            statusTimer.Start();
        }

        private void UpdateStatuses()
        {
            using (var db = new BanIDDBEntities())
            {
                var requests = db.Requests.Where(r => r.UserId == currentUser.Id && (r.Status == "Новая" || r.Status == "На рассмотрении")).ToList();
                bool changed = false;

                foreach (var req in requests.Where(r => r.Date != null))
                {
                    double minutes = (DateTime.Now - req.Date.Value).TotalMinutes;

                    if (req.Status == "Новая" && minutes >= 1)
                    {
                        req.Status = "На рассмотрении";
                        changed = true;
                    }
                    else if (req.Status == "На рассмотрении" && minutes >= 3)
                    {
                        req.Status = "Одобрена";
                        changed = true;
                        ProcessRequest(req, db);
                    }
                }

                if (changed)
                {
                    db.SaveChanges();
                    LoadRequests();
                    LoadAccounts();
                }
            }
        }

        private void ProcessRequest(Requests req, BanIDDBEntities db)
        {
            if (req.Type == "Открытие счета" || req.Type == "Открытие вклада")
            {
                string type = req.Type == "Открытие вклада" ? "Вклад" : "Текущий";

                DateTime oneMinuteAgo = DateTime.Now.AddMinutes(-1);

                var existingAccount = db.Accounts.FirstOrDefault(a =>
                    a.UserId == req.UserId &&
                    a.Type == type &&
                    a.OpenDate >= oneMinuteAgo);

                if (existingAccount == null)
                {
                    db.Accounts.Add(new Accounts
                    {
                        UserId = req.UserId,
                        AccountNumber = "40817" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10),
                        Balance = type == "Вклад" ? 10000 : 0,
                        Type = type,
                        OpenDate = DateTime.Now
                    });
                    MessageBox.Show("Счёт успешно открыт!");
                }
            }
            else if (req.Type == "Закрытие счета")
            {
                var acc = db.Accounts.FirstOrDefault(a => a.UserId == req.UserId && a.Type == "Текущий");
                if (acc != null)
                {
                    db.Accounts.Remove(acc);
                    MessageBox.Show("Счёт закрыт");
                }
                else
                    MessageBox.Show("Нет счетов для закрытия");
            }
            else if (req.Type == "Закрытие вклада")
            {
                var dep = db.Accounts.FirstOrDefault(a => a.UserId == req.UserId && a.Type == "Вклад");
                if (dep != null)
                {
                    db.Accounts.Remove(dep);
                    MessageBox.Show("Вклад закрыт");
                }
                else
                    MessageBox.Show("Нет вкладов для закрытия");
            }
        }

        private void OrderStatement_Click(object sender, RoutedEventArgs e)
        {
            var selected = AccountsGrid.SelectedItem as Accounts;
            if (selected == null)
            {
                MessageBox.Show("Выберите счёт!");
                return;
            }

            var save = new SaveFileDialog
            {
                Filter = "Текстовый файл (*.txt)|*.txt",
                FileName = $"Выписка_{selected.AccountNumber}_{DateTime.Now:yyyyMMdd}.txt"
            };

            if (save.ShowDialog() == true)
            {
                string content = $"ВЫПИСКА ПО СЧЁТУ\n" +
                                 $"Номер счёта: {selected.AccountNumber}\n" +
                                 $"Дата выписки: {DateTime.Now:dd.MM.yyyy HH:mm:ss}\n" +
                                 $"Баланс: {selected.Balance:F2} руб.\n\n" +
                                 $"ДЕТАЛИЗАЦИЯ ОПЕРАЦИЙ:\n" +
                                 $"01.04.2025  Поступление       +10 000.00 руб.\n" +
                                 $"02.04.2025  Оплата услуг      -2 500.00 руб.\n" +
                                 $"05.04.2025  Перевод           -1 000.00 руб.\n" +
                                 $"Итого оборотов:              +10 000.00 руб.\n" +
                                 $"Итого списаний:              -3 500.00 руб.";
                File.WriteAllText(save.FileName, content);
                MessageBox.Show("Выписка сохранена!");
            }
        }

        private void CreateRequest_Click(object sender, RoutedEventArgs e)
        {
            var type = RequestTypeBox.SelectedItem as ComboBoxItem;
            if (type == null)
            {
                MessageBox.Show("Выберите тип заявки!");
                return;
            }

            using (var db = new BanIDDBEntities())
            {
                db.Requests.Add(new Requests
                {
                    UserId = currentUser.Id,
                    Type = type.Content.ToString(),
                    Details = $"Запрос на {type.Content}",
                    Date = DateTime.Now,
                    Status = "Новая"
                });
                db.SaveChanges();
            }
            LoadRequests();
            MessageBox.Show("Заявка создана!");
        }

        protected override void OnClosed(EventArgs e)
        {
            statusTimer?.Stop();
            statusTimer?.Dispose();
            base.OnClosed(e);
        }
    }
}