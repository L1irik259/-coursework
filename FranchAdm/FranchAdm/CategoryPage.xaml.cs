using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FranchAdm.Db;

namespace FranchAdm
{
    public partial class CategoryPage : UserControl
    {
        private const int MAX_NAME_LENGTH = 100;
        private const int MIN_NAME_LENGTH = 2;

        private DataTable itemsTable;
        private bool _isLoading = false;

        public CategoryPage()
        {
            try
            {
                InitializeComponent();
                LoadData();
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при инициализации", ex);
            }
        }

        private void LoadData()
        {
            try
            {
                _isLoading = true;

                using (var db = new FranchiseDBEntities1())
                {
                    var query = db.Categories
                        .OrderBy(c => c.Name)
                        .Select(c => new
                        {
                            c.CategoryId,
                            c.Name
                        })
                        .ToList();

                    itemsTable = new DataTable();
                    itemsTable.Columns.Add("CategoryId", typeof(int));
                    itemsTable.Columns.Add("Name", typeof(string));

                    foreach (var item in query)
                    {
                        itemsTable.Rows.Add(
                            item.CategoryId,
                            SafeString(item.Name, MAX_NAME_LENGTH)
                        );
                    }

                    lvItems.ItemsSource = itemsTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при загрузке данных", ex);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ApplyFilters()
        {
            try
            {
                if (_isLoading || itemsTable == null) return;

                DataView view = itemsTable.DefaultView;
                string filter = "";

                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    string searchText = txtSearch.Text.Trim();
                    if (searchText.Length > 100)
                    {
                        searchText = searchText.Substring(0, 100);
                        txtSearch.Text = searchText;
                    }
                    searchText = searchText.Replace("'", "''");
                    filter += $"Name LIKE '%{searchText}%'";
                }

                view.RowFilter = filter;
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при фильтрации", ex);
                if (itemsTable != null)
                    itemsTable.DefaultView.RowFilter = "";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtSearch.Text) && txtSearch.Text.Length > 100)
            {
                txtSearch.Text = txtSearch.Text.Substring(0, 100);
                txtSearch.SelectionStart = txtSearch.Text.Length;
                return;
            }
            ApplyFilters();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            ApplyFilters();
        }

        private void LvItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = lvItems.SelectedItem != null;
            btnEdit.IsEnabled = hasSelection;
            // ✅ ИСПРАВЛЕНО: Только админ может удалять
            btnDelete.IsEnabled = hasSelection && UserSession.IsAdmin;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new CategoryEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lvItems.SelectedItem is DataRowView row)
            {
                if (row["CategoryId"] == DBNull.Value) return;

                int id = Convert.ToInt32(row["CategoryId"]);
                var editWindow = new CategoryEditWindow(id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadData();
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // ✅ ПРОВЕРКА ПРАВ ДОСТУПА
            if (!UserSession.IsAdmin)
            {
                MessageBox.Show("Только администратор может удалять записи!", "Доступ запрещён",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (lvItems.SelectedItem is DataRowView row)
            {
                if (row["CategoryId"] == DBNull.Value) return;

                int id = Convert.ToInt32(row["CategoryId"]);
                string name = row["Name"].ToString();

                try
                {
                    using (var db = new FranchiseDBEntities1())
                    {
                        // ✅ ПРОВЕРКА: используется ли категория в франшизах
                        var franchisesCount = db.Franchises.Count(f => f.CategoryId == id);

                        if (franchisesCount > 0)
                        {
                            MessageBox.Show(
                                $"Нельзя удалить категорию \"{name}\"!\n\n" +
                                $"Она используется в {franchisesCount} франшизах.\n" +
                                $"Сначала удалите или измените эти франшизы.",
                                "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    var result = MessageBox.Show(
                        $"Удалить категорию \"{name}\"?\n\nЭто действие нельзя отменить.",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        using (var db = new FranchiseDBEntities1())
                        {
                            var item = db.Categories.Find(id);
                            if (item != null)
                            {
                                db.Categories.Remove(item);
                                db.SaveChanges();

                                MessageBox.Show("Категория успешно удалена!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Категория не найдена в базе данных.", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    HandleError("Ошибка при удалении категории", ex);
                }
            }
        }

        private string SafeString(object value, int maxLength)
        {
            if (value == null || value == DBNull.Value) return string.Empty;
            string str = value.ToString().Trim();
            return str.Length > maxLength ? str.Substring(0, maxLength) : str;
        }

        private void HandleError(string context, Exception ex)
        {
            string msg = $"{context}\n\n{ex.Message}";

            // Показываем внутреннюю ошибку если есть
            if (ex.InnerException != null)
            {
                msg += $"\n\nДетали: {ex.InnerException.Message}";
            }

#if DEBUG
            msg += $"\n\n{ex.GetType().Name}";
#endif
            MessageBox.Show(msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}