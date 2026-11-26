using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BalahninLibrary
{
    public partial class ArchivePage : Page
    {
        // Флаг, чтобы не проверять роль каждый раз
        private bool _isDirector = false;

        public ArchivePage()
        {
            InitializeComponent();

            // Проверяем роль текущего пользователя
            if (Manager.CurrentEmployee != null)
            {
                _isDirector = Manager.CurrentEmployee.Position == "Директор" || Manager.CurrentEmployee.Position == "Администратор";
            }

            LoadArchive();
        }

        private void LoadArchive()
        {
            ApplyFilters(); // Загружаем сразу с учетом фильтров (по умолчанию пустые)
        }

        // Основной метод фильтрации (и по тексту, и по датам)
        private void ApplyFilters()
        {
            var context = LibraryBalahninEntities.GetContext();
            var query = context.ArchiveLibraryItem.AsQueryable();

            // ГЛАВНАЯ ФИЛЬТРАЦИЯ: Если это НЕ директор, показываем только его библиотеку
            if (!_isDirector && Manager.CurrentEmployee != null)
            {
                int currentLibraryId = Manager.CurrentEmployee.LibraryID;
                query = query.Where(a => a.LibraryFund.LibraryID == currentLibraryId);
            }

            // 1. Фильтр по тексту
            string search = SearchTextBox.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a =>
                    (a.Title != null && a.Title.ToLower().Contains(search)) ||
                    (a.Author != null && a.Author.ToLower().Contains(search)) ||
                    (a.InventoryNumber != null && a.InventoryNumber.ToLower().Contains(search)) ||
                    (a.Reason != null && a.Reason.ToLower().Contains(search)) ||
                    (a.Employees != null && a.Employees.Surname != null && a.Employees.Surname.ToLower().Contains(search))
                );
            }

            // 2. Фильтр по датам
            if (StartDatePicker.SelectedDate.HasValue)
            {
                var start = StartDatePicker.SelectedDate.Value;
                query = query.Where(a => a.ArchivedDate >= start);
            }
            if (EndDatePicker.SelectedDate.HasValue)
            {
                var end = EndDatePicker.SelectedDate.Value;
                query = query.Where(a => a.ArchivedDate <= end);
            }

            // Применяем сортировку (сначала новые)
            var result = query.OrderByDescending(a => a.ArchivedDate).ToList();

            ListViewArchive.ItemsSource = result;

            // Выводим статистику
            int totalCount = result.Count;
            CountTextBlock.Text = $"Найдено записей: {totalCount}";
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Проверка, чтобы не было ошибки при инициализации страницы
            if (IsLoaded)
                ApplyFilters();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            ApplyFilters();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}