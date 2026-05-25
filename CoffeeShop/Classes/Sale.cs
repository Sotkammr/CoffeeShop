using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShopSales.Classes
{
    public class Sale
    {
        public int Id { get; set; }
        public string SaleNumber { get; set; }
        public DateTime SaleDate { get; set; }
        public string Cashier { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentType { get; set; }
        public List<SaleItem> Items { get; set; } = new List<SaleItem>();

        public string TotalDisplay => $"{TotalAmount:C}";
        public string DateDisplay => SaleDate.ToString("dd.MM.yyyy HH:mm");

        public string ItemsDisplay
        {
            get
            {
                if (Items == null || Items.Count == 0)
                    return "Нет товаров";
                return string.Join(", ", Items);
            }
        }
    }
    public class SaleItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Price * Quantity;

        public override string ToString()
        {
            return $"{ProductName} x{Quantity}";
        }
    }
}