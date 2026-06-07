using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FranchAdm.Db;

namespace FranchAdm
{
    public partial class ContactMethodEditWindow : Window
    {
        private const int MAX_NAME_LENGTH = 50;
        private const int MIN_NAME_LENGTH = 2;

        private readonly int _contactMethodId;
        private readonly bool _isEditMode;

        public ContactMethodEditWindow(int contactMethodId = 0)
        {
            InitializeComponent();
            _contactMethodId = contactMethodId;
            _isEditMode = contactMethodId > 0;

            if (_isEditMode)
            {
                Title = "Редактирование способа связи";
                LoadData();
            }
            else
            {
                Title = "Добавление способа связи";
            }
        }

        private void LoadData()
        {
            using (var db = new FranchiseDBEntities1())
            {
                var contactMethod = db.ContactMethods.Find(_contactMethodId);
                if (contactMethod == null)
                {
                    MessageBox.Show("Способ связи не найден!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    Close();
                    return;
                }

                txtName.Text = contactMethod.Name;
            }
        }

        private bool ValidateInputs()
        {
            // === НАЗВАНИЕ ===
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название способа связи!", "Ошибка валидации",
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
                _isEditMode ? "Сохранить изменения?" : "Добавить новый способ связи?",
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
                        var contactMethod = db.ContactMethods.Find(_contactMethodId);
                        if (contactMethod == null)
                        {
                            MessageBox.Show("Способ связи не найден!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            DialogResult = false;
                            Close();
                            return;
                        }

                        contactMethod.Name = txtName.Text;
                        db.Entry(contactMethod).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        // Проверка на дубликат
                        if (db.ContactMethods.Any(c => c.Name == txtName.Text))
                        {
                            MessageBox.Show($"Способ связи \"{txtName.Text}\" уже существует!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            txtName.Focus();
                            return;
                        }

                        var contactMethod = new ContactMethod
                        {
                            Name = txtName.Text
                        };

                        db.ContactMethods.Add(contactMethod);
                    }

                    db.SaveChanges();

                    MessageBox.Show(
                        _isEditMode ? "Способ связи обновлён!" : "Способ связи добавлен!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 2627)
                {
                    MessageBox.Show("Способ связи с таким названием уже существует!",
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