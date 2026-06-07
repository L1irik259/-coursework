using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FranchAdm
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Устанавливаем иконку для всех окон
            var uri = new Uri("/Resources/raketa.ico", UriKind.Relative);
            this.MainWindow = new Window();
            this.MainWindow.Icon = new System.Windows.Media.Imaging.BitmapImage(uri);
        }
    }
}
