using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;

namespace Testone
{
    class Program
    {
        static void Main(string[] args)
        {
            func9();
            Console.ReadLine();
        }
        static void func11()
        {
            string filename = "";
            string connstr = @"Persist Security Info=False;uid=sa;pwd=12345;database=EFExample;server=localhost";

            ViewDbHelper.StrConn = connstr;
            string sql = "select * from Images";
            DataTable dt = ViewDbHelper.GetTable(sql);
            byte[] bs = (byte[])(dt.Rows[0]["Image"]);
            MemoryStream ms = new MemoryStream(bs);
            filename = dt.Rows[0]["Name"].ToString();
            Image image = Image.FromStream(ms);
            //ms.Close();
            image.Save(filename);
            ms.Close();
        }
        static void func10()
        {
            string filename = "D:\\Desert.jpg";
            string connstr = @"Persist Security Info=False;uid=sa;pwd=12345;database=EFExample;server=localhost";
            
            ViewDbHelper.StrConn = connstr;
            ViewDbHelper.Insert2(filename);
        }
        static void func9()
        {
            //02 30 31 30 30 32 37 37 30 30 32 37 37 30 30 30 32 30 30 32 34 30 30 30 31 31 31 30 31 30 36 30 31 30 30 30 30 30 31 36 30 34 30 36 31 35 35 34 34 31 35 35 03
            byte[] bs = {0x02, 0x30, 0x31, 0x30, 0x30, 0x32, 0x37, 0x37, 0x30, 0x30, 0x32, 0x37, 
                            0x37, 0x30, 0x30, 0x30, 0x32, 0x30, 0x30, 0x32 ,0x34 ,0x30, 0x30, 0x30 ,0x31, 0x31, 0x31,
                            0x30 ,0x31 ,0x30, 0x36 ,0x30 ,0x31 ,0x30 ,0x30 ,0x30 ,0x30 ,0x30 ,0x31, 0x36 ,0x30, 0x34 ,
                            0x30 ,0x36 ,0x31 ,0x35, 0x35, 0x34, 0x34, 0x31, 0x35, 0x35, 0x03};
            DateTime dt = GetDateTime(bs, 38, 12);
            float value = GetValue(bs, 3, 5);
            byte result = 0;
            int length = bs.Length;
            for (int i = 1; i < length - 3; i++)
            {
                result ^= bs[i];
            }
            Console.WriteLine(result);
        }
        static void func8()
        {
            string source = "810120160115010009";
            string s2 = "820143780742865868";
            string s3 = "820143780742875868";
            string result = SterilizationBD18Decode(s3);
            Console.WriteLine(result);
        }
        static void func7()
        {
            string name = "01.手术器械 01";
            name = Regex.Replace(name, @"\W", "");
            Console.WriteLine(name);
            Console.ReadKey();
        }
        static void func6()
        {
            string str = "39868370000";
            string str2 = "";
            double value = Convert.ToDouble(str2);
            Console.WriteLine(value);
        }
        static void func5()
        {
            Process proc = null;
            try
            {
                DateTime dt1 = DateTime.Now;
                proc = new Process();
                proc.StartInfo.FileName = @"1.bat";
                //proc.StartInfo.Arguments = string.Format("10");//this is argument
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
                proc.WaitForExit();
                DateTime dt2 = DateTime.Now;
                TimeSpan ts = dt2 - dt1;
                Console.WriteLine(ts.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
            }
        }
        static void func4()
        {
            string timestring = "08:12:23";
            DateTime dt1 = Convert.ToDateTime(timestring);
            DateTime dt2 = new DateTime();
            Console.WriteLine(dt1.ToLongTimeString());
        }
        static void func3()
        {
            string str = "2015/12/16 19:57:00";
            DateTime dt = Convert.ToDateTime(str);
            dt = DateTime.Parse(str);
            dt = DateTime.ParseExact(str, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
            Console.WriteLine(dt.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        static void func2()
        {
            byte[] bs1 = new byte[] { 10, 0, 1, 2, 3 };
            byte[] aa = new byte[4];
            Array.Copy(bs1, 0, aa, 0, 4);
            foreach (byte b in aa)
            {
                Console.Write(b.ToString("D2")+" ");
            }
            Console.WriteLine();
            byte[] bs2 = new byte[] { 0,4, 5, 3,10 };
            byte[] bs3 = new byte[] { 1, 6, 8,10 };
            byte[] bs4 = new byte[] {0, 9, 2, 3, 10};
            byte[] bs5 = new byte[] {0, 1, 6, 7};
            List<byte[]> source = new List<byte[]>();
            source.Add(bs1);
            source.Add(bs2);
            source.Add(bs3);
            source.Add(bs4);
            source.Add(bs5);
            List<byte[]> result = SplitDatas( ref source, 0, 10);
            Console.WriteLine("输出结果：");
            foreach (byte[] bs in result)
            {
                Console.Write("行：");
                foreach(byte b in bs)
                {
                    Console.Write(b.ToString("D2")+ " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("输出剩余：");
            foreach (byte[] bs in source)
            {
                Console.Write("行：");
                foreach (byte b in bs)
                {
                    Console.Write(b.ToString("D2") + " ");
                }
                Console.WriteLine();
            }
        }
        public static string SterilizationBD18Decode(string sourceCode)
        {

            string newsourceCode = "81" + sourceCode.Substring(2);


            char[] temp = newsourceCode.ToCharArray();

            //[2][3]
            //[4][7][10][13][16]
            //4   5  6   7   8
            //[5][8][11][14][17]
            //9   10 11  12  13
            //[6][9][12][15]
            //14 15  16  17


            newsourceCode = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17}",
                                  8,
                                  1,
                                  temp[2].ToString(),
                                  temp[3].ToString(),
                                  temp[4].ToString(),
                                  temp[9].ToString(),
                                  temp[14].ToString(),
                                  temp[5].ToString(),
                                  temp[10].ToString(),
                                  temp[15].ToString(),
                                  temp[6].ToString(),
                                  temp[11].ToString(),
                                  temp[16].ToString(),
                                  temp[7].ToString(),
                                  temp[12].ToString(),
                                  temp[17].ToString(),
                                  temp[8].ToString(),
                                  temp[13].ToString()
                                     );

            sourceCode = (Convert.ToInt64(newsourceCode) - 27374757678797).ToString();
            return sourceCode;

        }
        public static List<byte[]> SplitDatas(ref List<byte[]> source, byte first, byte last)
        {
            List<byte[]> lstresult = new List<byte[]>();
            int length = 0;
            foreach (byte[] bs in source)
            {
                length += bs.Length;
            }
            byte[] allbytes = new byte[length];
            int findex = 0;
            foreach (byte[] bs in source)
            {
                bs.CopyTo(allbytes, findex);
                findex += bs.Length;
            }
            //foreach (byte[] bs in source)
            //{
            //    if (bs.Length == 0) continue;
            //    byte[] old = allbytes;
            //    allbytes = new byte[old.Length + bs.Length];
            //    old.CopyTo(allbytes, 0);
            //    bs.CopyTo(allbytes, old.Length);
            //}
            bool firstflag = false;
            int index = 0;
            int lastindex = -1;
            List<byte> lstbytes = new List<byte>();
            foreach (byte b in allbytes)
            {
                if (b == first)
                {
                    firstflag = true;
                    lstbytes.Clear();
                }
                lstbytes.Add(b);
                if (b == last)
                {
                    if (firstflag)
                    {
                        lstresult.Add(lstbytes.ToArray());
                    }
                    firstflag = false;
                    lastindex = index;
                }
                index++;
            }
            source.Clear();
            if (firstflag)
            {
                if (lastindex >= 0)
                {
                    if (allbytes.Length > lastindex + 1)
                    {
                        byte[] bs = new byte[allbytes.Length - lastindex - 1];
                        for (int i = lastindex + 1; i < allbytes.Length; i++)
                        {
                            bs[i - lastindex - 1] = allbytes[i];
                        }
                        source.Add(bs);
                    }
                    else source.Add(new byte[0]);
                }
            }
            else source.Add(new byte[0]);
            return lstresult;
        }
        protected static List<byte[]> SplitDatasByFirst(ref List<byte[]> source, byte first)
        {
            List<byte[]> lstresult = new List<byte[]>();
            byte[] allbytes = new byte[0];
            foreach (byte[] bs in source)
            {
                if (bs.Length == 0) continue;
                byte[] old = allbytes;
                allbytes = new byte[old.Length + bs.Length];
                old.CopyTo(allbytes, 0);
                bs.CopyTo(allbytes, old.Length);
            }
            bool firstflag = false;
            int index = 0;
            List<byte> lstbytes = new List<byte>();
            foreach (byte b in allbytes)
            {
                if (b == first)
                {
                    if (lstbytes.Count > 0 && firstflag)
                    {
                        lstresult.Add(lstbytes.ToArray());
                    }
                    lstbytes.Clear();
                    firstflag = true;
                }
                lstbytes.Add(b);
                index++;
            }
            source.Clear();
            if (firstflag)
            {
                if (lstbytes.Count > 0)
                {
                    source.Add(lstbytes.ToArray());
                }
                else source.Add(new byte[0]);
            }
            return lstresult;
        }
        protected static List<byte[]> SplitDatasByLast(ref List<byte[]> source, byte last)
        {
            List<byte[]> lstresult = new List<byte[]>();
            byte[] allbytes = new byte[0];
            foreach (byte[] bs in source)
            {
                if (bs.Length == 0) continue;
                byte[] old = allbytes;
                allbytes = new byte[old.Length + bs.Length];
                old.CopyTo(allbytes, 0);
                bs.CopyTo(allbytes, old.Length);
            }
            int index = 0;
            int lastindex = -1;
            List<byte> lstbytes = new List<byte>();
            foreach (byte b in allbytes)
            {
                lstbytes.Add(b);
                if (b == last)
                {
                    lstresult.Add(lstbytes.ToArray());
                    lstbytes.Clear();
                    lastindex = index;
                }
                index++;
            }
            source.Clear();
            if (lastindex >= 0)
            {
                if (allbytes.Length > lastindex + 1)
                {
                    byte[] bs = new byte[allbytes.Length - lastindex - 1];
                    for (int i = lastindex + 1; i < allbytes.Length; i++)
                    {
                        bs[i - lastindex - 1] = allbytes[i];
                    }
                    source.Add(bs);
                }
                else source.Add(new byte[0]);
            }
            return lstresult;
        }
        static void func()
        {
            string buffer = "AP850D00001=   READY TO UNLOAD    = 2EC54CB0CFA8123AP9F9D00000DOOR OPEN                B1CD4CB0F8EB";
            while (buffer.Contains(""))
            {
                int i = buffer.IndexOf("");
                if (i > 12)
                {
                    string str = buffer.Substring(0, i - 12);
                    buffer = buffer.Substring(i + 1);
                    if (str.Contains(""))
                    {
                        int i2 = str.LastIndexOf("") + 13;
                        if (i2 >= str.Length) continue;
                        string MsgParseText = str.Substring(i2).Trim();
                        Console.WriteLine(MsgParseText);
                    }
                }
                else
                {
                    buffer = buffer.Substring(i + 1);
                }
            }
        }

        static bool Checkout(byte[] datas)
        {
            byte result = 0;
            int length = datas.Length;
            for (int i = 1; i < length - 3; i++)
            {
                result ^= datas[i];
            }
            byte crc1 = (byte)(result / 10 + 0x30);
            byte crc0 = (byte)(result % 10 + 0x30);
            if (crc1 == datas[length - 3] && crc0 == datas[length - 2])
                return true;
            else
                return false;
        }
        public static DateTime GetDateTime(byte[] datas, int index, int count)
        {
            //151015110834
            try
            {
                string str = "20" + Encoding.ASCII.GetString(datas, index, count).Trim();
                DateTime dt = DateTime.ParseExact(str, "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
                return dt;
            }
            catch
            {
                throw;
            }
        }
        public static float GetValue(byte[] datas, int index, int count)
        {
            try
            {
                string str = Encoding.ASCII.GetString(datas, index, count).Trim();//注意负号情况
                float value = Convert.ToSingle(str);
                return value;
            }
            catch
            {
                throw;
            }
        }
    }
    public class ViewDbHelper
    {
        //连接字符串
        public static string StrConn = "";

        public static void Insert2(string filename)
        {
            Stream ms = File.Open(filename, FileMode.Open, FileAccess.Read);
            byte[] picbyte = new byte[ms.Length];
            ms.Read(picbyte, 0, picbyte.Length);
            ms.Close();
            try
            {
                SqlConnection conn = new SqlConnection(StrConn);
                conn.Open();
                //string strsql = string.Format("insert into images(Name, Image) values('{0}', '{1}')", filename, picbyte);
               
                SqlCommand cmd = new SqlCommand("InsertImages", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(@"Image", SqlDbType.Image);
                cmd.Parameters["Image"].Value = picbyte;
                cmd.Parameters.Add(@"Name", SqlDbType.NVarChar);
                cmd.Parameters["Name"].Value = filename;
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("插入成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine("插入失败");
            }
        }
        public static void Insert(string filename)
        {
            
            Stream ms = File.Open(filename, FileMode.Open, FileAccess.Read);
            byte[] picbyte = new byte[ms.Length];
            ms.Read(picbyte, 0, picbyte.Length);
            ms.Close();
            try
            {
                SqlConnection conn = new SqlConnection(StrConn);
                conn.Open();
                //string strsql = string.Format("insert into images(Name, Image) values('{0}', '{1}')", filename, picbyte);
                string strsql = string.Format("insert into images(Name, Image) values(@Name, @Image)");
                SqlCommand cmd = new SqlCommand(strsql, conn);
                cmd.Parameters.Add(@"Image", SqlDbType.Image);
                cmd.Parameters["Image"].Value = picbyte;
                cmd.Parameters.Add(@"Name", filename);
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("插入成功");
            }
            catch(Exception ex)
            {
                Console.WriteLine("插入失败");
            }
        }
        public static DataTable GetTable(string strSql)
        {
            return GetTable(strSql, null);
        }

        public static DataTable GetTable(string strSql, SqlParameter[] pas)
        {
            return GetTable(strSql, pas, CommandType.Text);
        }

        public static DataTable GetTable(string strSql, SqlParameter[] pas, CommandType cmdtype)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(StrConn.Trim()))
            {
                SqlDataAdapter da = new SqlDataAdapter(strSql, conn);
                da.SelectCommand.CommandType = cmdtype;
                if (pas != null)
                {
                    da.SelectCommand.Parameters.AddRange(pas);
                }
                da.Fill(dt);
            }
            return dt;
        }
    }
}
