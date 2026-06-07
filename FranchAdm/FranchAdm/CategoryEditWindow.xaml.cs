using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using FranchAdm.Db;

namespace FranchAdm
{
    public partial class CategoryEditWindow : Window
    {
        private const int MAX_NAME_LENGTH = 100;
        private const int MIN_NAME_LENGTH = 2;

        private readonly int _categoryId;
        private readonly bool _isEditMode;

        public CategoryEditWindow(int categoryId = 0)
        {
            InitializeComponent();
            _categoryId = categoryId;
            _isEditMode = categoryId > 0;

            if (_isEditMode)
            {
                Title = "Редактирование категории";
                LoadData();
            }
            else
            {
                Title = "Добавление категории";
            }
        }

        private void LoadData()
        {
            using (var db = new FranchiseDBEntities1())
            {
                var category = db.Categories.Find(_categoryId);
                if (category == null)
                {
                    MessageBox.Show("Категория не найдена!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    Close();
                    return;
                }

                txtName.Text = category.Name;
            }
        }

        private bool ValidateInputs()
        {
            // Название
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название категории!", "Ошибка валидации",
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

            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                using (var db = new FranchiseDBEntities1())
                {
                    if (_isEditMode)
                    {
                        var category = db.Categories.Find(_categoryId);
                        if (category == null)
                        {
                            MessageBox.Show("Категория не найдена!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            DialogResult = false;
                            Close();
                            return;
                        }

                        category.Name = txtName.Text;
                        db.Entry(category).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        // Проверка на дубликат
                        if (db.Categories.Any(c => c.Name == txtName.Text))
                        {
                            MessageBox.Show($"Категория \"{txtName.Text}\" уже существует!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            txtName.Focus();
                            return;
                        }

                        var category = new Category
                        {
                            Name = txtName.Text
                        };

                        db.Categories.Add(category);
                    }

                    db.SaveChanges();

                    MessageBox.Show(
                        _isEditMode ? "Категория обновлена!" : "Категория добавлена!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 2627) // Unique constraint
                {
                    MessageBox.Show("Категория с таким названием уже существует!",
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
            DialogResult = false;
            Close();
        }

        private void TxtName_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Разрешаем только буквы, цифры, пробелы и дефис
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Zа-яА-Я0-9\s\-]+$");
        }
    }
}