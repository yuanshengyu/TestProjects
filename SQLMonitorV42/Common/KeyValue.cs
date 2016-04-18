using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Xnlab.SQLMon
{
    public class KeyValue<K, V>
    {
        public K Key { get; set; }
        public V Value { get; set; }

        public KeyValue(K Key, V Value)
        {
            this.Key = Key;
            this.Value = Value;
        }
    }
}
