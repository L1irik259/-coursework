using System.Windows;
using System.Windows.Controls;
using FranchAdm;
using FranchAdm.Views;


namespace FranchAdm
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            lblUserRole.Text = string.Format("{0} | {1}", UserSession.FullName, UserSession.RoleName);

            NavigateToPage("Franchises");
        }

        private void NavigateToPage(string pageTag)
        {
            UserControl selectedPage = null;
            string pageTitle = string.Empty;

            switch (pageTag)
            {
                case "Products":
                case "Franchises":
                    selectedPage = new FranshiseListPage();
                    pageTitle = "📦 Управление франшизами";
                    break;

                case "Orders":
                    selectedPage = new OrderListPage();
                    pageTitle = "🛒 Управление заявками";
                    break;

    

                // В методе NavigateToPage добавь case:
                case "PriceHistory":
                    selectedPage = new PriceHistoryPage();
                    pageTitle = "📊 История изменения цен";
                    break;


                case "Categories":
                    selectedPage = new CategoryPage();
                    pageTitle = "📂 Категории";
                    break;

                case "Franchisers":
                    selectedPage = new FranchiserPage();
                    pageTitle = "🏭 Франчайзеры";
                    break;

                case "FranTypes":
                    selectedPage = new FranTypePage();
                    pageTitle = "📋 Типы франшиз";
                    break;

                case "FranStatuses":
                    selectedPage = new FranStatusPage();
                    pageTitle = "📊 Статусы франшиз";
                    break;

                case "PremTypes":
                    selectedPage = new PremTypePage();
                    pageTitle = "🏢 Типы помещений";
                    break;

                case "Tags":
                    selectedPage = new TagPage();
                    pageTitle = "🏷️ Теги";
                    break;

                case "Regions":
                    selectedPage = new RegionPage();
                    pageTitle = "📍 Регионы";
                    break;

                case "ContactMethods":
                    selectedPage = new ContactMethodPage();
                    pageTitle = "📞 Способы связи";
                    break;

                case "OrderStatuses":
                    selectedPage = new OrderStatusPage();
                    pageTitle = "📝 Статусы заказов";
                    break;

                // === УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ (только Админ) ===
                case "Users":
                    selectedPage = new UsersPage();
                    pageTitle = "👥 Пользователи";
                    break;

                default:
                    pageTitle = "Главная";
                    break;
            }

            if (selectedPage != null)
            {
                ContentArea.Content = selectedPage;
                lblPageTitle.Text = pageTitle;
            }
            else
            {
                // Заглушка для ещё не созданных страниц
                ContentArea.Content = CreatePlaceholderPage(pageTitle);
                lblPageTitle.Text = pageTitle;
            }
        }

        // Создаём временную заглушку для страниц
        private UIElement CreatePlaceholderPage(string title)
        {
            Grid grid = new Grid();
            grid.Margin = new Thickness(30);

            TextBlock textBlock = new TextBlock();
            textBlock.Text = $"Страница \"{title}\" будет реализована на следующем шаге";
            textBlock.FontSize = 18;
            textBlock.Foreground = System.Windows.Media.Brushes.Gray;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.TextWrapping = TextWrapping.Wrap;

            grid.Children.Add(textBlock);
            return grid;
        }

        // === ОБРАБОТЧИКИ КНОПОК МЕНЮ ===

        // Основные разделы
        private void BtnProducts_Click(object sender, RoutedEventArgs e) => NavigateToPage("Franchises");
        private void BtnOrders_Click(object sender, RoutedEventArgs e) => NavigateToPage("Orders");
        private void BtnSupplies_Click(object sender, RoutedEventArgs e) => NavigateToPage("Supplies");

        // Обработчик кнопки истории цен
        private void BtnPriceHistory_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage("PriceHistory");
        }

        // Справочники
        private void BtnCategories_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAdminAccess("справочников"))
                NavigateToPage("Categories");
        }

        private void BtnFranchisers_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAdminAccess("справочников"))
                NavigateToPage("Franchisers");
        }

        private void BtnFranTypes_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAdminAccess("справочников"))
                NavigateToPage("FranTypes");
        }

        private void BtnFranStatuses_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAdminAccess("справочников"))
                NavigateToPage("FranStatuses");
        }

        private void BtnPremTypes_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAdminAccess("справочников"))
                NavigateToPage("PremTypes");
        }

        private void BtnTags_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAdminAccess("справочников"))
                NavigateToPage("Tags");
        }

        private void BtnRegions_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAdminAccess("справочников"))
                NavigateToPage("Regions");
        }

        private void BtnContactMethods_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAdminAccess("справочников"))
                NavigateToPage("ContactMethods");
        }

        private void BtnOrderStatuses_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAdminAccess("справочников"))
                NavigateToPage("OrderStatuses");
        }

        // Пользователи
        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAdminAccess("управления пользователями"))
                NavigateToPage("Users");
        }

        // === ПРОВЕРКА ПРАВ ДОСТУПА ===
        private bool CheckAdminAccess(string feature)
        {
            if (!UserSession.IsAdmin)
            {
                MessageBox.Show(
                    $"Доступ к {feature} разрешён только Администраторам.\n" +
                    $"Ваша роль: {UserSession.RoleName}",
                    "Доступ запрещён",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        // === КНОПКА ВЫХОДА ===
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы действительно хотите выйти?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                UserSession.Clear();

                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}