using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FranchAdm.Db;

namespace FranchAdm
{
    public partial class FranchiseEditWindow : Window
    {
        private const string PLACEHOLDER_IMAGE_URL = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQpC6vJ1vmEKcffQOvf-BAWkfSeAVxAkFNdNA&s";
        private const int MAX_NAME_LENGTH = 200;
        private const int MAX_DESCRIPTION_LENGTH = 2000;
        private const decimal MAX_PRICE = 1000000000;
        private const decimal MAX_INVESTMENT = 10000000000;
        private const decimal MAX_PERCENT = 100;
        private const int MAX_PAYBACK = 600;
        private const int MAX_URL_LENGTH = 500;

        private readonly int _franchiseId;
        private readonly bool _isEditMode;
        private string _currentPhotoUrl = string.Empty;
        private decimal _oldPrice = 0;
        private bool _priceChanged = false;

        public FranchiseEditWindow(int franchiseId = 0)
        {
            try
            {
                InitializeComponent();
                _franchiseId = franchiseId;
                _isEditMode = franchiseId > 0;

                if (_isEditMode)
                {
                    lblTitle.Text = "Редактирование франшизы";
                    LoadFranchiseData();
                }
                else
                {
                    lblTitle.Text = "Добавление франшизы";
                    LoadComboBoxes();
                }
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при инициализации окна", ex);
                DialogResult = false;
                Close();
            }
        }

        private void LoadComboBoxes()
        {
            try
            {
                using (var db = new FranchiseDBEntities1())
                {
                    cmbCategory.ItemsSource = db.Categories.OrderBy(c => c.Name).ToList();
                    cmbCategory.DisplayMemberPath = "Name";
                    cmbCategory.SelectedValuePath = "CategoryId";

                    cmbFranchiser.ItemsSource = db.Franchisers.OrderBy(f => f.Name).ToList();
                    cmbFranchiser.DisplayMemberPath = "Name";
                    cmbFranchiser.SelectedValuePath = "FranchiserId";

                    cmbFranType.ItemsSource = db.FranTypes.OrderBy(t => t.Name).ToList();
                    cmbFranType.DisplayMemberPath = "Name";
                    cmbFranType.SelectedValuePath = "FranTypeId";

                    cmbFranStatus.ItemsSource = db.FranStatuses.OrderBy(s => s.Name).ToList();
                    cmbFranStatus.DisplayMemberPath = "Name";
                    cmbFranStatus.SelectedValuePath = "FranStatusId";

                    var premTypes = db.PremTypeses.OrderBy(t => t.Name).ToList();
                    cmbPremType.ItemsSource = premTypes;
                    cmbPremType.DisplayMemberPath = "Name";  // ← Показываем название
                    cmbPremType.SelectedValuePath = "PremTypesId";

                    var tags = db.Tags.OrderBy(t => t.Name).ToList();
                    lstTags.ItemsSource = tags;
                    lstTags.DisplayMemberPath = "Name";
                    lstTags.SelectedValuePath = "TagId";
                }
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при загрузке справочников", ex);
            }
        }


