using System;
using System.Linq;
using System.Windows;
using FranchAdm.Db;

namespace FranchAdm
{
    public partial class OrderStatusWindow : Window
    {
        private readonly int _orderId;

        public OrderStatusWindow(int orderId, string currentStatus)
        {
            InitializeComponent();
            _orderId = orderId;
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            LoadStatuses(currentStatus);
        }

        private void LoadStatuses(string currentStatus)
        {
            using (var db = new FranchiseDBEntities1())
            {
                cmbStatus.ItemsSource = db.OrderStatuses.OrderBy(s => s.Name).ToList();
                cmbStatus.DisplayMemberPath = "Name";
                cmbStatus.SelectedValuePath = "OrderStatusId";

                // Выбираем текущий статус
                var current = db.OrderStatuses.FirstOrDefault(s => s.Name == currentStatus);
                if (current != null)
                {
                    cmbStatus.SelectedValue = current.OrderStatusId;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbStatus.SelectedValue == null)
            {
                MessageBox.Show("Выберите статус!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new FranchiseDBEntities1())
                {
                    var order = db.Orders.Find(_orderId);
                    if (order == null)
                    {
                        MessageBox.Show("Заявка не найдена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        DialogResult = false;
                        Close();
                        return;
                    }

                    // Обновляем только статус
                    order.OrderStatusId = Convert.ToInt32(cmbStatus.SelectedValue);

                    db.SaveChanges();

                    MessageBox.Show("Статус заявки обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}