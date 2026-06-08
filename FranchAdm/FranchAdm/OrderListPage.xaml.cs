using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FranchAdm.Db;
using FranchAdm.Services;

namespace FranchAdm.Views
{
    public partial class OrderListPage : UserControl
    {
        private const int MAX_SEARCH_LENGTH = 100;
        private DataTable allOrdersTable;
        private bool _isLoading = false;

        public OrderListPage()
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
                    // ЗАГРУЖАЕМ С JOIN К USERS
                    var ordersList = db.Orders
                        .Select(o => new
                        {
                            o.OrderId,
                            o.OrderNumber,
                            // Данные клиента из Users
                            ClientName = o.User != null ? o.User.FullName : "Не указан",
                            ClientEmail = o.User != null ? o.User.Email : "",
                            ClientPhone = o.User != null && o.User.Phone != null ? o.User.Phone : "",
                            // Франшиза
                            FranchiseName = o.Franchise != null ? o.Franchise.Name : "Удалено",
                            // Статус
                            StatusName = o.OrderStatus != null ? o.OrderStatus.Name : "Без статуса",
                            // Сумма
                            TotalAmount = o.TotalAmount,
                            // Дата
                            CreatedAt = o.CreatedAt,
                            // Способ связи
                            ContactMethodName = o.ContactMethod != null ? o.ContactMethod.Name : "",
                            // Регион
                            RegionName = o.Region != null ? o.Region.Name : "",
                            // Комментарий
                            Comment = o.Comments ?? ""
                        })
                        .OrderByDescending(o => o.CreatedAt)
                        .ToList();

                    allOrdersTable = new DataTable();
                    allOrdersTable.Columns.Add("OrderId", typeof(int));
                    allOrdersTable.Columns.Add("OrderNumber", typeof(string));
                    allOrdersTable.Columns.Add("ClientName", typeof(string));
                    allOrdersTable.Columns.Add("ClientEmail", typeof(string));
                    allOrdersTable.Columns.Add("ClientPhone", typeof(string));
                    allOrdersTable.Columns.Add("FranchiseName", typeof(string));
                    allOrdersTable.Columns.Add("StatusName", typeof(string));
                    allOrdersTable.Columns.Add("TotalAmount", typeof(decimal));
                    allOrdersTable.Columns.Add("CreatedAt", typeof(DateTime));
                    allOrdersTable.Columns.Add("ContactMethod", typeof(string));
                    allOrdersTable.Columns.Add("RegionName", typeof(string));
                    allOrdersTable.Columns.Add("Comment", typeof(string));

                    foreach (var item in ordersList)
                    {
                        allOrdersTable.Rows.Add(
                            item.OrderId,
                            item.OrderNumber ?? $"#{item.OrderId}",
                            SafeString(item.ClientName, 100),
                            SafeString(item.ClientEmail, 100),
                            SafeString(item.ClientPhone, 50),
                            SafeString(item.FranchiseName, 200),
                            SafeString(item.StatusName, 50),
                            item.TotalAmount,
                            item.CreatedAt,
                            SafeString(item.ContactMethodName, 50),
                            SafeString(item.RegionName, 100),
                            SafeString(item.Comment, 500)
                        );
                    }

                    lvOrders.ItemsSource = allOrdersTable.DefaultView;
                }

                LoadStatuses();
                LoadFranchises();
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

