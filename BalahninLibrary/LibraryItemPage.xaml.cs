using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BalahninLibrary
{
    public partial class LibraryItemPage : Page
    {
        private bool _isDirector = false;

        public LibraryItemPage()
        {
            InitializeComponent();

            if (Manager.CurrentEmployee != null)
            {
                _isDirector = Manager.CurrentEmployee.Position == "Директор" || Manager.CurrentEmployee.Position == "Администратор";
            }

            // Если это НЕ директор, прячем выбор библиотек
            if (!_isDirector)
            {
                LibraryFilterPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                LoadLibraries();
            }

            LoadFundsFilter(); // Первичная загрузка фондов
            StatusComboBox.SelectedIndex = 0;
            LibraryUpdate();
        }

        // 1. Загрузка списка библиотек (только для директора)
        private void LoadLibraries()
        {
            var context = LibraryBalahninEntities.GetContext();
            var libraries = context.Library.OrderBy(l => l.Name).ToList();

            LibraryComboBox.Items.Clear();
            LibraryComboBox.Items.Add(new ComboBoxItem { Content = "Все библиотеки" });

            foreach (var lib in libraries)
            {
                LibraryComboBox.Items.Add(new ComboBoxItem { Content = lib.Name, Tag = lib.LibraryID });
            }

            LibraryComboBox.SelectedIndex = 0;
        }

        // 2. Загрузка списка фондов (зависит от выбранной библиотеки)
        private void LoadFundsFilter()
        {
            var context = LibraryBalahninEntities.GetContext();
            var fundsQuery = context.LibraryFund.AsQueryable();

            if (_isDirector)
            {
                // Если директор выбрал конкретную библиотеку — фильтруем фонды по ней
                if (LibraryComboBox.SelectedIndex > 0 && LibraryComboBox.SelectedItem is ComboBoxItem selectedLib && selectedLib.Tag is int libId)
                {
                    fundsQuery = fundsQuery.Where(f => f.LibraryID == libId);
                }
            }
            else if (Manager.CurrentEmployee != null)
            {
                // Если обычный сотрудник — только фонды его библиотеки
                int currentLibraryId = Manager.CurrentEmployee.LibraryID;
                fundsQuery = fundsQuery.Where(f => f.LibraryID == currentLibraryId);
            }

            var funds = fundsQuery.Select(f => f.FundName).Distinct().ToList();

            FundComboBox.Items.Clear();
            FundComboBox.Items.Add(new ComboBoxItem { Content = "Все фонды" });

            foreach (var fundName in funds)
            {
                FundComboBox.Items.Add(new ComboBoxItem { Content = fundName });
            }

            FundComboBox.SelectedIndex = 0;
        }

        private void LibraryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadFundsFilter(); // Меняем список фондов
            LibraryUpdate();   // И сразу фильтруем книги
        }

        private void LibraryUpdate()
        {
            var context = LibraryBalahninEntities.GetContext();
            var query = context.LibraryItem.AsQueryable();

            // Фильтр по библиотеке
            if (_isDirector)
            {
                if (LibraryComboBox.SelectedIndex > 0 && LibraryComboBox.SelectedItem is ComboBoxItem selectedLib && selectedLib.Tag is int libId)
                {
                    query = query.Where(p => p.LibraryFund.LibraryID == libId);
                }
            }
            else if (Manager.CurrentEmployee != null)
            {
                int currentLibraryId = Manager.CurrentEmployee.LibraryID;
                query = query.Where(p => p.LibraryFund.LibraryID == currentLibraryId);
            }

            var _current = query.ToList();

            // Поиск
            string searchText = SearchTextBox.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                _current = _current.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(searchText))
                    || (p.Author != null && p.Author.ToLower().Contains(searchText))
                    || (p.InventoryNumber != null && p.InventoryNumber.ToLower().Contains(searchText))
                    || (p.Status != null && p.Status.ToLower().Contains(searchText))
                    || (p.Employees != null && p.Employees.FIO != null && p.Employees.FIO.ToLower().Contains(searchText))
                ).ToList();
            }

            // Фильтр по статусу
            if (StatusComboBox.SelectedIndex == 1)
                _current = _current.Where(p => p.Status == "Активен").ToList();
            else if (StatusComboBox.SelectedIndex == 2)
                _current = _current.Where(p => p.Status == "В архиве").ToList();

            // Фильтр по фонду
            if (FundComboBox.SelectedIndex > 0)
            {
                string fundName = (FundComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                if (!string.IsNullOrEmpty(fundName) && fundName != "Все фонды")
                {
                    _current = _current.Where(p => p.LibraryFund.FundName == fundName).ToList();
                }
            }

            ListViewLibrary.ItemsSource = _current;
            CountTextBlock.Text = $"Найдено записей: {_current.Count}";
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => LibraryUpdate();
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => LibraryUpdate();
        private void FundComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => LibraryUpdate();

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddLibraryItemWindow();
            if (addWindow.ShowDialog() == true)
            {
                LibraryUpdate();
                MessageBox.Show("Книга успешно добавлена!");
            }
        }

        // ... Delete_Click оставляем без изменений ...
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ListViewLibrary.SelectedItem is LibraryItem selectedItem)
            {
                if (selectedItem.Status == "В архиве")
                {
                    MessageBox.Show("Этот экземпляр уже находится в архиве.");
                    return;
                }

                var reasonWindow = new ArchiveReasonWindow();
                if (reasonWindow.ShowDialog() != true) return;

                string reason = reasonWindow.SelectedReason;

                try
                {
                    var context = LibraryBalahninEntities.GetContext();
                    var itemToArchive = context.LibraryItem.FirstOrDefault(i => i.ItemID == selectedItem.ItemID);

                    if (itemToArchive != null)
                    {
                        itemToArchive.Status = "В архиве";

                        var archiveItem = new ArchiveLibraryItem
                        {
                            ItemID = itemToArchive.ItemID,
                            FundID = itemToArchive.FundID,
                            LiteratureTypeID = itemToArchive.LiteratureTypeID,
                            ReplenishmentID = itemToArchive.ReplenishmentID,
                            Title = itemToArchive.Title,
                            Author = itemToArchive.Author,
                            InventoryNumber = itemToArchive.InventoryNumber,
                            AddedDate = itemToArchive.AddedDate,
                            ArchivedDate = DateTime.Now,
                            Reason = reason,
                            ArchivedBy = Manager.CurrentEmployee?.EmployeeID
                        };
                        context.ArchiveLibraryItem.Add(archiveItem);

                        var fund = context.LibraryFund.FirstOrDefault(f => f.FundID == itemToArchive.FundID);
                        if (fund != null)
                        {
                            switch (itemToArchive.LiteratureTypeID)
                            {
                                case 1: if (fund.BookCount > 0) fund.BookCount--; break;
                                case 2: if (fund.JournalCount > 0) fund.JournalCount--; break;
                                case 3: if (fund.NewspaperCount > 0) fund.NewspaperCount--; break;
                                case 4: if (fund.CollectionCount > 0) fund.CollectionCount--; break;
                                case 5: if (fund.DissertationCount > 0) fund.DissertationCount--; break;
                                case 6: if (fund.AbstractCount > 0) fund.AbstractCount--; break;
                            }
                        }

                        context.SaveChanges();
                        LibraryUpdate();
                        MessageBox.Show($"Экземпляр списан в архив.\nПричина: {reason}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при списании: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Сначала выберите книгу в списке, которую хотите списать.", "Внимание");
            }
        }
    }
}