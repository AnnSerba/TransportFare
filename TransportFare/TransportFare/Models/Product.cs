using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportFare.Models
{
    public class Product
    {
        public string Id { get; }
        public string Name { get; }
        public string FormattedPrice { get; }
        public double Price { get; }
        public string Currency { get; }
       public Product(string id,string name,double price,string currency)
        {
            Id = id;
            Name = name;
            Price = price;
            Currency = currency;
            FormattedPrice = price + " " + currency;
        }
    }
}
