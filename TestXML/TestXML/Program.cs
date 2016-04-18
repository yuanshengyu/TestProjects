using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Data;

namespace TestXML
{
    class Program
    {
        static void Main(string[] args)
        {
            Deserialize();
            Console.ReadLine();
        }
        static void GetDataSetFromXML()
        {
            DataSet ds = new DataSet();
            ds.ReadXml(@"D:\TestXML.xml");
            DataTable dt = ds.Tables[0];
            foreach (DataColumn dc in dt.Columns)
            {
                Console.Write(dc.Caption.PadRight(10));
            }
            Console.WriteLine("");
            foreach (DataRow dr in dt.Rows)
            {
                foreach (DataColumn dc in dt.Columns)
                {
                    Console.Write(dr[dc.Caption].ToString().PadRight(10));
                }
                Console.WriteLine("");
            }
        }
        static void Deserialize()//反序列化
        {
            List<Test> lts = new List<Test>();
            Type type = lts.GetType();
            XmlSerializer xs = new XmlSerializer(type);//要反序列化的对象只能是XML中最外面的那个元素
            FileStream fs = new FileStream(@"D:\TestXML.xml", FileMode.Open, FileAccess.Read);
            lts = xs.Deserialize(fs) as List<Test>;
            foreach (Test t0 in lts)
            {
                Console.WriteLine(string.Format("姓名：{0}，年龄：{1}，ID：{2}", t0.Name, t0.Age, t0.ID));
            }
        }
        static void Serialize()//序列化
        {
            Test t1 = new Test(2, "张三", 16);
            Test t2 = new Test(4, "李四", 24);
            Test t3 = new Test(7, "王五", 12);
            List<Test> lts = new List<Test>();
            lts.Add(t1);
            lts.Add(t2);
            lts.Add(t3);
            XmlSerializer xs = new XmlSerializer(lts.GetType());
            FileStream fs = new FileStream(@"D:\TestXML.xml", FileMode.OpenOrCreate, FileAccess.Write);
            xs.Serialize(fs, lts);//写入所有公共成员（不包括方法）
        }
    }
    public class Test
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Age;
        public Test()
        {

        }
        public Test(int id, string name, int age)
        {
            ID = id;
            Name = name;
            Age = age;
        }
        public int GetAge()
        {
            return Age;
        }
    }
}
