using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FranchAdm.Db;

namespace FranchAdm
{
    public partial class FranshiseListPage : UserControl
    {
        private const string PLACEHOLDER_IMAGE_URL = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQpC6vJ1vmEKcffQOvf-BAWkfSeAVxAkFNdNA&s";
        private const int MAX_SEARCH_LENGTH = 100;
        private const int MAX_CATEGORY_LENGTH = 100;
        private const int MAX_STATUS_LENGTH = 50;

        private DataTable allFranchisesTable;
        private bool _isLoading = false;

        public FranshiseListPage()
        {
            try
            {
                InitializeComponent();
                LoadData();
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при инициализации страницы", ex);
            }
        }

        private void LoadData()
        {
            try
            {
                _isLoading = true;

                using (var db = new FranchiseDBEntities1())
                {
                    var query = db.Franchises
                        .Select(f => new
                        {
                            f.FranchiseId,
                            f.Name,
                            f.Description,
                            f.FinalPrice,
                            f.MinPhotoUrl,
                            f.CreatedAt,
                            CategoryName = f.Category != null ? f.Category.Name : "Без категории",
                            FranchiserName = f.Franchiser != null ? f.Franchiser.Name : "Без франчайзера",
                            FranStatusName = f.FranStatus != null ? f.FranStatus.Name : "Без статуса"
                        })
                        .OrderByDescending(f => f.CreatedAt);

                    allFranchisesTable = new DataTable();
                    allFranchisesTable.Columns.Add("FranchiseId", typeof(int));
                    allFranchisesTable.Columns.Add("Name", typeof(string));
                    allFranchisesTable.Columns.Add("Description", typeof(string));
                    allFranchisesTable.Columns.Add("FinalPrice", typeof(decimal));
                    allFranchisesTable.Columns.Add("MinPhotoUrl", typeof(string));
                    allFranchisesTable.Columns.Add("CreatedAt", typeof(DateTime));
                    allFranchisesTable.Columns.Add("CategoryName", typeof(string));
                    allFranchisesTable.Columns.Add("FranchiserName", typeof(string));
                    allFranchisesTable.Columns.Add("FranStatusName", typeof(string));

                    foreach (var item in query.ToList())
                    {
                        string photoUrl = string.IsNullOrWhiteSpace(item.MinPhotoUrl) ? PLACEHOLDER_IMAGE_URL : item.MinPhotoUrl;

                        allFranchisesTable.Rows.Add(
                            item.FranchiseId,
                            SafeString(item.Name, 200),
                            SafeString(item.Description, 500),
                            item.FinalPrice,
                            photoUrl,
                            item.CreatedAt,
                            SafeString(item.CategoryName, 100),
                            SafeString(item.FranchiserName, 200),
                            SafeString(item.FranStatusName, 50)
                        );
                    }

                    lvFranchises.ItemsSource = allFranchisesTable.DefaultView;
                }

                LoadCategories();
                LoadStatuses();
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                HandleError("Ошибка подключения к базе данных", sqlEx);
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

        private void LoadCategories()
        {
            try
            {
                using (var db = new FranchiseDBEntities1())
                {
                    cmbCategory.Items.Clear();
                    cmbCategory.Items.Add("Все категории");

                    var categories = db.Categories.OrderBy(c => c.Name).ToList();
                    foreach (var cat in categories)
                    {
                        cmbCategory.Items.Add(SafeString(cat.Name, MAX_CATEGORY_LENGTH));
                    }
                    cmbCategory.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при загрузке категорий", ex);
            }
        }

        private void LoadStatuses()
        {
            try
            {
                using (var db = new FranchiseDBEntities1())
                {
                    cmbStatus.Items.Clear();
                    cmbStatus.Items.Add("Все статусы");

                    var statuses = db.FranStatuses.OrderBy(s => s.Name).ToList();
                    foreach (var status in statuses)
                    {
                        cmbStatus.Items.Add(SafeString(status.Name, MAX_STATUS_LENGTH));
                    }
                    cmbStatus.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при загрузке статусов", ex);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                if (_isLoading || allFranchisesTable == null) return;

                DataView view = allFranchisesTable.DefaultView;
                string filter = "";

                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    string searchText = txtSearch.Text.Trim();
                    if (searchText.Length > MAX_SEARCH_LENGTH)
                    {
                        searchText = searchText.Substring(0, MAX_SEARCH_LENGTH);
                        txtSearch.Text = searchText;
                    }
                    searchText = searchText.Replace("'", "''");
                    if (!string.IsNullOrWhiteSpace(filter)) filter += " AND ";
                    filter += $"Name LIKE '%{searchText}%' OR Description LIKE '%{searchText}%'";
                }

                if (cmbCategory.SelectedIndex > 0 && cmbCategory.SelectedItem != null)
                {
                    string categoryName = cmbCategory.SelectedItem.ToString();
                    if (!string.IsNullOrWhiteSpace(categoryName) && categoryName.Length <= MAX_CATEGORY_LENGTH)
                    {
                        categoryName = categoryName.Replace("'", "''");
                        if (!string.IsNullOrWhiteSpace(filter)) filter += " AND ";
                        filter += $"CategoryName = '{categoryName}'";
                    }
                }

                if (cmbStatus.SelectedIndex > 0 && cmbStatus.SelectedItem != null)
                {
                    string statusName = cmbStatus.SelectedItem.ToString();
                    if (!string.IsNullOrWhiteSpace(statusName) && statusName.Length <= MAX_STATUS_LENGTH)
                    {
                        statusName = statusName.Replace("'", "''");
                        if (!string.IsNullOrWhiteSpace(filter)) filter += " AND ";
                        filter += $"FranStatusName = '{statusName}'";
                    }
                }

                view.RowFilter = filter;
            }
            catch (SyntaxErrorException)
            {
                if (allFranchisesTable != null)
                    allFranchisesTable.DefaultView.RowFilter = "";
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при применении фильтров", ex);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtSearch.Text) && txtSearch.Text.Length > MAX_SEARCH_LENGTH)
            {
                txtSearch.Text = txtSearch.Text.Substring(0, MAX_SEARCH_LENGTH);
                txtSearch.SelectionStart = txtSearch.Text.Length;
                return;
            }
            ApplyFilters();
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (!_isLoading) ApplyFilters(); }
        private void CmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (!_isLoading) ApplyFilters(); }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtSearch.Text = "";
                cmbCategory.SelectedIndex = 0;
                cmbStatus.SelectedIndex = 0;
                ApplyFilters();
            }
            catch (Exception ex) { HandleError("Ошибка при сбросе фильтров", ex); }
        }

        private void LvFranchises_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                bool hasSelection = lvFranchises.SelectedItem != null;
                btnEdit.IsEnabled = hasSelection;
                btnDelete.IsEnabled = hasSelection && UserSession.IsAdmin;
            }
            catch (Exception ex) { HandleError("Ошибка при выборе франшизы", ex); }
        }

