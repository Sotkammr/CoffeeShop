using CoffeeShop.Classes;
using CoffeeShopSales.Classes;
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
    public partial class ProfilePage : Page
    {
        private User currentUser;

        public ProfilePage()
        {
            InitializeComponent();
            LoadUserData();
            LoadUserStatistics();
        }

        private void LoadUserData()
        {
            currentUser = DataManager.Instance.GetCurrentUser();

            if (currentUser != null)
            {
                txtLogin.Text = currentUser.Login;
                txtFullName.Text = currentUser.FullName;
                foreach (var item in cmbPosition.Items)
                {
                    if (item is ComboBoxItem comboItem &&
                        comboItem.Content.ToString() == currentUser.Position)
                    {
                        cmbPosition.SelectedItem = comboItem;
                        break;
                    }
                }

                txtPhone.Text = currentUser.Phone;
                txtEmail.Text = currentUser.Email;
            }
        }

        private void LoadUserStatistics()
        {
            var stats = DataManager.Instance.GetUserStatistics(currentUser?.Login);
            lblTotalSales.Text = stats["TotalSales"].ToString();
            lblTotalAmount.Text = $"{Convert.ToDecimal(stats["TotalAmount"]):C}";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtFullName.Text))
            {
                ShowMessage("Введите ФИО!", true);
                return;
            }

            string newPassword = txtNewPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword != confirmPassword)
                {
                    ShowMessage("Пароли не совпадают!", true);
                    return;
                }

                if (newPassword.Length < 4)
                {
                    ShowMessage("Пароль должен быть не менее 4 символов!", true);
                    return;
                }
            }

            string position = (cmbPosition.SelectedItem as ComboBoxItem)?.Content.ToString();

            bool success = DataManager.Instance.UpdateUser(
                currentUser.Login,
                txtFullName.Text,
                position,
                txtPhone.Text,
                txtEmail.Text,
                string.IsNullOrEmpty(newPassword) ? null : newPassword
            );

            if (success)
            {
                ShowMessage("Данные успешно сохранены!", false);
                LoadUserStatistics();
            }
            else
            {
                ShowMessage("Ошибка при сохранении данных!", true);
            }
        }

        private void ShowMessage(string message, bool isError)
        {
            lblStatus.Text = message;
            lblStatus.Foreground = isError ?
                System.Windows.Media.Brushes.Red :
                System.Windows.Media.Brushes.Green;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            LoadUserData();
            lblStatus.Text = "";
        }
    }
}
