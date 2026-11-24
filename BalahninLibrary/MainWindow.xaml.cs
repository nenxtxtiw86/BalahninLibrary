using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BalahninLibrary
{
    public partial class MainWindow : Window
    {
        // Флаг, чтобы не проверять роль каждый раз при переходе
        private bool _isDirector = false;

        public MainWindow()
        {
            InitializeComponent();

            // Сначала — вход
            var login = new LoginWindow();
            if (login.ShowDialog() != true)
            {
                Application.Current.Shutdown();
                return;
            }

            // Проверяем, кто вошёл
            if (Manager.CurrentEmployee != null)
            {
                var emp = Manager.CurrentEmployee;
                TbCurrentUser.Text = $"Вы вошли как: {emp.Surname} {emp.FirstName} - {emp.Position}";

                // Настройка прав доступа: только директор видит кнопку Фонды
                _isDirector = emp.Position == "Директор" || emp.Position == "Администратор";

                if (!_isDirector)
                {
                    BtnFunds.Visibility = Visibility.Collapsed;
                }
            }

            // Подписываемся на смену страниц
            MainFrame.Navigated += MainFrame_Navigated;

            // Стартовая страница
            MainFrame.Navigate(new LibraryItemPage());
            Manager.MainFrame = MainFrame;
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // 1. Кнопку "Архив" скрываем только на странице архива
            BtnArchive.Visibility = (e.Content is ArchivePage) ? Visibility.Collapsed : Visibility.Visible;

            // 2. Кнопку "Фонды" скрываем на странице фондов, 
            // и ВООБЩЕ не показываем, если это не директор!
            if (e.Content is LibraryFundPage || !_isDirector)
            {
                BtnFunds.Visibility = Visibility.Collapsed;
            }
            else
            {
                BtnFunds.Visibility = Visibility.Visible;
            }
        }

        private void OpenFunds_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LibraryFundPage());
        }

        private void OpenArchive_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ArchivePage());
        }
    }
}