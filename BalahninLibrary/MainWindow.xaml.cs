using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation; // Обязательно добавь этот using

namespace BalahninLibrary
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Подписываемся на событие смены страниц
            MainFrame.Navigated += MainFrame_Navigated;

            // Открываем стартовую страницу
            MainFrame.Navigate(new LibraryItemPage());
            Manager.MainFrame = MainFrame;
        }

        // Событие, которое срабатывает при каждом переходе
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Если открыта страница "Фонды" — прячем кнопку "Фонды"
            if (e.Content is LibraryFundPage)
            {
                BtnFunds.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Если мы на любой другой странице — показываем кнопку "Фонды"
                BtnFunds.Visibility = Visibility.Visible;
            }
        }

        private void OpenFunds_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LibraryFundPage());
        }
    }
}