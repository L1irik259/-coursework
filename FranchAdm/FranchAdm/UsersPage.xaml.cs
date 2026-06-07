using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FranchAdm.Db;

namespace FranchAdm
{
    public partial class UsersPage : UserControl
    {
        private const int MAX_SEARCH_LENGTH = 100;

        private DataTable usersTable;
        private bool _isLoading = false;

        public UsersPage()
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
                    var query = db.Users
                        .OrderBy(u => u.FullName)
                        .Select(u => new
                        {
                            u.UserId,
                            u.FullName,
                            u.Email,
                            u.Phone,
                            u.RegisteredAt,
                            u.LastLogin,
                            RoleName = u.Role != null ? u.Role.Name : "Без роли",
                            RoleId = u.RoleId,
                            UserStatusName = u.UserStatus != null ? u.UserStatus.Name : "Без статуса",
                            UserStatusId = u.UserStatusId
                        })
                        .ToList();

                    usersTable = new DataTable();
                    usersTable.Columns.Add("UserId", typeof(int));
                    usersTable.Columns.Add("FullName", typeof(string));
                    usersTable.Columns.Add("Email", typeof(string));
                    usersTable.Columns.Add("Phone", typeof(string));
                    usersTable.Columns.Add("RegisteredAt", typeof(DateTime));
                    usersTable.Columns.Add("LastLogin", typeof(DateTime));
                    usersTable.Columns.Add("RoleName", typeof(string));
                    usersTable.Columns.Add("RoleId", typeof(int));
                    usersTable.Columns.Add("UserStatusName", typeof(string));
                    usersTable.Columns.Add("UserStatusId", typeof(int));

                    foreach (var item in query)
                    {
                        usersTable.Rows.Add(
                            item.UserId,
                            SafeString(item.FullName, 100),
                            SafeString(item.Email, 100),
                            SafeString(item.Phone, 50),
                            item.RegisteredAt,
                            item.LastLogin,
                            SafeString(item.RoleName, 50),
                            item.RoleId,
                            SafeString(item.UserStatusName, 50),
                            item.UserStatusId
                        );
                    }

                    lvUsers.ItemsSource = usersTable.DefaultView;
                }

                LoadRoles();
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

        private void LoadRoles()
        {
            try
            {
                using (var db = new FranchiseDBEntities1())
                {
                    cmbRole.Items.Clear();
                    cmbRole.Items.Add("Все роли");

                    var roles = db.Roles.OrderBy(r => r.Name).ToList();
                    foreach (var role in roles)
                    {
                        cmbRole.Items.Add(role.Name);
                    }

                    cmbRole.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при загрузке ролей", ex);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                if (_isLoading || usersTable == null) return;

                DataView view = usersTable.DefaultView;
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
                    filter += $"(FullName LIKE '%{searchText}%' OR Email LIKE '%{searchText}%' OR Phone LIKE '%{searchText}%')";
                }

                if (cmbRole.SelectedIndex > 0 && cmbRole.SelectedItem != null)
                {
                    string roleName = cmbRole.SelectedItem.ToString().Replace("'", "''");
                    if (!string.IsNullOrWhiteSpace(filter)) filter += " AND ";
                    filter += $"RoleName = '{roleName}'";
                }

                view.RowFilter = filter;
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при фильтрации", ex);
                if (usersTable != null)
                    usersTable.DefaultView.RowFilter = "";
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

        private void CmbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoading)
                ApplyFilters();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            cmbRole.SelectedIndex = 0;
            ApplyFilters();
        }

        private void LvUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                bool hasSelection = lvUsers.SelectedItem != null;
                btnEdit.IsEnabled = hasSelection;
                btnDelete.IsEnabled = hasSelection;
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при выборе", ex);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new UserEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lvUsers.SelectedItem is DataRowView row)
            {
                if (row["UserId"] == DBNull.Value) return;

                int id = Convert.ToInt32(row["UserId"]);

                if (id == UserSession.UserId)
                {
                    MessageBox.Show("Вы не можете редактировать свою учётную запись здесь.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var editWindow = new UserEditWindow(id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadData();
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lvUsers.SelectedItem is DataRowView row)
            {
                if (row["UserId"] == DBNull.Value) return;

                int id = Convert.ToInt32(row["UserId"]);
                string fullName = row["FullName"].ToString();
                string roleName = row["RoleName"].ToString();

                if (id == UserSession.UserId)
                {
                    MessageBox.Show("Вы не можете удалить свою учётную запись!",
                        "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (roleName == "Администратор")
                {
                    using (var db = new FranchiseDBEntities1())
                    {
                        var adminCount = db.Users.Count(u => u.RoleId == 1);
                        if (adminCount <= 1)
                        {
                            MessageBox.Show(
                                "Нельзя удалить последнего администратора!\n\n" +
                                "Сначала назначьте администратором другого пользователя.",
                                "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                }

                using (var db = new FranchiseDBEntities1())
                {
                    var ordersCount = db.Orders.Count(o => o.UserId == id);
                    if (ordersCount > 0)
                    {
                        MessageBox.Show(
                            $"Нельзя удалить пользователя \"{fullName}\"!\n\n" +
                            $"Он является автором {ordersCount} заявок.",
                            "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                var result = MessageBox.Show(
                    $"Удалить пользователя \"{fullName}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new FranchiseDBEntities1())
                        {
                            var user = db.Users.Find(id);
                            if (user != null)
                            {
                                db.Users.Remove(user);
                                db.SaveChanges();

                                MessageBox.Show("Пользователь успешно удалён!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadData();
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