        private void LoadFranchiseData()
        {
            try
            {
                using (var db = new FranchiseDBEntities1())
                {
                    var franchise = db.Franchises.FirstOrDefault(f => f.FranchiseId == _franchiseId);

                    if (franchise == null)
                    {
                        MessageBox.Show("Франшиза не найдена!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        DialogResult = false;
                        Close();
                        return;
                    }

                    // Загружаем все справочники
                    var categoryList = db.Categories.OrderBy(c => c.Name).ToList();
                    var franchiserList = db.Franchisers.OrderBy(f => f.Name).ToList();
                    var franTypeList = db.FranTypes.OrderBy(t => t.Name).ToList();
                    var franStatusList = db.FranStatuses.OrderBy(s => s.Name).ToList();
                    var premTypesList = db.PremTypeses.OrderBy(p => p.Name).ToList();
                    var tagsList = db.Tags.OrderBy(t => t.Name).ToList();
                    var franchiseTags = db.FranchiseTags.Where(ft => ft.FranchiseId == _franchiseId).Select(ft => ft.TagId).ToList();

                    // Заполняем ComboBoxes
                    cmbCategory.ItemsSource = categoryList;
                    cmbCategory.DisplayMemberPath = "Name";
                    cmbCategory.SelectedValuePath = "CategoryId";

                    cmbFranchiser.ItemsSource = franchiserList;
                    cmbFranchiser.DisplayMemberPath = "Name";
                    cmbFranchiser.SelectedValuePath = "FranchiserId";

                    cmbFranType.ItemsSource = franTypeList;
                    cmbFranType.DisplayMemberPath = "Name";
                    cmbFranType.SelectedValuePath = "FranTypeId";

                    cmbFranStatus.ItemsSource = franStatusList;
                    cmbFranStatus.DisplayMemberPath = "Name";
                    cmbFranStatus.SelectedValuePath = "FranStatusId";

                    cmbPremType.ItemsSource = premTypesList;
                    cmbPremType.DisplayMemberPath = "Name";
                    cmbPremType.SelectedValuePath = "PremTypesId";

                    lstTags.ItemsSource = tagsList;
                    lstTags.DisplayMemberPath = "Name";
                    lstTags.SelectedValuePath = "TagId";

                    // Запускаем таймер для установки значений
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromMilliseconds(200);
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();

                        // Текстовые поля
                        txtName.Text = SafeString(franchise.Name, MAX_NAME_LENGTH);
                        txtDescription.Text = SafeString(franchise.Description, MAX_DESCRIPTION_LENGTH);
                        txtFinalPrice.Text = franchise.FinalPrice > 0 ? franchise.FinalPrice.ToString() : "";
                        txtInvestmentAmount.Text = franchise.InvestmentAmount > 0 ? franchise.InvestmentAmount.ToString() : "";
                        txtPledgeAmount.Text = franchise.PledgeAmount > 0 ? franchise.PledgeAmount.ToString() : "";
                        txtRoyaltyPercent.Text = franchise.RoyaltyPercent > 0 ? franchise.RoyaltyPercent.ToString() : "";
                        txtPaybackPeriod.Text = franchise.PaybackPeriod > 0 ? franchise.PaybackPeriod.ToString() : "";
                        txtDiscountPercent.Text = franchise.DiscountPercent > 0 ? franchise.DiscountPercent.ToString() : "";

                        // ✅ Выбираем значения
                        cmbCategory.SelectedValue = franchise.CategoryId;
                        cmbFranchiser.SelectedValue = franchise.FranchiserId;

                        if (franchise.FranTypeId > 0)
                            cmbFranType.SelectedValue = franchise.FranTypeId;

                        cmbFranStatus.SelectedValue = franchise.FranStatusId;

                        // ✅ Тип помещения
                        if (franchise.PremTypesId > 0)
                            cmbPremType.SelectedValue = franchise.PremTypesId;

                        // Теги
                        lstTags.SelectedItems.Clear();
                        foreach (var tagId in franchiseTags)
                        {
                            var tagItem = lstTags.Items.Cast<dynamic>().FirstOrDefault(t => t.TagId == tagId);
                            if (tagItem != null)
                                lstTags.SelectedItems.Add(tagItem);
                        }

                        // Фото
                        _currentPhotoUrl = string.IsNullOrWhiteSpace(franchise.MinPhotoUrl) ? PLACEHOLDER_IMAGE_URL : franchise.MinPhotoUrl;
                        LoadImage(_currentPhotoUrl);
                        _oldPrice = franchise.FinalPrice;
                    };
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при загрузке данных франшизы", ex);
                DialogResult = false;
                Close();
            }
        }

        private void LoadImage(string url)
        {
            try
            {
                string imageUrl = string.IsNullOrWhiteSpace(url) ? PLACEHOLDER_IMAGE_URL : url;

                if (!imageUrl.StartsWith("http://") && !imageUrl.StartsWith("https://"))
                {
                    ShowPlaceholder();
                    return;
                }

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageUrl);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 200;
                bitmap.DecodePixelHeight = 200;
                bitmap.EndInit();
                bitmap.Freeze();

                imgPreview.Source = bitmap;
                imgPreview.Visibility = Visibility.Visible;
                txtNoImage.Visibility = Visibility.Collapsed;
            }
            catch
            {
                ShowPlaceholder();
            }
        }

