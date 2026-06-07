using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FranchAdm.Db;

namespace FranchAdm
{
    public partial class FranchiserEditWindow : Window
    {
        private const int MAX_NAME_LENGTH = 200;
        private const int MAX_INN_LENGTH = 50;
        private const int MAX_WEBSITE_LENGTH = 200;
        private const int MAX_CONTACTS_LENGTH = 500;
        private const int MIN_NAME_LENGTH = 3;

        private readonly int _franchiserId;
        private readonly bool _isEditMode;

        public FranchiserEditWindow(int franchiserId = 0)
        {
            InitializeComponent();
            _franchiserId = franchiserId;
            _isEditMode = franchiserId > 0;

            if (_isEditMode)
            {
                Title = "Редактирование франчайзера";
                LoadData();
            }
            else
            {
                Title = "Добавление франчайзера";
            }
        }

        private void LoadData()
        {
            using (var db = new FranchiseDBEntities1())
            {
                var franchiser = db.Franchisers.Find(_franchiserId);
                if (franchiser == null)
                {
                    MessageBox.Show("Франчайзер не найден!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    Close();
                    return;
                }

                txtName.Text = franchiser.Name;
                txtInn.Text = franchiser.INN;
                txtWebsite.Text = franchiser.Website;
                txtContacts.Text = franchiser.Contacts;
            }
        }

        private bool ValidateInputs()
        {
            // === НАЗВАНИЕ ===
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название франчайзера!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return false;
            }

            txtName.Text = txtName.Text.Trim();

            if (txtName.Text.Length < MIN_NAME_LENGTH)
            {
                MessageBox.Show($"Название должно содержать не менее {MIN_NAME_LENGTH} символов!",
                    "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return false;
            }

            if (txtName.Text.Length > MAX_NAME_LENGTH)
            {
                MessageBox.Show($"Название не должно превышать {MAX_NAME_LENGTH} символов!",
                    "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return false;
            }

            // === ИНН ===
            if (!string.IsNullOrWhiteSpace(txtInn.Text))
            {
                txtInn.Text = txtInn.Text.Trim();

                // Проверка: только цифры
                if (!Regex.IsMatch(txtInn.Text, @"^[0-9]+$"))
                {
                    MessageBox.Show("ИНН должен содержать только цифры!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtInn.Focus();
                    return false;
                }

                // Проверка длины (10 или 12 знаков для российского ИНН)
                if (txtInn.Text.Length != 10 && txtInn.Text.Length != 12)
                {
                    var result = MessageBox.Show(
                        $"ИНН обычно содержит 10 или 12 цифр (сейчас: {txtInn.Text.Length}).\n\n" +
                        $"Продолжить с текущим значением?",
                        "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result != MessageBoxResult.Yes)
                    {
                        txtInn.Focus();
                        return false;
                    }
                }

                if (txtInn.Text.Length > MAX_INN_LENGTH)
                {
                    MessageBox.Show($"ИНН не должен превышать {MAX_INN_LENGTH} символов!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtInn.Focus();
                    return false;
                }
            }

            // === САЙТ ===
            if (!string.IsNullOrWhiteSpace(txtWebsite.Text))
            {
                txtWebsite.Text = txtWebsite.Text.Trim();

                if (txtWebsite.Text.Length > MAX_WEBSITE_LENGTH)
                {
                    MessageBox.Show($"Сайт не должен превышать {MAX_WEBSITE_LENGTH} символов!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtWebsite.Focus();
                    return false;
                }

                // Проверка формата URL
                if (!txtWebsite.Text.StartsWith("http://") &&
                    !txtWebsite.Text.StartsWith("https://") &&
                    !txtWebsite.Text.StartsWith("www."))
                {
                    if (MessageBox.Show(
                        "Сайт должен начинаться с http://, https:// или www.\n\n" +
                        "Исправить автоматически? (добавить https://)",
                        "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        txtWebsite.Text = "https://" + txtWebsite.Text;
                    }
                }
            }

            // === КОНТАКТЫ (ТЕЛЕФОН) ===
            if (!string.IsNullOrWhiteSpace(txtContacts.Text))
            {
                txtContacts.Text = txtContacts.Text.Trim();

                if (txtContacts.Text.Length > MAX_CONTACTS_LENGTH)
                {
                    MessageBox.Show($"Контактная информация не должна превышать {MAX_CONTACTS_LENGTH} символов!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtContacts.Focus();
                    return false;
                }

                // Проверка формата телефона
                string phonePattern = @"^(\+7|7|8)?[\s\-]?\(?[489][0-9]{2}\)?[\s\-]?[0-9]{3}[\s\-]?[0-9]{2}[\s\-]?[0-9]{2}$";

                // Если не соответствует формату телефона - предупреждение
                if (!Regex.IsMatch(txtContacts.Text, phonePattern))
                {
                    // Разрешаем если это не похоже на телефон (например email или адрес)
                    if (Regex.IsMatch(txtContacts.Text, @"^[0-9\+\-\(\)\s]+$"))
                    {
                        // Это точно телефон, но в неверном формате
                        var result = MessageBox.Show(
                            "Номер телефона должен быть в формате: +7 (XXX) XXX-XX-XX\n\n" +
                            $"Текущее значение: {txtContacts.Text}\n\n" +
                            "Исправить автоматически?",
                            "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            // Простая автоматическая коррекция
                            string digits = Regex.Replace(txtContacts.Text, @"[^0-9]", "");
                            if (digits.Length == 11 && digits.StartsWith("8"))
                            {
                                digits = "7" + digits.Substring(1);
                            }
                            if (digits.Length == 11 && digits.StartsWith("7"))
                            {
                                txtContacts.Text = $"+7 ({digits.Substring(1, 3)}) {digits.Substring(4, 3)}-{digits.Substring(7, 2)}-{digits.Substring(9, 2)}";
                            }
                        }
                        else
                        {
                            txtContacts.Focus();
                            return false;
                        }
                    }
                    // Если содержит @ или другие символы - возможно это не телефон, пропускаем
                }
            }

            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            var result = MessageBox.Show(
                _isEditMode ? "Сохранить изменения?" : "Добавить нового франчайзера?",
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
                        var franchiser = db.Franchisers.Find(_franchiserId);
                        if (franchiser == null)
                        {
                            MessageBox.Show("Франчайзер не найден!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            DialogResult = false;
                            Close();
                            return;
                        }

                        franchiser.Name = txtName.Text;
                        franchiser.INN = string.IsNullOrWhiteSpace(txtInn.Text) ? null : txtInn.Text;
                        franchiser.Website = string.IsNullOrWhiteSpace(txtWebsite.Text) ? null : txtWebsite.Text;
                        franchiser.Contacts = string.IsNullOrWhiteSpace(txtContacts.Text) ? null : txtContacts.Text;

                        db.Entry(franchiser).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        // Проверка на дубликат
                        if (db.Franchisers.Any(f => f.Name == txtName.Text))
                        {
                            MessageBox.Show($"Франчайзер \"{txtName.Text}\" уже существует!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            txtName.Focus();
                            return;
                        }

                        var franchiser = new Franchiser
                        {
                            Name = txtName.Text,
                            INN = string.IsNullOrWhiteSpace(txtInn.Text) ? null : txtInn.Text,
                            Website = string.IsNullOrWhiteSpace(txtWebsite.Text) ? null : txtWebsite.Text,
                            Contacts = string.IsNullOrWhiteSpace(txtContacts.Text) ? null : txtContacts.Text
                        };

                        db.Franchisers.Add(franchiser);
                    }

                    db.SaveChanges();

                    MessageBox.Show(
                        _isEditMode ? "Франчайзер обновлён!" : "Франчайзер добавлен!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 2627)
                {
                    MessageBox.Show("Франчайзер с таким названием уже существует!",
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

        // === ОБРАБОТЧИКИ ВВОДА ===

        private void TxtName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем буквы, цифры, пробелы, дефис и точку
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Zа-яА-Я0-9\s\-\.]+$");
        }

        private void TxtInn_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // ТОЛЬКО ЦИФРЫ для ИНН
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void TxtInn_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Разрешаем: цифры, стрелки, Backspace, Delete, Tab, Home, End
            if (e.Key >= Key.D0 && e.Key <= Key.D9 ||
                e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9 ||
                e.Key == Key.Back || e.Key == Key.Delete ||
                e.Key == Key.Left || e.Key == Key.Right ||
                e.Key == Key.Tab || e.Key == Key.Home || e.Key == Key.End)
            {
                e.Handled = false;
                return;
            }

            // Блокируем: Ctrl+V, Ctrl+C, пробел, буквы
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = e.Key != Key.C; // Разрешаем только Ctrl+C (копирование)
                return;
            }

            // Блокируем всё остальное
            e.Handled = true;
        }

        private void TxtContacts_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем цифры, +, -, (, ), пробел для телефона
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9\+\-\(\)\s]+$");
        }

        private void txtInn_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}