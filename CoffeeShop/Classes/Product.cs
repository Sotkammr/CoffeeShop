using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Classes
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public int QuantityInStock { get; set; }
        public bool IsActive { get; set; } = true;

        public string PriceDisplay => $"{Price:C}";
    }
}