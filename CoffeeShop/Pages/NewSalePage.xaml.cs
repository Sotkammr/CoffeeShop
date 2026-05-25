using CoffeeShop.Classes;
using CoffeeShopSales.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CoffeeShopSales.Pages
{
    public partial class NewSalePage : Page
    {
        private List<Product> products;
        private List<CartItem> cartItems;

        public NewSalePage()
        {
            InitializeComponent();

            LoadProducts();
            cartItems = new List<CartItem>();

            lstProducts.ItemsSource = products;
            dgCart.ItemsSource = cartItems;

            UpdateCart();
        }

        private void LoadProducts()
        {
            products = DataManager.Instance.GetProducts();
        }

        private void UpdateCart()
        {
            dgCart.Items.Refresh();
            decimal total = cartItems.Sum(item => item.Total);
            lblTotal.Text = $"{total:C}";

            int totalItems = cartItems.Sum(item => item.Quantity);
            lblItemsCount.Text = $"{totalItems} товаров в чеке";

            if (cartItems.Count > 0)
            {
                txtSaleInfo.Text = $"В чеке {cartItems.Count} позиций на сумму {total:C}";
            }
            else
            {
                txtSaleInfo.Text = "Добавьте товары в чек";
            }
        }

        private void btnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (lstProducts.SelectedItem is Product selectedProduct)
            {
                var existingItem = cartItems.FirstOrDefault(item => item.ProductId == selectedProduct.Id);

                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    cartItems.Add(new CartItem
                    {
                        ProductId = selectedProduct.Id,
                        Name = selectedProduct.Name,
                        Price = selectedProduct.Price,
                        Quantity = 1
                    });
                }

                UpdateCart();
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is CartItem item)
            {
                item.Quantity++;
                UpdateCart();
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is CartItem item)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                    UpdateCart();
                }
                else
                {
                    cartItems.Remove(item);
                    UpdateCart();
                }
            }
        }

        private void QuantitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (slider != null && slider.Tag is CartItem item)
            {
                int newQuantity = (int)slider.Value;
                if (newQuantity != item.Quantity)
                {
                    item.Quantity = newQuantity;
                    UpdateCart();
                }
            }
        }

        private void btnRemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if (dgCart.SelectedItem is CartItem selectedItem)
            {
                cartItems.Remove(selectedItem);
                UpdateCart();
            }
        }

        private void btnClearCart_Click(object sender, RoutedEventArgs e)
        {
            cartItems.Clear();
            UpdateCart();
        }

        private void btnCompleteSale_Click(object sender, RoutedEventArgs e)
        {
            if (cartItems.Count == 0)
            {
                MessageBox.Show("Добавьте товары в чек!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string paymentType = (cmbPaymentType.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(paymentType))
            {
                MessageBox.Show("Выберите способ оплаты!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal total = cartItems.Sum(item => item.Total);
            var sale = DataManager.Instance.SaveSale(cartItems, paymentType);

            if (sale == null)
                return;

            string receiptPath = null;
            string receiptError = null;

            try
            {
                receiptPath = ReceiptDocumentService.CreateReceiptDocument(sale);
            }
            catch (System.Exception ex)
            {
                receiptError = ex.Message;
            }

            string message = $"Продажа завершена!\n\nСумма: {total:C}\nОплата: {paymentType}";
            if (!string.IsNullOrWhiteSpace(receiptPath))
            {
                message += $"\nWord-чек сохранен:\n{receiptPath}";
            }
            else if (!string.IsNullOrWhiteSpace(receiptError))
            {
                message += $"\n\nПродажа сохранена, но Word-чек создать не удалось:\n{receiptError}";
            }

            MessageBox.Show(message, "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

            cartItems.Clear();
            cmbPaymentType.SelectedIndex = 0;
            UpdateCart();
            LoadProducts();
        }

        private void lstProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnAddToCart.IsEnabled = lstProducts.SelectedItem != null;
        }

        private void dgCart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnRemoveFromCart.IsEnabled = dgCart.SelectedItem != null;
        }
    }
}
