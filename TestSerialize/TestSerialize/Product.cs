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
    [Serializable]
    public class Product
    {
        public long Id;
        public string Name;
        public double Price;

        [NonSerialized]
        String Notes;
        public Product(long id, string name, double price, string notes)
        {
            Id = id;
            Name = name;
            Price = price;
            Notes = notes;
        }
        public override string ToString()
        {
            return string.Format("{0}: {1} (${2:F2}) {3}", Id, Name, Price, Notes);
        }
    }
}
