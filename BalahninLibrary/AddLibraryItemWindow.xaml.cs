using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BalahninLibrary
{
    public partial class AddLibraryItemWindow : Window
    {
        public AddLibraryItemWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            var context = LibraryBalahninEntities.GetContext();

            // 1. Типы литературы
            TypeComboBox.ItemsSource = context.LiteratureType.ToList();
            TypeComboBox.SelectedIndex = 0;

            // Проверяем роль
            bool isDirector = Manager.CurrentEmployee != null &&
                              (Manager.CurrentEmployee.Position == "Директор" ||
                               Manager.CurrentEmployee.Position == "Администратор");

            if (isDirector)
            {
                // Если директор — показываем ComboBox с библиотеками
                LibraryPanel.Visibility = Visibility.Visible;
                LibraryComboBox.ItemsSource = context.Library.OrderBy(l => l.Name).ToList();
                LibraryComboBox.SelectedIndex = 0;

                // Загружаем фонды для первой библиотеки
                LoadFundsByLibrary();
            }
            else
            {
                // Если обычный сотрудник — прячем выбор библиотеки и показываем только его фонды
                LibraryPanel.Visibility = Visibility.Collapsed;

                int currentLibraryId = Manager.CurrentEmployee.LibraryID;
                var funds = context.LibraryFund.Where(f => f.LibraryID == currentLibraryId).ToList();
                FundComboBox.ItemsSource = funds;
                FundComboBox.SelectedIndex = 0;
            }
        }

        // Обработчик смены библиотеки у директора
        private void LibraryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadFundsByLibrary();
        }

        // Метод для загрузки фондов по выбранной библиотеке
        private void LoadFundsByLibrary()
        {
            if (LibraryComboBox.SelectedItem is Library selectedLibrary)
            {
                var context = LibraryBalahninEntities.GetContext();
                var funds = context.LibraryFund.Where(f => f.LibraryID == selectedLibrary.LibraryID).ToList();
                FundComboBox.ItemsSource = funds;
                FundComboBox.SelectedIndex = 0;
            }
        }

        // Показ/скрытие полей в зависимости от типа литературы
        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TypeComboBox.SelectedItem is LiteratureType selectedType)
            {
                string typeName = selectedType.TypeName;

                // Периодика (журнал, газета) — автор обычно не указывается
                bool isPeriodical = (typeName == "Журнал" || typeName == "Газета");
                AuthorPanel.Visibility = isPeriodical ? Visibility.Collapsed : Visibility.Visible;

                // Газета, диссертация, реферат — издательство не указывается
                bool hidePublisher = (typeName == "Газета"
                                   || typeName == "Диссертация"
                                   || typeName == "Реферат");
                PublisherPanel.Visibility = hidePublisher ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbTitle.Text))
            {
                MessageBox.Show("Заполните название!", "Ошибка");
                return;
            }

            if (TypeComboBox.SelectedItem == null || FundComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип литературы и фонд!", "Ошибка");
                return;
            }

            if (!int.TryParse(TbCopiesCount.Text, out int copiesCount) || copiesCount < 1)
            {
                MessageBox.Show("Введите корректное количество копий (1 или больше)!", "Ошибка");
                return;
            }

            if (!int.TryParse(TbPublicationYear.Text, out int pubYear) || pubYear < 1500 || pubYear > DateTime.Now.Year)
            {
                MessageBox.Show("Введите корректный год публикации!", "Ошибка");
                return;
            }

            var selectedType = (LiteratureType)TypeComboBox.SelectedItem;

            // Если поле «Автор» скрыто — не требуем его заполнения
            bool authorVisible = AuthorPanel.Visibility == Visibility.Visible;
            if (authorVisible && string.IsNullOrWhiteSpace(TbAuthor.Text))
            {
                MessageBox.Show("Заполните автора!", "Ошибка");
                return;
            }

            try
            {
                var context = LibraryBalahninEntities.GetContext();
                var selectedFund = (LibraryFund)FundComboBox.SelectedItem;

                string publisherValue = (PublisherPanel.Visibility == Visibility.Visible
                                         && !string.IsNullOrWhiteSpace(TbPublisher.Text))
                    ? TbPublisher.Text.Trim()
                    : "Не указано";

                // Берем ID текущего пользователя из менеджера
                int currentEmployeeId = Manager.CurrentEmployee.EmployeeID;

                // 1. Партия
                var newReplenishment = new Replenishment
                {
                    FundID = selectedFund.FundID,
                    EmployeeID = currentEmployeeId, // <--- ИСПРАВЛЕНО ТУТ
                    LiteratureTypeID = selectedType.LiteratureTypeID,
                    ReplenishmentDate = DateTime.Now,
                    SourceName = TbTitle.Text,
                    Publisher = publisherValue,
                    PublicationDate = new DateTime(pubYear, 1, 1),
                    CopiesCount = copiesCount
                };
                context.Replenishment.Add(newReplenishment);
                context.SaveChanges();

                // 2. Экземпляры — БЕЗ InventoryNumber (вычисляемый столбец)
                for (int i = 0; i < copiesCount; i++)
                {
                    var newItem = new LibraryItem
                    {
                        FundID = selectedFund.FundID,
                        LiteratureTypeID = selectedType.LiteratureTypeID,
                        ReplenishmentID = newReplenishment.ReplenishmentID,
                        Title = TbTitle.Text,
                        Author = (AuthorPanel.Visibility == Visibility.Visible
                                  && !string.IsNullOrWhiteSpace(TbAuthor.Text))
                            ? TbAuthor.Text.Trim()
                            : null,
                        AddedDate = DateTime.Now,
                        Status = "Активен",
                        AddedBy = currentEmployeeId // <--- ИСПРАВЛЕНО ТУТ
                    };
                    context.LibraryItem.Add(newItem);
                }

                // 3. Счётчики
                switch (selectedType.LiteratureTypeID)
                {
                    case 1: selectedFund.BookCount += copiesCount; break;
                    case 2: selectedFund.JournalCount += copiesCount; break;
                    case 3: selectedFund.NewspaperCount += copiesCount; break;
                    case 4: selectedFund.CollectionCount += copiesCount; break;
                    case 5: selectedFund.DissertationCount += copiesCount; break;
                    case 6: selectedFund.AbstractCount += copiesCount; break;
                }

                context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении: " + ex.Message);
            }
        }
    }
}