        private void BtnAddFranchise_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var editWindow = new FranchiseEditWindow();
                editWindow.ShowDialog();
                if (editWindow.DialogResult == true) LoadData();
            }
            catch (Exception ex) { HandleError("Ошибка при открытии окна добавления", ex); }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lvFranchises.SelectedItem is DataRowView row)
                {
                    if (row["FranchiseId"] == DBNull.Value)
                    {
                        MessageBox.Show("Неверные данные франшизы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    int id = Convert.ToInt32(row["FranchiseId"]);
                    var editWindow = new FranchiseEditWindow(id);
                    editWindow.ShowDialog();
                    if (editWindow.DialogResult == true) LoadData();
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Неверный формат данных франшизы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex) { HandleError("Ошибка при редактировании франшизы", ex); }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lvFranchises.SelectedItem == null)
                {
                    MessageBox.Show("Выберите франшизу для удаления!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!(lvFranchises.SelectedItem is DataRowView row))
                {
                    MessageBox.Show("Ошибка выбора данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (row["FranchiseId"] == DBNull.Value)
                {
                    MessageBox.Show("Не удалось определить ID франшизы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int id = Convert.ToInt32(row["FranchiseId"]);
                string name = row["Name"]?.ToString() ?? "Без названия";

                using (var db = new FranchiseDBEntities1())
                {
                    var ordersCount = db.Orders.Count(o => o.FranchiseId == id);
                    if (ordersCount > 0)
                    {
                        MessageBox.Show($"Нельзя удалить франшизу \"{name}\"!\n\nОна используется в {ordersCount} заявках.",
                            "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                var result = MessageBox.Show($"Удалить франшизу \"{name}\"?\n\nЭто действие нельзя отменить.",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;

                using (var db = new FranchiseDBEntities1())
                {
                    var franchise = db.Franchises.Find(id);
                    if (franchise == null)
                    {
                        MessageBox.Show("Франшиза не найдена в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var franchiseTags = db.FranchiseTags.Where(ft => ft.FranchiseId == id).ToList();
                    if (franchiseTags.Any()) db.FranchiseTags.RemoveRange(franchiseTags);

                    var priceHistory = db.PriceHistories.Where(ph => ph.FranchiseId == id).ToList();
                    if (priceHistory.Any()) db.PriceHistories.RemoveRange(priceHistory);

                    db.Franchises.Remove(franchise);
                    int rowsAffected = db.SaveChanges();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"Франшиза \"{name}\" успешно удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить франшизу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 547)
                {
                    MessageBox.Show("Нельзя удалить франшизу: она связана с другими записями в базе данных.",
                        "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"Ошибка базы данных: {sqlEx.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
            string userMessage = $"{context}.\n\nЕсли ошибка повторяется, обратитесь к администратору.";
#if DEBUG
            userMessage += $"\n\nТехническая информация:\n{ex.GetType().Name}: {ex.Message}";
#endif
            MessageBox.Show(userMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}