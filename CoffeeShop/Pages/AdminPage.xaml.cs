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
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                var users = DataManager.Instance.GetAllUsers();
                dgUsers.ItemsSource = users;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Owner = Window.GetWindow(this);
            registerWindow.ShowDialog();
            LoadUsers();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }
    }
}