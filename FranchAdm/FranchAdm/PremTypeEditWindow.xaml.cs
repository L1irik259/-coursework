using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FranchAdm.Db;

namespace FranchAdm
{
    public partial class PremTypeEditWindow : Window
    {
        private const int MAX_NAME_LENGTH = 100;
        private const int MAX_REQUIREMENTS_LENGTH = 500;
        private const int MIN_NAME_LENGTH = 2;
        private const int MAX_AREA = 100000;
        private const int MIN_AREA = 0;

        private readonly int _premTypeId;
        private readonly bool _isEditMode;

        public PremTypeEditWindow(int premTypeId = 0)
        {
            InitializeComponent();
            _premTypeId = premTypeId;
            _isEditMode = premTypeId > 0;

            if (_isEditMode)
            {
                Title = "Редактирование типа помещения";
                LoadData();
            }
            else
            {
                Title = "Добавление типа помещения";
            }
        }

        private void LoadData()
        {
            using (var db = new FranchiseDBEntities1())
            {
                var premType = db.PremTypeses.Find(_premTypeId);
                if (premType == null)
                {
                    MessageBox.Show("Тип помещения не найден!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    Close();
                    return;
                }

                txtName.Text = premType.Name;
                txtMinArea.Text = premType.MinArea > 0 ? premType.MinArea.ToString() : "";
                txtRequirements.Text = premType.Requirements;
            }
        }

        private bool ValidateInputs()
        {
            // === НАЗВАНИЕ ===
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название типа помещения!", "Ошибка валидации",
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

            // === МИНИМАЛЬНАЯ ПЛОЩАДЬ ===
            if (!string.IsNullOrWhiteSpace(txtMinArea.Text))
            {
                if (!int.TryParse(txtMinArea.Text, out int minArea))
                {
                    MessageBox.Show("Минимальная площадь должна быть числом!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtMinArea.Focus();
                    return false;
                }

                if (minArea < MIN_AREA)
                {
                    MessageBox.Show($"Минимальная площадь не может быть отрицательной!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtMinArea.Focus();
                    return false;
                }

                if (minArea > MAX_AREA)
                {
                    MessageBox.Show($"Минимальная площадь не может превышать {MAX_AREA} м²!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtMinArea.Focus();
                    return false;
                }
            }

            // === ТРЕБОВАНИЯ ===
            if (!string.IsNullOrWhiteSpace(txtRequirements.Text))
            {
                txtRequirements.Text = txtRequirements.Text.Trim();

                if (txtRequirements.Text.Length > MAX_REQUIREMENTS_LENGTH)
                {
                    MessageBox.Show($"Требования не должны превышать {MAX_REQUIREMENTS_LENGTH} символов!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtRequirements.Focus();
                    return false;
                }
            }

            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            var result = MessageBox.Show(
                _isEditMode ? "Сохранить изменения?" : "Добавить новый тип помещения?",
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
                        var premType = db.PremTypeses.Find(_premTypeId);
                        if (premType == null)
                        {
                            MessageBox.Show("Тип помещения не найден!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            DialogResult = false;
                            Close();
                            return;
                        }

                        premType.Name = txtName.Text;
                        premType.MinArea = int.TryParse(txtMinArea.Text, out int area) ? area : 0;
                        premType.Requirements = string.IsNullOrWhiteSpace(txtRequirements.Text)
                            ? null
                            : txtRequirements.Text;

                        db.Entry(premType).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        // Проверка на дубликат
                        if (db.PremTypeses.Any(p => p.Name == txtName.Text))
                        {
                            MessageBox.Show($"Тип помещения \"{txtName.Text}\" уже существует!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            txtName.Focus();
                            return;
                        }

                        var premType = new PremTypes
                        {
                            Name = txtName.Text,
                            MinArea = int.TryParse(txtMinArea.Text, out int area) ? area : 0,
                            Requirements = string.IsNullOrWhiteSpace(txtRequirements.Text)
                                ? null
                                : txtRequirements.Text
                        };

                        db.PremTypeses.Add(premType);
                    }

                    db.SaveChanges();

                    MessageBox.Show(
                        _isEditMode ? "Тип помещения обновлён!" : "Тип помещения добавлен!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 2627)
                {
                    MessageBox.Show("Тип помещения с таким названием уже существует!",
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

        private void TxtMinArea_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // ТОЛЬКО ЦИФРЫ для площади
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }
    }
}