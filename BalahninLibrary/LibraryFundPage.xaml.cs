using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BalahninLibrary
{
    public partial class LibraryFundPage : Page
    {
        private List<LibraryFund> _allFunds;
        private ICollectionView _view;

        public LibraryFundPage()
        {
            InitializeComponent();

            LoadData();
            FillLibraryComboBox();
            ApplyFilter();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Пусто, так как всё загружено в конструкторе
        }

        private void LoadData()
        {
            _allFunds = LibraryBalahninEntities.GetContext()
                                               .LibraryFund
                                               .Include("Library")
                                               .ToList();
        }

        private void FillLibraryComboBox()
        {
            LibraryComboBox.SelectionChanged -= LibraryComboBox_SelectionChanged;

            LibraryComboBox.Items.Clear();
            LibraryComboBox.Items.Add(new ComboBoxItem { Content = "Все библиотеки" });

            var libraries = LibraryBalahninEntities.GetContext()
                                                   .Library
                                                   .OrderBy(l => l.Name)
                                                   .ToList();

            foreach (var lib in libraries)
            {
                LibraryComboBox.Items.Add(new ComboBoxItem
                {
                    Content = lib.Name,
                    Tag = lib.LibraryID
                });
            }

            LibraryComboBox.SelectedIndex = 0;

            LibraryComboBox.SelectionChanged += LibraryComboBox_SelectionChanged;
        }

        private void ApplyFilter()
        {
            if (_allFunds == null) return;

            IEnumerable<LibraryFund> query = _allFunds;

            string search = SearchTextBox.Text != null ? SearchTextBox.Text.Trim() : "";

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(f =>
                    ContainsIgnoreCase(f.FundName, search)
                    || ContainsIgnoreCase(f.Library != null ? f.Library.Name : null, search));
            }

            if (LibraryComboBox.SelectedIndex > 0
                && LibraryComboBox.SelectedItem is ComboBoxItem item
                && item.Tag is int libId)
            {
                query = query.Where(f => f.LibraryID == libId);
            }

            var result = query.OrderBy(f => f.FundName).ToList();

            _view = CollectionViewSource.GetDefaultView(result);
            _view.SortDescriptions.Clear();
            _view.SortDescriptions.Add(new SortDescription(nameof(LibraryFund.FundName),
                                                           ListSortDirection.Ascending));

            ListViewFunds.ItemsSource = _view;

            UpdateSummary(result);
        }

        private void UpdateSummary(List<LibraryFund> funds)
        {
            if (funds == null || funds.Count == 0)
            {
                SummaryTextBlock.Text = "Фондов: 0";
                return;
            }

            int books = funds.Sum(f => f.BookCount);
            int journals = funds.Sum(f => f.JournalCount);
            int newspapers = funds.Sum(f => f.NewspaperCount);
            int collections = funds.Sum(f => f.CollectionCount);
            int dissertations = funds.Sum(f => f.DissertationCount);
            int abstracts = funds.Sum(f => f.AbstractCount);
            int total = books + journals + newspapers + collections + dissertations + abstracts;

            SummaryTextBlock.Text =
                $"Фондов: {funds.Count}   |   " +
                $"Книги: {books}   " +
                $"Журналы: {journals}   " +
                $"Газеты: {newspapers}   " +
                $"Сборники: {collections}   " +
                $"Диссертации: {dissertations}   " +
                $"Рефераты: {abstracts}   |   " +
                $"ВСЕГО: {total}";
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is GridViewColumnHeader header)
                || header.Role != GridViewColumnHeaderRole.Padding)
                return;

            string sortBy = header.Content?.ToString();
            if (string.IsNullOrEmpty(sortBy)) return;

            string propertyName = null;

            // Переписали рекурсивный шаблон на классический switch
            switch (sortBy)
            {
                case "ID": propertyName = nameof(LibraryFund.FundID); break;
                case "Фонд": propertyName = nameof(LibraryFund.FundName); break;
                case "Библиотека": propertyName = "Library.Name"; break;
                case "Книги": propertyName = nameof(LibraryFund.BookCount); break;
                case "Журналы": propertyName = nameof(LibraryFund.JournalCount); break;
                case "Газеты": propertyName = nameof(LibraryFund.NewspaperCount); break;
                case "Сборники": propertyName = nameof(LibraryFund.CollectionCount); break;
                case "Диссертации": propertyName = nameof(LibraryFund.DissertationCount); break;
                case "Рефераты": propertyName = nameof(LibraryFund.AbstractCount); break;
                case "Всего": propertyName = nameof(LibraryFund.TotalCount); break;
                default: return;
            }

            if (_view == null) return;

            var current = _view.SortDescriptions.FirstOrDefault();
            ListSortDirection direction = ListSortDirection.Ascending;

            if (current.PropertyName == propertyName)
            {
                direction = current.Direction == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }

            _view.SortDescriptions.Clear();
            _view.SortDescriptions.Add(new SortDescription(propertyName, direction));
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void LibraryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
            FillLibraryComboBox();
            ApplyFilter();
        }

        // Заменили Contains на IndexOf для совместимости с C# 7.3
        private static bool ContainsIgnoreCase(string source, string search)
        {
            return !string.IsNullOrEmpty(source)
                && source.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}