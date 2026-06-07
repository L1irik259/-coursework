using System;
using System.Windows;

namespace FranchAdm
{
    public partial class PhotoUrlWindow : Window
    {
        public string PhotoUrl { get; private set; } = string.Empty;

        public PhotoUrlWindow(string currentUrl)
        {
            InitializeComponent();
            txtUrl.Text = currentUrl;
            // Owner устанавливается автоматически при ShowDialog()
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            string url = txtUrl.Text.Trim();

            if (!string.IsNullOrWhiteSpace(url))
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    MessageBox.Show("URL должен начинаться с http:// или https://",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (url.Length > 500)
                {
                    MessageBox.Show("URL слишком длинный (макс. 500 символов)",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            PhotoUrl = url;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}