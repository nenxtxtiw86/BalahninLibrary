using System;
using System.Linq;
using System.Windows;

namespace BalahninLibrary
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = TbEmail.Text.Trim();
            string password = TbPassword.Text;  // ← Text, не Password

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите email и пароль!", "Ошибка");
                return;
            }

            try
            {
                var context = LibraryBalahninEntities.GetContext();
                var employee = context.Employees
                    .FirstOrDefault(emp => emp.Email == email && emp.Password == password);

                if (employee == null)
                {
                    MessageBox.Show("Неверный email или пароль!", "Ошибка входа");
                    return;
                }

                Manager.CurrentEmployee = employee;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при входе: " + ex.Message);
            }
        }
    }
}