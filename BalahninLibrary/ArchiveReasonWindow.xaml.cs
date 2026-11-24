using System.Windows;
using System.Windows.Controls;

namespace BalahninLibrary
{
    public partial class ArchiveReasonWindow : Window
    {
        public string SelectedReason { get; private set; }

        public ArchiveReasonWindow()
        {
            InitializeComponent();
            ReasonComboBox.SelectedIndex = 0;
        }

        // Показ/скрытие поля «Своя причина»
        private void ReasonComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReasonComboBox.SelectedItem is ComboBoxItem item)
            {
                string content = item.Content?.ToString() ?? "";
                CustomReasonPanel.Visibility = content.StartsWith("Другое")
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            var item = ReasonComboBox.SelectedItem as ComboBoxItem;
            string reason = item?.Content?.ToString() ?? "";

            // Если выбрано «Другое» — берём из TextBox
            if (reason.StartsWith("Другое"))
            {
                if (string.IsNullOrWhiteSpace(TbCustomReason.Text))
                {
                    MessageBox.Show("Введите свою причину списания!", "Ошибка");
                    return;
                }
                reason = TbCustomReason.Text.Trim();
            }

            SelectedReason = reason;
            DialogResult = true;
            Close();
        }
    }
}