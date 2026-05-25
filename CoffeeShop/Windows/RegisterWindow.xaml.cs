using CoffeeShop.Classes;
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
using System.Windows.Shapes;

namespace CoffeeShop.Windows
{
    public partial class RegisterWindow : Window
    {
        public bool IsRegistered { get; private set; }

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;
            string fullName = txtFullName.Text.Trim();
            string position = (cmbPosition.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();
            string phone = txtPhone.Text.Trim();
            string email = txtEmail.Text.Trim();
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(position))
            {
                ShowError("Заполните обязательные поля!");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Пароли не совпадают!");
                return;
            }

            if (password.Length < 4)
            {
                ShowError("Пароль должен быть не менее 4 символов!");
                return;
            }
            bool success = DataManager.Instance.RegisterUser(login, password, fullName, position, phone, email);

            if (success)
            {
                MessageBox.Show("Сотрудник успешно зарегистрирован!\nТеперь вы можете войти в систему.",
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                IsRegistered = true;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                ShowError("Ошибка при регистрации! Возможно, логин уже занят.");
            }
        }
        private void ShowError(string message)
        {
            lblStatus.Text = message;
            lblStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}