        private void LoadStatuses()
        {
            try
            {
                using (var db = new FranchiseDBEntities1())
                {
                    cmbStatus.Items.Clear();
                    cmbStatus.Items.Add("Все статусы");

                    var statuses = db.OrderStatuses.OrderBy(s => s.Name).ToList();
                    foreach (var status in statuses)
                    {
                        cmbStatus.Items.Add(status.Name);
                    }

                    cmbStatus.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при загрузке статусов", ex);
            }
        }

        private void LoadFranchises()
        {
            try
            {
                using (var db = new FranchiseDBEntities1())
                {
                    cmbFranchise.Items.Clear();
                    cmbFranchise.Items.Add("Все франшизы");

                    var franchises = db.Franchises.OrderBy(f => f.Name).ToList();
                    foreach (var franchise in franchises)
                    {
                        cmbFranchise.Items.Add(franchise.Name);
                    }

                    cmbFranchise.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при загрузке франшиз", ex);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                if (_isLoading || allOrdersTable == null) return;

                DataView view = allOrdersTable.DefaultView;
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
                    filter += $"(ClientName LIKE '%{searchText}%' OR ClientEmail LIKE '%{searchText}%' OR OrderNumber LIKE '%{searchText}%')";
                }

                if (cmbStatus.SelectedIndex > 0 && cmbStatus.SelectedItem != null)
                {
                    string statusName = cmbStatus.SelectedItem.ToString().Replace("'", "''");
                    if (!string.IsNullOrWhiteSpace(filter)) filter += " AND ";
                    filter += $"StatusName = '{statusName}'";
                }

                if (cmbFranchise.SelectedIndex > 0 && cmbFranchise.SelectedItem != null)
                {
                    string franchiseName = cmbFranchise.SelectedItem.ToString().Replace("'", "''");
                    if (!string.IsNullOrWhiteSpace(filter)) filter += " AND ";
                    filter += $"FranchiseName = '{franchiseName}'";
                }

                view.RowFilter = filter;
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при применении фильтров", ex);
                if (allOrdersTable != null)
                    allOrdersTable.DefaultView.RowFilter = "";
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

        private void CmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (!_isLoading) ApplyFilters(); }
        private void CmbFranchise_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (!_isLoading) ApplyFilters(); }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtSearch.Text = "";
                cmbStatus.SelectedIndex = 0;
                cmbFranchise.SelectedIndex = 0;
                ApplyFilters();
            }
            catch (Exception ex) { HandleError("Ошибка при сбросе фильтров", ex); }
        }

        private void LvOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                bool hasSelection = lvOrders.SelectedItem != null;
                btnEditStatus.IsEnabled      = hasSelection;
                btnGenerateReceipt.IsEnabled = hasSelection;
                btnDelete.IsEnabled          = hasSelection && UserSession.IsAdmin;
            }
            catch (Exception ex) { HandleError("Ошибка при выборе заявки", ex); }
        }

        private void BtnEditStatus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lvOrders.SelectedItem is DataRowView row)
                {
                    if (row["OrderId"] == DBNull.Value)
                    {
                        MessageBox.Show("Неверные данные заявки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    int orderId = Convert.ToInt32(row["OrderId"]);
                    string currentStatus = row["StatusName"].ToString();

                    var statusWindow = new OrderStatusWindow(orderId, currentStatus);
                    if (statusWindow.ShowDialog() == true)
                    {
                        LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при изменении статуса", ex);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lvOrders.SelectedItem is DataRowView row)
                {
                    if (row["OrderId"] == DBNull.Value)
                    {
                        MessageBox.Show("Неверные данные заявки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    int id = Convert.ToInt32(row["OrderId"]);
                    string orderNumber = row["OrderNumber"]?.ToString() ?? $"#{id}";

                    var result = MessageBox.Show(
                        $"Удалить заявку №{orderNumber}?\n\nЭто действие нельзя отменить.",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        using (var db = new FranchiseDBEntities1())
                        {
                            var order = db.Orders.Find(id);
                            if (order != null)
                            {
                                db.Orders.Remove(order);
                                db.SaveChanges();

                                MessageBox.Show("Заявка успешно удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadData();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при удалении заявки", ex);
            }
        }

        private void BtnGenerateReceipt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(lvOrders.SelectedItem is DataRowView row)) return;
                if (row["OrderId"] == DBNull.Value) return;

                int    orderId     = Convert.ToInt32(row["OrderId"]);
                string orderNumber = row["OrderNumber"]?.ToString() ?? $"#{orderId}";

                var confirm = MessageBox.Show(
                    $"Сформировать PDF-чек для заявки {orderNumber}?",
                    "Формирование чека",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes);

                if (confirm != MessageBoxResult.Yes) return;

                var data = LoadReceiptData(orderId);
                if (data == null)
                {
                    MessageBox.Show("Не удалось загрузить данные заявки.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                byte[] pdfBytes = PdfReceiptService.Generate(data);

                SaveReceiptToDb(orderId, orderNumber, pdfBytes);

                string safeName  = orderNumber.Replace("/", "_").Replace("\\", "_");
                string tempPath  = Path.Combine(Path.GetTempPath(),
                    $"Чек_{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                File.WriteAllBytes(tempPath, pdfBytes);
                Process.Start(tempPath);

                MessageBox.Show("Чек успешно сформирован и сохранён!", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при формировании чека", ex);
            }
        }

        private ReceiptDataDto LoadReceiptData(int orderId)
        {
            using (var db = new FranchiseDBEntities1())
            {
                return db.Orders
                    .Where(o => o.OrderId == orderId)
                    .Select(o => new ReceiptDataDto
                    {
                        OrderId           = o.OrderId,
                        OrderNumber       = o.OrderNumber,
                        CreatedAt         = o.CreatedAt,
                        ClientName        = o.User.FullName,
                        ClientEmail       = o.User.Email,
                        FranchiseName     = o.Franchise.Name,
                        FranchiserName    = o.Franchise.Franchiser.Name,
                        CategoryName      = o.Franchise.Category.Name,
                        FinalPrice        = o.Franchise.FinalPrice,
                        PledgeAmount      = o.Franchise.PledgeAmount,
                        RoyaltyPercent    = o.Franchise.RoyaltyPercent,
                        DiscountPercent   = o.Franchise.DiscountPercent,
                        RegionName        = o.Region.Name,
                        ContactMethodName = o.ContactMethod.Name
                    })
                    .FirstOrDefault();
            }
        }

        private void SaveReceiptToDb(int orderId, string orderNumber, byte[] pdfBytes)
        {
            string fileName = $"Чек_{orderNumber.Replace("/", "_")}_{DateTime.Now:yyyyMMdd}.pdf";

            using (var db = new FranchiseDBEntities1())
            {
                db.Database.ExecuteSqlCommand(
                    @"INSERT INTO OrderReceipts (OrderId, FileName, FileData, GeneratedAt, GeneratedByUserId)
                      VALUES (@orderId, @fileName, @fileData, @generatedAt, @generatedByUserId)",
                    new SqlParameter("@orderId",           orderId),
                    new SqlParameter("@fileName",          fileName),
                    new SqlParameter("@fileData",          pdfBytes),
                    new SqlParameter("@generatedAt",       DateTime.Now),
                    new SqlParameter("@generatedByUserId", UserSession.UserId));
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
            userMessage += $"\n\n{ex.GetType().Name}: {ex.Message}";
#endif
            MessageBox.Show(userMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}