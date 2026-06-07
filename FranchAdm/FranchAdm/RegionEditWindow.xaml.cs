using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using FranchAdm.Db;

namespace FranchAdm
{
    public partial class RegionEditWindow : Window
    {
        private const int MAX_NAME_LENGTH = 100;
        private const int MAX_CODE_LENGTH = 50;
        private const int MIN_NAME_LENGTH = 2;

        private readonly int _regionId;
        private readonly bool _isEditMode;

        public RegionEditWindow(int regionId = 0)
        {
            InitializeComponent();
            _regionId = regionId;
            _isEditMode = regionId > 0;

            if (_isEditMode)
            {
                Title = "Редактирование региона";
                LoadData();
            }
            else
            {
                Title = "Добавление региона";
            }
        }

        private void LoadData()
        {
            using (var db = new FranchiseDBEntities1())
            {
                var region = db.Regions.Find(_regionId);
                if (region == null)
                {
                    MessageBox.Show("Регион не найден!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    Close();
                    return;
                }

                txtName.Text = region.Name;
                txtRegionCode.Text = region.RegionCode;
            }
        }

        private bool ValidateInputs()
        {
            // === НАЗВАНИЕ ===
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название региона!", "Ошибка валидации",
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

            // === КОД РЕГИОНА ===
            if (!string.IsNullOrWhiteSpace(txtRegionCode.Text))
            {
                txtRegionCode.Text = txtRegionCode.Text.Trim();

                if (txtRegionCode.Text.Length > MAX_CODE_LENGTH)
                {
                    MessageBox.Show($"Код региона не должен превышать {MAX_CODE_LENGTH} символов!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtRegionCode.Focus();
                    return false;
                }

                // Проверка: код должен содержать только цифры и дефис
                if (!Regex.IsMatch(txtRegionCode.Text, @"^[0-9\-]+$"))
                {
                    MessageBox.Show("Код региона должен содержать только цифры!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtRegionCode.Focus();
                    return false;
                }
            }

            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            var result = MessageBox.Show(
                _isEditMode ? "Сохранить изменения?" : "Добавить новый регион?",
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
                        var region = db.Regions.Find(_regionId);
                        if (region == null)
                        {
                            MessageBox.Show("Регион не найден!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            DialogResult = false;
                            Close();
                            return;
                        }

                        region.Name = txtName.Text;
                        region.RegionCode = string.IsNullOrWhiteSpace(txtRegionCode.Text)
                            ? null
                            : txtRegionCode.Text;

                        db.Entry(region).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        // Проверка на дубликат названия
                        if (db.Regions.Any(r => r.Name == txtName.Text))
                        {
                            MessageBox.Show($"Регион \"{txtName.Text}\" уже существует!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            txtName.Focus();
                            return;
                        }

                        // Проверка на дубликат кода
                        if (!string.IsNullOrWhiteSpace(txtRegionCode.Text) &&
                            db.Regions.Any(r => r.RegionCode == txtRegionCode.Text))
                        {
                            MessageBox.Show($"Регион с кодом \"{txtRegionCode.Text}\" уже существует!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            txtRegionCode.Focus();
                            return;
                        }

                        var region = new Region
                        {
                            Name = txtName.Text,
                            RegionCode = string.IsNullOrWhiteSpace(txtRegionCode.Text)
                                ? null
                                : txtRegionCode.Text
                        };

                        db.Regions.Add(region);
                    }

                    db.SaveChanges();

                    MessageBox.Show(
                        _isEditMode ? "Регион обновлён!" : "Регион добавлен!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 2627)
                {
                    MessageBox.Show("Регион с таким названием или кодом уже существует!",
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