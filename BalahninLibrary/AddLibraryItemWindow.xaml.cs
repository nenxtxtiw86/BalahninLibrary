using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

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

            // Загружаем типы литературы
            TypeComboBox.ItemsSource = context.LiteratureType.ToList();
            TypeComboBox.SelectedIndex = 0;

            // Загружаем фонды
            FundComboBox.ItemsSource = context.LibraryFund.ToList();
            FundComboBox.SelectedIndex = 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            // 1. Проверка на пустоту
            if (string.IsNullOrWhiteSpace(TbTitle.Text) || string.IsNullOrWhiteSpace(TbInventory.Text))
            {
                MessageBox.Show("Заполните название и базовый инвентарный номер!", "Ошибка");
                return;
            }

            // 2. Проверка формата инвентарного номера (должен начинаться с INV- и содержать цифры)
            string invPattern = @"^INV-\d+$";
            if (!Regex.IsMatch(TbInventory.Text.Trim(), invPattern, RegexOptions.IgnoreCase))
            {
                MessageBox.Show("Инвентарный номер должен быть в формате: INV-000 (латинские буквы INV, дефис и цифры).", "Ошибка ввода");
                return;
            }

            if (TypeComboBox.SelectedItem == null || FundComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип литературы и фонд!", "Ошибка");
                return;
            }

            // Парсим количество копий
            if (!int.TryParse(TbCopiesCount.Text, out int copiesCount) || copiesCount < 1)
            {
                MessageBox.Show("Введите корректное количество копий (1 или больше)!", "Ошибка");
                return;
            }

            // Парсим год публикации
            if (!int.TryParse(TbPublicationYear.Text, out int pubYear) || pubYear < 1500 || pubYear > DateTime.Now.Year)
            {
                MessageBox.Show("Введите корректный год публикации!", "Ошибка");
                return;
            }

            try
            {
                var context = LibraryBalahninEntities.GetContext();
                var selectedFund = (LibraryFund)FundComboBox.SelectedItem;
                var selectedType = (LiteratureType)TypeComboBox.SelectedItem;
                string baseInventory = TbInventory.Text.Trim();

                // 1. СОЗДАЕМ ПАРТИЮ (Replenishment)
                var newReplenishment = new Replenishment
                {
                    FundID = selectedFund.FundID,
                    EmployeeID = 1, // TODO: Заменить на ID текущего сотрудника
                    LiteratureTypeID = selectedType.LiteratureTypeID,
                    ReplenishmentDate = DateTime.Now,
                    SourceName = TbTitle.Text,
                    Publisher = TbPublisher.Text,
                    PublicationDate = new DateTime(pubYear, 1, 1), // 1 января указанного года
                    CopiesCount = copiesCount // Указываем реальное количество!
                };
                context.Replenishment.Add(newReplenishment);
                context.SaveChanges(); // Сохраняем, чтобы получить ReplenishmentID

                // 2. СОЗДАЕМ N ЭКЗЕМПЛЯРОВ (LibraryItem)
                for (int i = 0; i < copiesCount; i++)
                {
                    // Генерируем инвентарный номер (если базовый INV-100, то будут INV-100, INV-101...)
                    // Если в конце есть цифры, увеличиваем их. Если нет — добавляем.
                    string currentInventoryNumber = GenerateInventoryNumber(baseInventory, i);

                    // Проверка на дубликат
                    if (context.LibraryItem.Any(item => item.InventoryNumber == currentInventoryNumber))
                    {
                        MessageBox.Show($"Инвентарный номер {currentInventoryNumber} уже существует в базе! Измените базовый номер.", "Ошибка");
                        return; // Прерываем сохранение
                    }

                    var newItem = new LibraryItem
                    {
                        FundID = selectedFund.FundID,
                        LiteratureTypeID = selectedType.LiteratureTypeID,
                        ReplenishmentID = newReplenishment.ReplenishmentID,
                        Title = TbTitle.Text,
                        Author = TbAuthor.Text,
                        InventoryNumber = currentInventoryNumber,
                        AddedDate = DateTime.Now,
                        Status = "Активен",
                        AddedBy = 1
                    };
                    context.LibraryItem.Add(newItem);
                }

                // 3. УВЕЛИЧИВАЕМ СЧЕТЧИК В ФОНДЕ НА КОЛИЧЕСТВО КОПИЙ
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

        // Вспомогательный метод для генерации номеров
        private string GenerateInventoryNumber(string baseNumber, int offset)
        {
            if (offset == 0) return baseNumber; // Первый экземпляр получает ровно то, что ввели

            // Ищем цифры в конце строки (например, INV-100 -> 100)
            int digitIndex = baseNumber.Length - 1;
            while (digitIndex >= 0 && char.IsDigit(baseNumber[digitIndex]))
            {
                digitIndex--;
            }

            if (digitIndex < baseNumber.Length - 1)
            {
                // Если цифры есть, отделяем их
                string prefix = baseNumber.Substring(0, digitIndex + 1);
                string numberPart = baseNumber.Substring(digitIndex + 1);
                int number = int.Parse(numberPart) + offset;

                // Сохраняем ведущие нули (если было 005, станет 006)
                return prefix + number.ToString(new string('0', numberPart.Length));
            }
            else
            {
                // Если цифр нет, просто добавляем дефис и индекс
                return $"{baseNumber}-{offset + 1}";
            }
        }
    }
}