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
    public partial class ProductsPage : Page
    {
        private List<Product> products;

        public ProductsPage()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts()
        {
            products = DataManager.Instance.GetProducts();
            dgProducts.ItemsSource = products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Category,
                PriceDisplay = p.PriceDisplay,
                p.QuantityInStock
            }).ToList();
            UpdateButtonsState();
        }

        private void UpdateButtonsState()
        {
            bool hasSelection = dgProducts.SelectedItem != null;
            btnEditProduct.IsEnabled = hasSelection;
            btnDeleteProduct.IsEnabled = hasSelection;
        }
        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            ProductWindow productWindow = new ProductWindow();
            productWindow.Owner = Window.GetWindow(this);

            if (productWindow.ShowDialog() == true)
            {
                bool success = DataManager.Instance.AddProduct(
                    productWindow.ProductName,
                    productWindow.ProductPrice,
                    productWindow.ProductCategory
                );

                if (success)
                {
                    LoadProducts();
                    MessageBox.Show("Товар успешно добавлен!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void btnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem != null)
            {
                dynamic selected = dgProducts.SelectedItem;
                int id = selected.Id;
                string name = selected.Name;
                decimal price = decimal.Parse(selected.PriceDisplay, System.Globalization.NumberStyles.Currency);
                string category = selected.Category;

                ProductWindow productWindow = new ProductWindow(name, price, category);
                productWindow.Owner = Window.GetWindow(this);

                if (productWindow.ShowDialog() == true)
                {
                    bool success = DataManager.Instance.UpdateProduct(
                        id,
                        productWindow.ProductName,
                        productWindow.ProductPrice,
                        productWindow.ProductCategory
                    );
                    if (success)
                    {
                        LoadProducts();
                        MessageBox.Show("Товар успешно обновлен!", "Успех",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
        private void btnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem != null)
            {
                dynamic selected = dgProducts.SelectedItem;

                MessageBoxResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить товар '{selected.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    bool success = DataManager.Instance.DeleteProduct(selected.Id);

                    if (success)
                    {
                        LoadProducts();
                        MessageBox.Show("Товар удален!", "Успех",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }
        private void dgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonsState();
        }
    }
}
