using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FranchAdm.Db;

namespace FranchAdm
{ 
    public partial class FranStatusEditWindow : Window
    {
        private const int MAX_NAME_LENGTH = 50;
        private const int MIN_NAME_LENGTH = 2;

        private readonly int _franStatusId;
        private readonly bool _isEditMode;

        public FranStatusEditWindow(int franStatusId = 0)
        {
            InitializeComponent();
            _franStatusId = franStatusId;
            _isEditMode = franStatusId > 0;

            if (_isEditMode)
            {
                Title = "Редактирование статуса";
                LoadData();
            }
            else
            {
                Title = "Добавление статуса";
            }
        }

        private void LoadData()
        {
            using (var db = new FranchiseDBEntities1())
            {
                var status = db.FranStatuses.Find(_franStatusId);
                if (status == null)
                {
                    MessageBox.Show("Статус не найден!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    Close();
                    return;
                }

                txtName.Text = status.Name;
            }
        }

        private bool ValidateInputs()
        {
            // === НАЗВАНИЕ ===
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название статуса!", "Ошибка валидации",
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
                _isEditMode ? "Сохранить изменения?" : "Добавить новый статус?",
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
                        var status = db.FranStatuses.Find(_franStatusId);
                        if (status == null)
                        {
                            MessageBox.Show("Статус не найден!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            DialogResult = false;
                            Close();
                            return;
                        }

                        status.Name = txtName.Text;
                        db.Entry(status).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        // Проверка на дубликат
                        if (db.FranStatuses.Any(s => s.Name == txtName.Text))
                        {
                            MessageBox.Show($"Статус \"{txtName.Text}\" уже существует!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            txtName.Focus();
                            return;
                        }

                        var status = new FranStatus
                        {
                            Name = txtName.Text
                        };

                        db.FranStatuses.Add(status);
                    }

                    db.SaveChanges();

                    MessageBox.Show(
                        _isEditMode ? "Статус обновлён!" : "Статус добавлен!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 2627)
                {
                    MessageBox.Show("Статус с таким названием уже существует!",
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