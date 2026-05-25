using CoffeeShop.Classes;
using CoffeeShopSales.Classes;
using System;
using System.Collections.Generic;
using System.IO;
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
    public partial class SalesHistoryPage : Page
    {
        private List<Sale> sales = new List<Sale>();

        public SalesHistoryPage()
        {
            InitializeComponent();
            dpFrom.SelectedDate = DateTime.Today.AddDays(-30);
            dpTo.SelectedDate = DateTime.Today;
            LoadSales();
        }
        public class SaleDisplay
        {
            public string DateDisplay { get; set; }
            public string SaleNumber { get; set; }
            public string Cashier { get; set; }
            public string ItemsDisplay { get; set; }
            public string TotalDisplay { get; set; }
            public string PaymentType { get; set; }
        }

        private void LoadSales()
        {
            try
            {
                DateTime fromDate = dpFrom.SelectedDate ?? DateTime.Today.AddDays(-30);
                DateTime toDate = dpTo.SelectedDate ?? DateTime.Today;

                sales = DataManager.Instance.GetSales(fromDate, toDate.AddDays(1));

                if (sales == null)
                {
                    sales = new List<Sale>();
                }
                var displayList = new List<SaleDisplay>();

                foreach (var sale in sales)
                {
                    if (sale == null) continue;

                    string items = "Нет товаров";
                    if (sale.Items != null && sale.Items.Count > 0)
                    {
                        items = string.Join(", ", sale.Items.Select(i => $"{i.ProductName} x{i.Quantity}"));
                    }

                    displayList.Add(new SaleDisplay
                    {
                        DateDisplay = sale.SaleDate.ToString("dd.MM.yyyy HH:mm"),
                        SaleNumber = sale.SaleNumber ?? "-",
                        Cashier = sale.Cashier ?? "-",
                        ItemsDisplay = items,
                        TotalDisplay = sale.TotalAmount.ToString("C"),
                        PaymentType = sale.PaymentType ?? "-"
                    });
                }

                dgSales.ItemsSource = displayList;
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void UpdateStatistics()
        {
            try
            {
                if (sales == null || sales.Count == 0)
                {
                    lblTotalSales.Text = "0";
                    lblTotalAmount.Text = "0 руб";
                    lblAverageSale.Text = "0 руб";
                    lblPopularPayment.Text = "-";
                    return;
                }

                lblTotalSales.Text = sales.Count.ToString();
                decimal total = sales.Sum(s => s.TotalAmount);
                lblTotalAmount.Text = $"{total:C}";
                if (sales.Count > 0)
                {
                    decimal avg = total / sales.Count;
                    lblAverageSale.Text = $"{avg:C}";
                }
                var popular = sales
                    .Where(s => !string.IsNullOrEmpty(s.PaymentType))
                    .GroupBy(s => s.PaymentType)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                lblPopularPayment.Text = popular?.Key ?? "-";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка статистики: {ex.Message}");
            }
        }

        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dpFrom.SelectedDate = DateTime.Today;
            dpTo.SelectedDate = DateTime.Today;
            LoadSales();
        }
        private void btnWeek_Click(object sender, RoutedEventArgs e)
        {
            dpFrom.SelectedDate = DateTime.Today.AddDays(-7);
            dpTo.SelectedDate = DateTime.Today;
            LoadSales();
        }
        private void btnMonth_Click(object sender, RoutedEventArgs e)
        {
            dpFrom.SelectedDate = DateTime.Today.AddMonths(-1);
            dpTo.SelectedDate = DateTime.Today;
            LoadSales();
        }
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadSales();
        }
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sales == null || sales.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта");
                    return;
                }

                string fileName = $"Sales_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = System.IO.Path.Combine(desktopPath, fileName);

                var lines = new List<string> { "Дата;Номер;Кассир;Товары;Сумма;Тип оплаты" };

                foreach (var sale in sales)
                {
                    string items = sale.Items != null && sale.Items.Count > 0
                        ? string.Join("; ", sale.Items.Select(i => $"{i.ProductName} x{i.Quantity}"))
                        : "Нет товаров";

                    lines.Add($"{sale.SaleDate:dd.MM.yyyy HH:mm};{sale.SaleNumber};{sale.Cashier};{items};{sale.TotalAmount};{sale.PaymentType}");
                }

                lines.Add($"\nВсего продаж;{sales.Count};;;{sales.Sum(s => s.TotalAmount)};");

                File.WriteAllLines(filePath, lines);
                MessageBox.Show($"Файл сохранен:\n{filePath}", "Успешно");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}");
            }
        }
    }
}