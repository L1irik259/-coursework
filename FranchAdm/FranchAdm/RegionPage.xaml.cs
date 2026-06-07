using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FranchAdm.Db;

namespace FranchAdm
{
    public partial class RegionPage : UserControl
    {
        private const int MAX_NAME_LENGTH = 100;
        private const int MAX_CODE_LENGTH = 50;
        private const int MIN_NAME_LENGTH = 2;

        private DataTable itemsTable;
        private bool _isLoading = false;

        public RegionPage()
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
                    var query = db.Regions
                        .OrderBy(r => r.Name)
                        .Select(r => new
                        {
                            r.RegionId,
                            r.Name,
                            r.RegionCode
                        })
                        .ToList();

                    itemsTable = new DataTable();
                    itemsTable.Columns.Add("RegionId", typeof(int));
                    itemsTable.Columns.Add("Name", typeof(string));
                    itemsTable.Columns.Add("RegionCode", typeof(string));

                    foreach (var item in query)
                    {
                        itemsTable.Rows.Add(
                            item.RegionId,
                            SafeString(item.Name, MAX_NAME_LENGTH),
                            SafeString(item.RegionCode, MAX_CODE_LENGTH)
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
                    filter += $"Name LIKE '%{searchText}%' OR RegionCode LIKE '%{searchText}%'";
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
            try
            {
                bool hasSelection = lvItems.SelectedItem != null;
                btnEdit.IsEnabled = hasSelection;
                btnDelete.IsEnabled = hasSelection && UserSession.IsAdmin;
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при выборе", ex);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new RegionEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lvItems.SelectedItem is DataRowView row)
            {
                if (row["RegionId"] == DBNull.Value) return;

                int id = Convert.ToInt32(row["RegionId"]);
                var editWindow = new RegionEditWindow(id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadData();
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!UserSession.IsAdmin)
            {
                MessageBox.Show("Только администратор может удалять записи!", "Доступ запрещён",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (lvItems.SelectedItem is DataRowView row)
            {
                if (row["RegionId"] == DBNull.Value) return;

                int id = Convert.ToInt32(row["RegionId"]);
                string name = row["Name"].ToString();

                // ✅ ПРОВЕРКА: используется ли регион в заявках
                using (var db = new FranchiseDBEntities1())
                {
                    var ordersCount = db.Orders.Count(o => o.RegionId == id);
                    if (ordersCount > 0)
                    {
                        MessageBox.Show(
                            $"Нельзя удалить регион \"{name}\"!\n\n" +
                            $"Он используется в {ordersCount} заявках.\n" +
                            $"Сначала удалите или измените эти заявки.",
                            "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                var result = MessageBox.Show(
                    $"Удалить регион \"{name}\"?\n\nЭто действие нельзя отменить.",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new FranchiseDBEntities1())
                        {
                            var item = db.Regions.Find(id);
                            if (item != null)
                            {
                                db.Regions.Remove(item);
                                db.SaveChanges();
                                MessageBox.Show("Регион успешно удалён!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Регион не найден в базе данных.", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleError("Ошибка при удалении", ex);
                    }
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
#if DEBUG
            msg += $"\n\n{ex.GetType().Name}";
#endif
            MessageBox.Show(msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}