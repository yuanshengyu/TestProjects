using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace TestEnumrator
{
    public class Friends : IEnumerable
    {
        private Friend[] friendarray;
        public Friends()
        {
            friendarray = new Friend[]
            {
                new Friend("张三"),
                 new Friend("李四"),
                new Friend("王五")
             };
        }
        //索引器
        public Friend this[int index]
        {
            get { return friendarray[index]; }
        }
        public int Count
        {
            get { return friendarray.Length; }
        }
        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < friendarray.Length; i++)
            {
                yield return friendarray[i];
            }
        }
    }
}
