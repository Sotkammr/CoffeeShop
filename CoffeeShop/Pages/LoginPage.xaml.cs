using CoffeeShop.Classes;
using CoffeeShop.Windows;
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

namespace CoffeeShop.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            bool connected = DatabaseHelper.Instance.TestConnection();
            if (!connected)
            {
                MessageBox.Show("Нет подключения к базе данных!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Подключение к БД успешно!", "Отладка",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                lblStatus.Text = "Заполните все поля!";
                return;
            }
            bool isAuthenticated = DataManager.Instance.AuthenticateUser(login, password);
            if (isAuthenticated)
            {
                NavigationService?.Navigate(new MainPage());
            }
            else
            {
                lblStatus.Text = "Неверный логин или пароль!";
            }
        }
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Owner = Window.GetWindow(this);
            registerWindow.ShowDialog();
        }
    }
}
