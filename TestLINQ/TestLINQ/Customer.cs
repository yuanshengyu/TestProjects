using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLINQ
{
    public class Customer
    {
        public string ID { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public decimal Sales { get; set; }
        public override string ToString()
        {
            return string.Format("ID: {0} City: {1} Country: {2} Region: {3} Sales: {4}", ID, City, Country, Region, Sales);
        }
    }
    public class Order
    {
        public string ID { get; set; }
        public decimal Amount { get; set; }
    }
}
