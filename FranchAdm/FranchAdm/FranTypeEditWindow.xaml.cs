using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FranchAdm.Db;

namespace FranchAdm
{
    public partial class FranTypeEditWindow : Window
    {
        private const int MAX_NAME_LENGTH = 100;
        private const int MIN_NAME_LENGTH = 2;

        private readonly int _franTypeId;
        private readonly bool _isEditMode;

        public FranTypeEditWindow(int franTypeId = 0)
        {
            InitializeComponent();
            _franTypeId = franTypeId;
            _isEditMode = franTypeId > 0;

            if (_isEditMode)
            {
                Title = "Редактирование типа франшизы";
                LoadData();
            }
            else
            {
                Title = "Добавление типа франшизы";
            }
        }

        private void LoadData()
        {
            using (var db = new FranchiseDBEntities1())
            {
                var franType = db.FranTypes.Find(_franTypeId);
                if (franType == null)
                {
                    MessageBox.Show("Тип франшизы не найден!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    Close();
                    return;
                }

                txtName.Text = franType.Name;
            }
        }

        private bool ValidateInputs()
        {
            // === НАЗВАНИЕ ===
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название типа франшизы!", "Ошибка валидации",
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

            var result = MessageBox.Show(
                _isEditMode ? "Сохранить изменения?" : "Добавить новый тип франшизы?",
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
                        var franType = db.FranTypes.Find(_franTypeId);
                        if (franType == null)
                        {
                            MessageBox.Show("Тип франшизы не найден!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            DialogResult = false;
                            Close();
                            return;
                        }

                        franType.Name = txtName.Text;
                        db.Entry(franType).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        // Проверка на дубликат
                        if (db.FranTypes.Any(t => t.Name == txtName.Text))
                        {
                            MessageBox.Show($"Тип франшизы \"{txtName.Text}\" уже существует!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            txtName.Focus();
                            return;
                        }

                        var franType = new FranType
                        {
                            Name = txtName.Text
                        };

                        db.FranTypes.Add(franType);
                    }

                    db.SaveChanges();

                    MessageBox.Show(
                        _isEditMode ? "Тип франшизы обновлён!" : "Тип франшизы добавлен!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 2627)
                {
                    MessageBox.Show("Тип франшизы с таким названием уже существует!",
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
    }
}