using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLINQ
{
    class Program
    {
        static List<Customer> customers = null;
        static List<Order> orders = null;
        static void Main(string[] args)
        {
            Init();
            Console.ReadKey();
        }
        static void Other()
        {
            var customerIDs =
                from c in customers
                select c.ID;
            var orderIDs =
                from o in orders
                select o.ID;
            Console.WriteLine("Customer IDs:");
            foreach (var item in customerIDs)
            {
                Console.Write("{0} ", item);
            }
            Console.WriteLine();
            Console.WriteLine("Order IDs:");
            foreach (var item in orderIDs)
            {
                Console.Write("{0} ", item);
            }
            Console.WriteLine();
            var customersWithOrders = customerIDs.Intersect(orderIDs);
            Console.WriteLine("Customer IDs with Orders:");
            foreach (var item in customersWithOrders)
            {
                Console.Write("{0} ", item);
            }
            Console.WriteLine();
            Console.WriteLine("Order IDs with no customers:");
            var ordersNoCustomers = orderIDs.Except(customerIDs);
            foreach(var item in ordersNoCustomers)
            {
                Console.Write("{0} ", item);
            }
            Console.WriteLine();
            Console.WriteLine("All Customer and Order IDs:");
            var all = orderIDs.Union(customerIDs);
            foreach (var item in all)
            {
                Console.Write("{0} ", item);
            }
            Console.WriteLine();
            var queryResults =
                from c in customers
                join o in orders on c.ID equals o.ID
                select new { c.ID, c.City, SalesBefore = c.Sales, NewOrder = o.Amount, SalesAfter = c.Sales + o.Amount };
            foreach (var item in queryResults)
            {
                Console.Write("{0} ", item);
            }
        }
        static void TakeSkip()
        {
            var query =
                from c in customers
                orderby c.Sales
                select c;
            foreach (var c in query.Skip(5))//Take: 取前五个， Skip: 跳过前五个
            {
                Console.WriteLine(c);
            }
        }
        static void Group()
        {
            var queryResults =
                from c in customers
                group c by c.Region into cg
                select new { TotalSales = cg.Sum(c => c.Sales), Region = cg.Key }
                ;
            var result2 =
                from c in queryResults
                orderby c.TotalSales descending
                select c;
            foreach (var c in result2)
            {
                Console.WriteLine(c);
            }
        }
        static void AnyAll()
        {
            string[] strs = new string[] { "a", "b", "c", "a" };
            bool result = strs.Any(a =>
            {
                int ll = a.CompareTo("b");
                if (ll > 0) return true;
                return false;
            });
            Console.WriteLine(result);
        }
        static void Init()
        {
            customers = new List<Customer>();
            customers.Add(new Customer { ID = "A", City = "New York", Country = "USA", Region = "North America", Sales = 9999 });
            customers.Add(new Customer { ID = "B", City = "Mumbai", Country = "India", Region = "Asia", Sales = 8888 });
            customers.Add(new Customer { ID = "C", City = "Karachi", Country = "Pakistan", Region = "Asia", Sales = 7777 });
            customers.Add(new Customer { ID = "D", City = "Delhi", Country = "India", Region = "Asia", Sales = 6666 });
            customers.Add(new Customer { ID = "E", City = "S o Paulo", Country = "Brazil", Region = "South America", Sales = 5555 });
            customers.Add(new Customer { ID = "F", City = "Moscow", Country = "Russia", Region = "Europe", Sales = 4444 });
            customers.Add(new Customer { ID = "G", City = "Seoul", Country = "Korea", Region = "Asia", Sales = 3333 });
            customers.Add(new Customer { ID = "H", City = "Istanbul", Country = "Turkey", Region = "Asia", Sales = 2222 });
            customers.Add(new Customer { ID = "I", City = "Shanghai", Country = "China", Region = "Asia", Sales = 1111 });
            customers.Add(new Customer { ID = "J", City = "Lagos", Country = "Nigeria", Region = "Africa", Sales = 1000 });
            customers.Add(new Customer { ID = "K", City = "Mexico City", Country = "Mexico", Region = "North America", Sales = 2000 });
            customers.Add(new Customer { ID = "L", City = "Jakarta", Country = "Indonesia", Region = "Asia", Sales = 3000 });
            customers.Add(new Customer { ID = "M", City = "Tokyo", Country = "Japan", Region = "Asia", Sales = 4000 });
            customers.Add(new Customer { ID = "N", City = "Los Angeles", Country = "USA", Region = "North America", Sales = 5000 });
            customers.Add(new Customer { ID = "O", City = "Cairo", Country = "Egypt", Region = "Africa", Sales = 6000 });
            customers.Add(new Customer { ID = "P", City = "Tehran", Country = "Iran", Region = "Asia", Sales = 7000 });
            customers.Add(new Customer { ID = "Q", City = "London", Country = "UK", Region = "Europe", Sales = 8000 });
            customers.Add(new Customer { ID = "R", City = "Beijing", Country = "China", Region = "Asia", Sales = 9000 });
            customers.Add(new Customer { ID = "S", City = "Bogot", Country = "Colombia", Region = "South America", Sales = 1001 });
            customers.Add(new Customer { ID = "T", City = "Lima", Country = "Peru", Region = "South America", Sales = 2002 });
            orders = new List<Order>
            {
                new Order { ID="P", Amount = 100},
                new Order { ID="Q", Amount = 200},
                new Order { ID="R", Amount = 300},
                new Order { ID="S", Amount = 400},
                new Order { ID="T", Amount = 500},
                new Order { ID="U", Amount = 600},
                new Order { ID="V", Amount = 700},
                new Order { ID="W", Amount = 800},
                new Order { ID="X", Amount = 900},
                new Order { ID="Y", Amount = 1000},
                new Order { ID="Z", Amount = 1100}
            };
        }
    }
}
