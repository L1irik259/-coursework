using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FranchAdm.Db;

namespace FranchAdm
{
    public partial class TagEditWindow : Window
    {
        private const int MAX_NAME_LENGTH = 50;
        private const int MAX_COLOR_LENGTH = 20;
        private const int MIN_NAME_LENGTH = 2;

        private readonly int _tagId;
        private readonly bool _isEditMode;

        public TagEditWindow(int tagId = 0)
        {
            InitializeComponent();
            _tagId = tagId;
            _isEditMode = tagId > 0;

            if (_isEditMode)
            {
                Title = "Редактирование тега";
                LoadData();
            }
            else
            {
                Title = "Добавление тега";
                // Устанавливаем цвет по умолчанию
                cmbColor.SelectedIndex = 0;
            }
        }

        private void LoadData()
        {
            using (var db = new FranchiseDBEntities1())
            {
                var tag = db.Tags.Find(_tagId);
                if (tag == null)
                {
                    MessageBox.Show("Тег не найден!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    Close();
                    return;
                }

                txtName.Text = tag.Name;
                txtColor.Text = tag.Color;

                // Выбираем цвет в ComboBox если есть совпадение
                string color = tag.Color?.ToUpper() ?? "";
                foreach (var item in cmbColor.Items)
                {
                    var comboBoxItem = item as ComboBoxItem;
                    if (comboBoxItem != null && comboBoxItem.Tag?.ToString() == color)
                    {
                        cmbColor.SelectedItem = item;
                        break;
                    }
                }

                UpdateColorPreview();
            }
        }

        private bool ValidateInputs()
        {
            // Название
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название тега!", "Ошибка валидации",
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

            // Цвет
            if (!string.IsNullOrWhiteSpace(txtColor.Text))
            {
                txtColor.Text = txtColor.Text.Trim().ToUpper();

                // Проверка формата HEX цвета
                if (!Regex.IsMatch(txtColor.Text, @"^#[0-9A-F]{6}$"))
                {
                    MessageBox.Show("Цвет должен быть в формате HEX: #RRGGBB (например: #FF6B35)",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtColor.Focus();
                    return false;
                }

                if (txtColor.Text.Length > MAX_COLOR_LENGTH)
                {
                    MessageBox.Show($"Цвет не должен превышать {MAX_COLOR_LENGTH} символов!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtColor.Focus();
                    return false;
                }
            }

            return true;
        }

        private void UpdateColorPreview()
        {
            try
            {
                string colorHex = txtColor.Text.Trim();
                if (!string.IsNullOrWhiteSpace(colorHex) && colorHex.StartsWith("#"))
                {
                    Color color = (Color)ColorConverter.ConvertFromString(colorHex);
                    colorPreview.Background = new SolidColorBrush(color);
                }
            }
            catch
            {
                // Игнорируем ошибки конвертации
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            var result = MessageBox.Show(
                _isEditMode ? "Сохранить изменения?" : "Добавить новый тег?",
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
                        var tag = db.Tags.Find(_tagId);
                        if (tag == null)
                        {
                            MessageBox.Show("Тег не найден!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            DialogResult = false;
                            Close();
                            return;
                        }

                        tag.Name = txtName.Text;
                        tag.Color = string.IsNullOrWhiteSpace(txtColor.Text) ? null : txtColor.Text;

                        db.Entry(tag).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        // Проверка на дубликат
                        if (db.Tags.Any(t => t.Name == txtName.Text))
                        {
                            MessageBox.Show($"Тег \"{txtName.Text}\" уже существует!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            txtName.Focus();
                            return;
                        }

                        var tag = new Tag
                        {
                            Name = txtName.Text,
                            Color = string.IsNullOrWhiteSpace(txtColor.Text) ? null : txtColor.Text
                        };

                        db.Tags.Add(tag);
                    }

                    db.SaveChanges();

                    MessageBox.Show(
                        _isEditMode ? "Тег обновлён!" : "Тег добавлен!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 2627)
                {
                    MessageBox.Show("Тег с таким названием уже существует!",
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

        private void CmbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbColor.SelectedItem is ComboBoxItem selectedItem)
            {
                string colorHex = selectedItem.Tag?.ToString();
                if (!string.IsNullOrWhiteSpace(colorHex))
                {
                    txtColor.Text = colorHex;
                    UpdateColorPreview();
                }
            }
        }

        private void TxtColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateColorPreview();
        }
    }
}