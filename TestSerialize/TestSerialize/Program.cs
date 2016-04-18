using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TestSerialize
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                List<Product> products = new List<Product>();
                products.Add(new Product(1, "Spiky Pung", 1000.0, "Good stuff."));
                products.Add(new Product(2, "Gloop Galloop Soup", 25.0, "Tasty."));
                products.Add(new Product(4, "Hat Sauce", 12.0, "One for the kids."));
                foreach (Product product in products)
                {
                    Console.WriteLine(product);
                }
                Console.WriteLine();
                IFormatter serializer = new BinaryFormatter();
                FileStream saveFile = new FileStream("Products.bin", FileMode.Create, FileAccess.Write);
                serializer.Serialize(saveFile, products);
                saveFile.Close();

                FileStream loadFile = new FileStream("Products.bin", FileMode.Open, FileAccess.Read);
                List<Product> savedProducts = serializer.Deserialize(loadFile) as List<Product>;
                loadFile.Close();
                Console.WriteLine("Products loaded:");
                foreach (Product product in savedProducts)
                {
                    Console.WriteLine(product);
                }
            }
            catch (SerializationException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }
    }
}
