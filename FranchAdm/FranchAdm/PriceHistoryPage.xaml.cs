using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FranchAdm.Db;

namespace FranchAdm
{
    public partial class PriceHistoryPage : UserControl
    {
        private DataTable historyTable;
        private bool _isLoading = false;

        public PriceHistoryPage()
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
                    var query = db.PriceHistories
                        .OrderByDescending(ph => ph.ChangeAd)
                        .Select(ph => new
                        {
                            ph.PriceHistoryId,
                            FranchiseName = ph.Franchise != null ? ph.Franchise.Name : "Удалено",
                            ph.OldPrice,
                            ph.NewPrice,
                            ph.ChangeAd,  // ✅ ИСПРАВЛЕНО: ChangedAt → ChangeAd
                            ph.Reason,    // ✅ Добавлено поле Reason
                            Difference = ph.NewPrice - ph.OldPrice,
                            DifferencePercent = ph.OldPrice != 0 ? ((ph.NewPrice - ph.OldPrice) / ph.OldPrice * 100) : 0
                        })
                        .ToList();

                    historyTable = new DataTable();
                    historyTable.Columns.Add("PriceHistoryId", typeof(int));
                    historyTable.Columns.Add("FranchiseName", typeof(string));
                    historyTable.Columns.Add("OldPrice", typeof(decimal));
                    historyTable.Columns.Add("NewPrice", typeof(decimal));
                    historyTable.Columns.Add("ChangeAd", typeof(DateTime));  // ✅ ИСПРАВЛЕНО
                    historyTable.Columns.Add("Reason", typeof(string));       // ✅ Добавлено
                    historyTable.Columns.Add("Difference", typeof(decimal));
                    historyTable.Columns.Add("DifferencePercent", typeof(decimal));

                    foreach (var item in query)
                    {
                        historyTable.Rows.Add(
                            item.PriceHistoryId,
                            item.FranchiseName,
                            item.OldPrice,
                            item.NewPrice,
                            item.ChangeAd,  // ✅ ИСПРАВЛЕНО
                            item.Reason,    // ✅ Добавлено
                            item.Difference,
                            item.DifferencePercent
                        );
                    }

                    lvHistory.ItemsSource = historyTable.DefaultView;
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
                if (_isLoading || historyTable == null) return;

                DataView view = historyTable.DefaultView;
                string filter = "";

                // Поиск по франшизе
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    string searchText = txtSearch.Text.Trim();
                    if (searchText.Length > 100)
                    {
                        searchText = searchText.Substring(0, 100);
                        txtSearch.Text = searchText;
                    }
                    searchText = searchText.Replace("'", "''");
                    filter += $"FranchiseName LIKE '%{searchText}%'";
                }

                // Фильтр по дате
                if (dpFromDate.SelectedDate.HasValue)
                {
                    DateTime fromDate = dpFromDate.SelectedDate.Value.Date;
                    string dateFilter = $"ChangeAd >= '#{fromDate:yyyy-MM-dd}#'";  // ✅ ИСПРАВЛЕНО

                    if (!string.IsNullOrWhiteSpace(filter))
                        filter += " AND " + dateFilter;
                    else
                        filter = dateFilter;
                }

                view.RowFilter = filter;
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при фильтрации", ex);
                if (historyTable != null)
                    historyTable.DefaultView.RowFilter = "";
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

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoading)
                ApplyFilters();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            dpFromDate.SelectedDate = null;
            ApplyFilters();
        }

        private void HandleError(string context, Exception ex)
        {
            string msg = $"{context}\n\n{ex.Message}";

            if (ex.InnerException != null)
            {
                msg += $"\n\nДетали: {ex.InnerException.Message}";

                if (ex.InnerException.InnerException != null)
                {
                    msg += $"\n{ex.InnerException.InnerException.Message}";
                }
            }

#if DEBUG
            msg += $"\n\n{ex.GetType().Name}";
#endif
            MessageBox.Show(msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}