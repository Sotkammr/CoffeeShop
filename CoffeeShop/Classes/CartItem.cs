using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Classes
{
    public class CartItem : INotifyPropertyChanged
    {
        private int _quantity;

        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(TotalDisplay));
            }
        }
        public decimal Total => Price * Quantity;
        public string TotalDisplay => $"{Total:C}";
        public string PriceDisplay => $"{Price:C}";

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}