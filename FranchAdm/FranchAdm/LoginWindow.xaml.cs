using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FranchAdm.Db;
using FranchAdm;// ← Проверь, что это правильное пространство имён!

namespace FranchAdm
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private bool ValidateEmail(string email, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(email))
            {
                errorMessage = "Введите Email";
                return false;
            }

            email = email.Trim();

            // Проверка длины
            if (email.Length > 100)
            {
                errorMessage = "Email слишком длинный (макс. 100 символов)";
                return false;
            }

            // ПРОСТАЯ проверка: просто наличие @
            if (!email.Contains("@"))
            {
                errorMessage = "Email должен содержать символ @";
                return false;
            }

            // Проверка на пробелы
            if (email.Contains(" "))
            {
                errorMessage = "Email не должен содержать пробелы";
                return false;
            }

            return true;
        }

        private bool ValidatePassword(string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(password))
            {
                errorMessage = "Введите пароль";
                return false;
            }

            if (password.Length > 100)
            {
                errorMessage = "Пароль слишком длинный (макс. 100 символов)";
                return false;
            }

            return true;
        }

        private void CheckInputs(object sender, RoutedEventArgs e)
        {
            btnLogin.IsEnabled = !string.IsNullOrWhiteSpace(txtEmail.Text) &&
                                 !string.IsNullOrWhiteSpace(txtPassword.Password);
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            btnLogin.IsEnabled = false;
            lblStatus.Text = "Проверка данных...";
            lblError.Visibility = Visibility.Collapsed;

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            if (!ValidateEmail(email, out string emailError))
            {
                ShowError(emailError);
                btnLogin.IsEnabled = true;
                return;
            }

            if (!ValidatePassword(password, out string passwordError))
            {
                ShowError(passwordError);
                btnLogin.IsEnabled = true;
                return;
            }

            try
            {
                using (var db = new FranchAdm.Db.FranchiseDBEntities1())  // ← Проверь, что класс называется так!
                {
                    var user = db.Users
                        .Include("Role")
                        .Include("UserStatus")
                        .FirstOrDefault(u =>
                            u.Email == email &&
                            u.Password == password);

                    if (user == null)
                    {
                        ShowError("Неверный Email или пароль.\nПроверьте правильность ввода.");
                        btnLogin.IsEnabled = true;
                        return;
                    }

                    string status = user.UserStatus?.Name?.Trim() ?? "Не определен";

                    if (string.IsNullOrWhiteSpace(status))
                    {
                        ShowError("Ошибка: статус пользователя не определен");
                        btnLogin.IsEnabled = true;
                        return;
                    }

                    if (status != "Активен")
                    {
                        // ИСПРАВЛЕНО для C# 7.3
                        string statusMessage;
                        if (status == "Заблокирован")
                            statusMessage = "Ваш аккаунт заблокирован. Обратитесь к администратору.";
                        else if (status == "Неактивен")
                            statusMessage = "Ваш аккаунт неактивен. Обратитесь к администратору.";
                        else if (status == "Удален")
                            statusMessage = "Ваш аккаунт удален.";
                        else
                            statusMessage = $"Ваш аккаунт имеет статус: {status}. Обратитесь к администратору.";

                        ShowError(statusMessage);
                        btnLogin.IsEnabled = true;
                        return;
                    }

                    string role = user.Role?.Name?.Trim() ?? "Не определен";

                    if (string.IsNullOrWhiteSpace(role))
                    {
                        ShowError("Ошибка: роль пользователя не определена");
                        btnLogin.IsEnabled = true;
                        return;
                    }

                    if (role != "Администратор" && role != "Менеджер")
                    {
                        ShowError($"Доступ запрещен. Ваша роль: {role}\nПриложение доступно только Администраторам и Менеджерам.");
                        btnLogin.IsEnabled = true;
                        return;
                    }

                    UserSession.UserId = user.UserId;
                    UserSession.FullName = user.FullName ?? "Пользователь";
                    UserSession.RoleName = role;

                    var mainWindow = new MainWindow();  // ← Убедись, что MainWindow существует!
                    mainWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                if (ex is System.Data.Entity.Core.EntityException ||
                    ex is System.Data.SqlClient.SqlException)
                {
                    ShowError($"Ошибка подключения к базе данных:\n{ex.Message}");
                }
                else
                {
                    ShowError($"Непредвиденная ошибка:\n{ex.Message}");
                }
                btnLogin.IsEnabled = true;
            }
            finally
            {
                lblStatus.Text = "Введите данные для входа";
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visibility = Visibility.Visible;
            lblStatus.Text = "Вход не выполнен";
            System.Media.SystemSounds.Beep.Play();
        }

        private void TxtEmail_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && btnLogin.IsEnabled)
            {
                txtPassword.Focus();
            }
        }

        private void TxtPassword_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && btnLogin.IsEnabled)
            {
                BtnLogin_Click(this, null);
            }
        }
    }
}