        private void ShowPlaceholder()
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(PLACEHOLDER_IMAGE_URL);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 200;
                bitmap.DecodePixelHeight = 200;
                bitmap.EndInit();
                bitmap.Freeze();

                imgPreview.Source = bitmap;
                imgPreview.Visibility = Visibility.Visible;
                txtNoImage.Visibility = Visibility.Collapsed;
            }
            catch
            {
                txtNoImage.Text = "Нет фото";
                txtNoImage.Visibility = Visibility.Visible;
                imgPreview.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnUploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            var urlWindow = new PhotoUrlWindow(_currentPhotoUrl == PLACEHOLDER_IMAGE_URL ? "" : _currentPhotoUrl);

            if (urlWindow.ShowDialog() == true)
            {
                _currentPhotoUrl = string.IsNullOrWhiteSpace(urlWindow.PhotoUrl) ? PLACEHOLDER_IMAGE_URL : urlWindow.PhotoUrl;
                LoadImage(_currentPhotoUrl);
            }
        }

        private void CheckPriceChange()
        {
            try
            {
                if (_isEditMode && decimal.TryParse(txtFinalPrice.Text, out decimal newPrice))
                {
                    if (newPrice != _oldPrice)
                    {
                        _priceChanged = true;
                        txtOldPrice.Text = _oldPrice.ToString("N0");
                        txtNewPrice.Text = newPrice.ToString("N0");
                        brdPriceHistory.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        _priceChanged = false;
                        brdPriceHistory.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch { }
        }

        private bool ValidateInputs()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    ShowValidationError("Введите название франшизы", txtName);
                    return false;
                }

                txtName.Text = txtName.Text.Trim();
                if (txtName.Text.Length > MAX_NAME_LENGTH)
                {
                    ShowValidationError($"Название не должно превышать {MAX_NAME_LENGTH} символов", txtName);
                    return false;
                }

                if (txtName.Text.Length < 3)
                {
                    ShowValidationError("Название должно содержать не менее 3 символов", txtName);
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(txtDescription.Text))
                {
                    txtDescription.Text = txtDescription.Text.Trim();
                    if (txtDescription.Text.Length > MAX_DESCRIPTION_LENGTH)
                    {
                        ShowValidationError($"Описание не должно превышать {MAX_DESCRIPTION_LENGTH} символов", txtDescription);
                        return false;
                    }
                }

                if (cmbCategory.SelectedValue == null)
                {
                    ShowValidationError("Выберите категорию", cmbCategory);
                    return false;
                }

                if (cmbFranchiser.SelectedValue == null)
                {
                    ShowValidationError("Выберите франчайзера", cmbFranchiser);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtFinalPrice.Text))
                {
                    ShowValidationError("Введите цену", txtFinalPrice);
                    return false;
                }

                if (!decimal.TryParse(txtFinalPrice.Text, out decimal price))
                {
                    ShowValidationError("Неверный формат цены", txtFinalPrice);
                    return false;
                }

                if (price < 0 || price > MAX_PRICE)
                {
                    ShowValidationError($"Цена должна быть от 0 до {MAX_PRICE:N0} ₽", txtFinalPrice);
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(txtInvestmentAmount.Text))
                {
                    if (!decimal.TryParse(txtInvestmentAmount.Text, out decimal investment))
                    {
                        ShowValidationError("Неверный формат инвестиций", txtInvestmentAmount);
                        return false;
                    }
                    if (investment < 0 || investment > MAX_INVESTMENT)
                    {
                        ShowValidationError($"Инвестиции от 0 до {MAX_INVESTMENT:N0} ₽", txtInvestmentAmount);
                        return false;
                    }
                }

                if (!string.IsNullOrWhiteSpace(txtPledgeAmount.Text))
                {
                    if (!decimal.TryParse(txtPledgeAmount.Text, out decimal pledge))
                    {
                        ShowValidationError("Неверный формат паушального взноса", txtPledgeAmount);
                        return false;
                    }
                    if (pledge < 0)
                    {
                        ShowValidationError("Паушальный взнос не может быть отрицательным", txtPledgeAmount);
                        return false;
                    }
                }

                if (!string.IsNullOrWhiteSpace(txtRoyaltyPercent.Text))
                {
                    if (!decimal.TryParse(txtRoyaltyPercent.Text, out decimal royalty))
                    {
                        ShowValidationError("Неверный формат роялти", txtRoyaltyPercent);
                        return false;
                    }
                    if (royalty < 0 || royalty > MAX_PERCENT)
                    {
                        ShowValidationError($"Роялти от 0 до {MAX_PERCENT}%", txtRoyaltyPercent);
                        return false;
                    }
                }

                if (!string.IsNullOrWhiteSpace(txtPaybackPeriod.Text))
                {
                    if (!int.TryParse(txtPaybackPeriod.Text, out int payback))
                    {
                        ShowValidationError("Неверный формат окупаемости", txtPaybackPeriod);
                        return false;
                    }
                    if (payback < 0 || payback > MAX_PAYBACK)
                    {
                        ShowValidationError($"Окупаемость от 0 до {MAX_PAYBACK} мес.", txtPaybackPeriod);
                        return false;
                    }
                }

                if (!string.IsNullOrWhiteSpace(txtDiscountPercent.Text))
                {
                    if (!decimal.TryParse(txtDiscountPercent.Text, out decimal discount))
                    {
                        ShowValidationError("Неверный формат скидки", txtDiscountPercent);
                        return false;
                    }
                    if (discount < 0 || discount > MAX_PERCENT)
                    {
                        ShowValidationError($"Скидка от 0 до {MAX_PERCENT}%", txtDiscountPercent);
                        return false;
                    }
                }

                if (!string.IsNullOrWhiteSpace(_currentPhotoUrl) && _currentPhotoUrl != PLACEHOLDER_IMAGE_URL)
                {
                    if (_currentPhotoUrl.Length > MAX_URL_LENGTH)
                    {
                        ShowValidationError($"URL фото слишком длинный (макс. {MAX_URL_LENGTH} символов)", txtName);
                        return false;
                    }
                    if (!_currentPhotoUrl.StartsWith("http://") && !_currentPhotoUrl.StartsWith("https://"))
                    {
                        ShowValidationError("URL фото должен начинаться с http:// или https://", txtName);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                HandleError("Ошибка при валидации", ex);
                return false;
            }
        }

        private void ShowValidationError(string message, Control control)
        {
            MessageBox.Show(message, "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            control.Focus();
            control.BorderBrush = System.Windows.Media.Brushes.Red;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            var result = MessageBox.Show(
                _isEditMode ? "Сохранить изменения?" : "Добавить новую франшизу?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new FranchiseDBEntities1())
                {
                    Franchise franchise;

                    if (_isEditMode)
                    {
                        franchise = db.Franchises.Find(_franchiseId);
                        if (franchise == null)
                        {
                            MessageBox.Show("Франшиза не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            DialogResult = false;
                            Close();
                            return;
                        }
                    }
                    else
                    {
                        franchise = new Franchise();
                        franchise.CreatedAt = DateTime.Now;
                        franchise.Article = $"ART-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                    }

                    franchise.Name = txtName.Text.Trim();
                    franchise.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();
                    franchise.FinalPrice = decimal.TryParse(txtFinalPrice.Text, out decimal price) ? price : 0;
                    franchise.InvestmentAmount = decimal.TryParse(txtInvestmentAmount.Text, out decimal investment) ? investment : 0;
                    franchise.PledgeAmount = decimal.TryParse(txtPledgeAmount.Text, out decimal pledge) ? pledge : 0;
                    franchise.RoyaltyPercent = decimal.TryParse(txtRoyaltyPercent.Text, out decimal royalty) ? royalty : 0;
                    franchise.PaybackPeriod = int.TryParse(txtPaybackPeriod.Text, out int payback) ? payback : 0;
                    franchise.DiscountPercent = decimal.TryParse(txtDiscountPercent.Text, out decimal discount) ? discount : 0;

                    franchise.CategoryId = cmbCategory.SelectedValue != null ? Convert.ToInt32(cmbCategory.SelectedValue) : 0;
                    franchise.FranchiserId = cmbFranchiser.SelectedValue != null ? Convert.ToInt32(cmbFranchiser.SelectedValue) : 0;
                    franchise.FranTypeId = cmbFranType.SelectedValue != null ? Convert.ToInt32(cmbFranType.SelectedValue) : 0;
                    franchise.FranStatusId = cmbFranStatus.SelectedValue != null ? Convert.ToInt32(cmbFranStatus.SelectedValue) : 1;
                    franchise.PremTypesId = cmbPremType.SelectedValue != null ? Convert.ToInt32(cmbPremType.SelectedValue) : 0;

                    franchise.MinPhotoUrl = string.IsNullOrWhiteSpace(_currentPhotoUrl) || _currentPhotoUrl == PLACEHOLDER_IMAGE_URL
                        ? PLACEHOLDER_IMAGE_URL
                        : _currentPhotoUrl;

                    if (_isEditMode)
                    {
                        if (_priceChanged && decimal.TryParse(txtFinalPrice.Text, out decimal newPrice))
                        {
                            var priceHistory = new PriceHistory
                            {
                                FranchiseId = _franchiseId,
                                OldPrice = _oldPrice,
                                NewPrice = newPrice
                            };
                            db.PriceHistories.Add(priceHistory);
                        }

                        var existingTags = db.FranchiseTags.Where(ft => ft.FranchiseId == _franchiseId).ToList();
                        db.FranchiseTags.RemoveRange(existingTags);

                        var selectedTagIds = lstTags.SelectedItems.Cast<dynamic>().Select(t => t.TagId).ToList();
                        foreach (var tagId in selectedTagIds)
                        {
                            db.FranchiseTags.Add(new FranchiseTag { FranchiseId = _franchiseId, TagId = tagId });
                        }

                        db.Entry(franchise).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        db.Franchises.Add(franchise);
                    }

                    db.SaveChanges();

                    MessageBox.Show(_isEditMode ? "Франшиза обновлена!" : "Франшиза добавлена!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                // Детали валидации EF
                string errors = string.Join("\n", dbEx.EntityValidationErrors.SelectMany(ev =>
                    ev.ValidationErrors.Select(v => $"Поле '{v.PropertyName}': {v.ErrorMessage}")));
                MessageBox.Show($"Ошибка валидации базы данных:\n{errors}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                // Детали SQL ошибки
                string sqlError = $"Код ошибки: {sqlEx.Number}\n{sqlEx.Message}";
                if (sqlEx.InnerException != null)
                {
                    sqlError += $"\n\nВнутренняя ошибка:\n{sqlEx.InnerException.Message}";
                }
                MessageBox.Show($"Ошибка базы данных:\n{sqlError}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Data.Entity.Core.UpdateException updateEx)
            {
                // Ошибка обновления (внешние ключи, ограничения)
                string updateError = updateEx.Message;
                if (updateEx.InnerException != null)
                {
                    updateError += $"\n\nВнутренняя ошибка:\n{updateEx.InnerException.Message}";

                    // Пробуем достать ещё глубже
                    if (updateEx.InnerException.InnerException != null)
                    {
                        updateError += $"\n\nДетали:\n{updateEx.InnerException.InnerException.Message}";
                    }
                }
                MessageBox.Show($"Ошибка при обновлении данных:\n{updateError}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Общая ошибка с деталями
                string errorMsg = $"{ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMsg += $"\n\nВнутренняя ошибка:\n{ex.InnerException.Message}";

                    if (ex.InnerException.InnerException != null)
                    {
                        errorMsg += $"\n\nДетали:\n{ex.InnerException.InnerException.Message}";
                    }
                }

                errorMsg += $"\n\nСтек вызовов:\n{ex.StackTrace}";

                MessageBox.Show($"Ошибка при сохранении:\n{errorMsg}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void TxtFinalPrice_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !IsTextAllowed(e.Text);
        private void TxtInvestmentAmount_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !IsTextAllowed(e.Text);
        private void TxtPledgeAmount_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !IsTextAllowed(e.Text);
        private void TxtRoyaltyPercent_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !IsTextAllowed(e.Text);
        private void TxtPaybackPeriod_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !IsTextAllowed(e.Text);
        private void TxtDiscountPercent_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !IsTextAllowed(e.Text);

        private static bool IsTextAllowed(string text) => Regex.IsMatch(text, @"^[0-9]+$");

        private string SafeString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            value = value.Trim();
            return value.Length > maxLength ? value.Substring(0, maxLength) : value;
        }

        private void HandleError(string context, Exception ex)
        {
            MessageBox.Show($"{context}\n\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}