using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BalahninLibrary
{
    /// <summary>
    /// Логика взаимодействия для LibraryItemPage.xaml
    /// </summary>
    public partial class LibraryItemPage : Page
    {
        public LibraryItemPage()
        {
            InitializeComponent();
            var _current = LibraryBalahninEntities.GetContext().LibraryItem.ToList();
            ListViewLibrary.ItemsSource = _current;
            StatusComboBox.SelectedIndex = 0;
            FundComboBox.SelectedIndex = 0;
            LibraryUpdate();
        }

        private void LibraryUpdate()
        {
            var _current = LibraryBalahninEntities.GetContext().LibraryItem.ToList();

            string searchText = SearchTextBox.Text.Trim().ToLower();

            if (!string.IsNullOrEmpty(searchText))
            {
                _current = _current.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(searchText))
                    || (p.Author != null && p.Author.ToLower().Contains(searchText))
                    || (p.InventoryNumber != null && p.InventoryNumber.ToLower().Contains(searchText))
                    || (p.Status != null && p.Status.ToLower().Contains(searchText))
                    || (p.Employees != null && p.Employees.FIO != null && p.Employees.FIO.ToLower().Contains(searchText))
                    || (p.LibraryFund != null && p.LibraryFund.FundName != null && p.LibraryFund.FundName.ToLower().Contains(searchText))
                ).ToList();
            }


            if (StatusComboBox.SelectedIndex == 1)
            {
                _current = _current.Where(p => p.Status == "Активен").ToList();
            }
            else if(StatusComboBox.SelectedIndex == 2)
            {
                _current = _current.Where(p => p.Status == "В архиве").ToList();
            }
            if (FundComboBox.SelectedIndex == 1)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Основной фонд").ToList();
            }
            else if (FundComboBox.SelectedIndex == 2)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Редкий фонд").ToList();
            }
            else if (FundComboBox.SelectedIndex == 3)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Учебный фонд").ToList();
            }
            else if (FundComboBox.SelectedIndex == 4)
            {
                _current = _current.Where(p =>p.LibraryFund.FundName == "Периодика").ToList();
            }
            else if (FundComboBox.SelectedIndex == 5)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Детский фонд").ToList();
            }
            else if (FundComboBox.SelectedIndex == 6)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Научный фонд").ToList();
            }
            else if (FundComboBox.SelectedIndex == 7)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Краеведческий фонд").ToList();
            }
            else if (FundComboBox.SelectedIndex == 8)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Фонд периодических изданий").ToList();
            }
            else if (FundComboBox.SelectedIndex == 9)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Художественный фонд").ToList();
            }
            else if (FundComboBox.SelectedIndex == 10)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Фонд редких книг").ToList();
            }
            else if (FundComboBox.SelectedIndex == 11)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Справочный фонд").ToList();
            }
            else if (FundComboBox.SelectedIndex == 12)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Фонд диссертаций").ToList();
            }
            else if (FundComboBox.SelectedIndex == 13)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Фонд рефератов").ToList();
            }
            else if (FundComboBox.SelectedIndex == 14)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Учебно-методический фонд").ToList();
            }
            else if (FundComboBox.SelectedIndex == 15)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Фонд аудиокниг").ToList();
            }
            else if (FundComboBox.SelectedIndex == 16)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Медиатека").ToList();
            }
            else if (FundComboBox.SelectedIndex == 17)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Фонд периодики").ToList();
            }
            else if (FundComboBox.SelectedIndex == 18)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Молодёжный фонд").ToList();
            }
            else if (FundComboBox.SelectedIndex == 19)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Досуговый фонд").ToList();
            }
            else if (FundComboBox.SelectedIndex == 20)
            {
                _current = _current.Where(p => p.LibraryFund.FundName == "Фонд нотных изданий").ToList();
            }
            ListViewLibrary.ItemsSource = _current;
        }
        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LibraryUpdate();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LibraryUpdate();
        }

        private void FundComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LibraryUpdate();
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddLibraryItemWindow();
            if (addWindow.ShowDialog() == true)
            {
                // После добавления обновляем список
                LibraryUpdate();
                MessageBox.Show("Книга успешно добавлена!");
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, выбрана ли какая-то книга в списке
            if (ListViewLibrary.SelectedItem is LibraryItem selectedItem)
            {
                // Проверяем, не в архиве ли она уже
                if (selectedItem.Status == "В архиве")
                {
                    MessageBox.Show("Этот экземпляр уже находится в архиве.");
                    return;
                }

                // Сначала — окно выбора причины
                var reasonWindow = new ArchiveReasonWindow();
                if (reasonWindow.ShowDialog() != true)
                    return;  // пользователь отменил

                string reason = reasonWindow.SelectedReason;

                try
                {
                    var context = LibraryBalahninEntities.GetContext();
                    var itemToArchive = context.LibraryItem.FirstOrDefault(i => i.ItemID == selectedItem.ItemID);

                    if (itemToArchive != null)
                    {
                        // 2. Статус
                        itemToArchive.Status = "В архиве";

                        // 3. Копия в архив
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
                            Reason = reason,  // ← из окна
                            ArchivedBy = Manager.CurrentEmployee?.EmployeeID
                        };
                        context.ArchiveLibraryItem.Add(archiveItem);

                        // 4. Счётчик по типу литературы
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
