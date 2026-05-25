using CoffeeShop.Classes;
using CoffeeShopSales.Pages;
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
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            LoadUserInfo();
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new NewSalePage());
        }

        private void LoadUserInfo()
        {
            var currentUser = DataManager.Instance.GetCurrentUser();
            if (currentUser != null)
            {
                lblUserInfo.Text = $"{currentUser.FullName} ({currentUser.Position})";
                if (currentUser.Position == "Администратор")
                {
                    AddAdminButton();
                }
            }
        }

        private void AddAdminButton()
        {
            Button btnAdmin = new Button
            {
                Content = "⚙️ Администрирование",
                Background = System.Windows.Media.Brushes.White,
                Foreground = System.Windows.Media.Brushes.Brown,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(20, 10, 20, 10),
                Margin = new Thickness(0, 0, 10, 0),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            btnAdmin.Click += (s, args) => MainFrame.Navigate(new AdminPage());

            NavigationPanel.Children.Insert(5, btnAdmin);
        }

        private void btnNewSale_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new NewSalePage());
        }
        private void btnSalesHistory_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SalesHistoryPage());
        }
        private void btnProducts_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProductsPage());
        }
        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfilePage());
        }
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Вы уверены, что хотите выйти из системы?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                DataManager.Instance.CurrentUser = null;
                DataManager.Instance.CurrentUserInfo = null;
                if (NavigationService != null)
                {
                    NavigationService.Navigate(new LoginPage());
                }
            }
        }
    }
}