using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using FranchAdm.Db;

namespace FranchAdm
{
    public partial class UserEditWindow : Window
    {
        private const int MAX_NAME_LENGTH = 100;
        private const int MAX_EMAIL_LENGTH = 100;
        private const int MAX_PHONE_LENGTH = 50;
        private const int MIN_PASSWORD_LENGTH = 6;

        private readonly int _userId;
        private readonly bool _isEditMode;

        public UserEditWindow(int userId = 0)
        {
            InitializeComponent();
            _userId = userId;
            _isEditMode = userId > 0;

            if (_isEditMode)
            {
                Title = "Редактирование пользователя";
                LoadData();
            }
            else
            {
                Title = "Добавление пользователя";
            }

            LoadRoles();
        }

        private void LoadData()
        {
            using (var db = new FranchiseDBEntities1())
            {
                var user = db.Users.Find(_userId);
                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    Close();
                    return;
                }

                txtFullName.Text = user.FullName;
                txtEmail.Text = user.Email;
                txtPhone.Text = user.Phone;
                cmbRole.SelectedValue = user.RoleId;
            }
        }

        private void LoadRoles()
        {
            using (var db = new FranchiseDBEntities1())
            {
                cmbRole.ItemsSource = db.Roles.OrderBy(r => r.Name).ToList();
                cmbRole.DisplayMemberPath = "Name";
                cmbRole.SelectedValuePath = "RoleId";

                if (!_isEditMode)
                {
                    var managerRole = db.Roles.FirstOrDefault(r => r.Name == "Менеджер");
                    if (managerRole != null)
                    {
                        cmbRole.SelectedValue = managerRole.RoleId;
                    }
                }
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введите ФИО!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFullName.Focus();
                return false;
            }

            txtFullName.Text = txtFullName.Text.Trim();
            if (txtFullName.Text.Length > MAX_NAME_LENGTH)
            {
                MessageBox.Show($"ФИО не должно превышать {MAX_NAME_LENGTH} символов!",
                    "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFullName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Введите Email!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            txtEmail.Text = txtEmail.Text.Trim();
            if (!Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Введите корректный Email!",
                    "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            if (txtEmail.Text.Length > MAX_EMAIL_LENGTH)
            {
                MessageBox.Show($"Email не должен превышать {MAX_EMAIL_LENGTH} символов!",
                    "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                txtPhone.Text = txtPhone.Text.Trim();
                if (txtPhone.Text.Length > MAX_PHONE_LENGTH)
                {
                    MessageBox.Show($"Телефон не должен превышать {MAX_PHONE_LENGTH} символов!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPhone.Focus();
                    return false;
                }
            }

            if (cmbRole.SelectedValue == null)
            {
                MessageBox.Show("Выберите роль!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbRole.Focus();
                return false;
            }

            if (!_isEditMode)
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    MessageBox.Show("Введите пароль!", "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Focus();
                    return false;
                }

                if (txtPassword.Password.Length < MIN_PASSWORD_LENGTH)
                {
                    MessageBox.Show($"Пароль должен содержать не менее {MIN_PASSWORD_LENGTH} символов!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Focus();
                    return false;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    if (txtPassword.Password.Length < MIN_PASSWORD_LENGTH)
                    {
                        MessageBox.Show($"Пароль должен содержать не менее {MIN_PASSWORD_LENGTH} символов!",
                            "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtPassword.Focus();
                        return false;
                    }
                }
            }

            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            var result = MessageBox.Show(
                _isEditMode ? "Сохранить изменения?" : "Добавить нового пользователя?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new FranchiseDBEntities1())
                {
                    if (_isEditMode)
                    {
                        var user = db.Users.Find(_userId);
                        if (user == null)
                        {
                            MessageBox.Show("Пользователь не найден!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            DialogResult = false;
                            Close();
                            return;
                        }

                        user.FullName = txtFullName.Text;
                        user.Email = txtEmail.Text;
                        user.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text;
                        user.RoleId = Convert.ToInt32(cmbRole.SelectedValue);

                        if (!string.IsNullOrWhiteSpace(txtPassword.Password))
                        {
                            user.Password = HashPassword(txtPassword.Password);
                        }

                        db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        if (db.Users.Any(u => u.Email == txtEmail.Text))
                        {
                            MessageBox.Show($"Пользователь с Email \"{txtEmail.Text}\" уже существует!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            txtEmail.Focus();
                            return;
                        }

                        var user = new User
                        {
                            FullName = txtFullName.Text,
                            Email = txtEmail.Text,
                            Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text,
                            Password = HashPassword(txtPassword.Password),
                            RoleId = Convert.ToInt32(cmbRole.SelectedValue),
                            RegisteredAt = DateTime.Now,
                            UserStatusId = 1
                        };

                        db.Users.Add(user);
                    }

                    db.SaveChanges();

                    MessageBox.Show(
                        _isEditMode ? "Пользователь обновлён!" : "Пользователь добавлен!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 2627)
                {
                    MessageBox.Show("Пользователь с таким Email уже существует!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка базы данных: {sqlEx.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Закрыть без сохранения?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}