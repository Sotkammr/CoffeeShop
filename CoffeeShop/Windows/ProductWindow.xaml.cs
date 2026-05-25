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
using System.Xml.Linq;

namespace CoffeeShop.Windows
{
    public partial class ProductWindow : Window
    {
        public string ProductName { get; private set; }
        public decimal ProductPrice { get; private set; }
        public string ProductCategory { get; private set; }

        public ProductWindow()
        {
            InitializeComponent();
        }

        public ProductWindow(string name, decimal price, string category) : this()
        {
            txtName.Text = name;
            txtPrice.Text = price.ToString();

            foreach (var item in cmbCategory.Items)
            {
                if (item is System.Windows.Controls.ComboBoxItem comboItem &&
                    comboItem.Content.ToString() == category)
                {
                    cmbCategory.SelectedItem = comboItem;
                    break;
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            string priceText = txtPrice.Text.Trim();
            string category = (cmbCategory.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(name))
            {
                ShowError("Введите название товара!");
                return;
            }

            if (!decimal.TryParse(priceText, out decimal price) || price <= 0)
            {
                ShowError("Введите корректную цену!");
                return;
            }

            if (string.IsNullOrEmpty(category))
            {
                ShowError("Выберите категорию!");
                return;
            }

            ProductName = name;
            ProductPrice = price;
            ProductCategory = category;

            this.DialogResult = true;
            this.Close();
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
