using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;

namespace TestCSV
{
    public class CSVFileHelper
    {
        static private string fileName;
        static private StreamReader sr;
        static public void LoadFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new Exception("文件名不能为空");
            }
            FileInfo info = new FileInfo(filename);
            if (!info.Exists)
            {
                throw new Exception(string.Format("文件{0}不存在", filename));
            }
            fileName = filename;
            sr = new StreamReader(filename, Encoding.Default);
        }
        static public DataTable GetDataTable()
        {
            DataTable dt = new DataTable("DataTable");
            string line = sr.ReadLine().Trim();
            if (string.IsNullOrEmpty(line))
            {
                return null;
            }
            string[] strs = Regex.Split(line, ";");
            foreach (string str in strs)
            {
                dt.Columns.Add(str.Trim());
            }
            int count = dt.Columns.Count;
            while (true)
            {
                line = sr.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }
                line = line.Trim();
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }
                strs = Regex.Split(line, ";");
                if (count != strs.Length)
                {
                    continue;
                }
                DataRow dr = dt.NewRow();

                for (int i = 0; i < count; i++)
                {
                    dr[i] = strs[i];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
        static public void CloseFile()
        {
            sr.Close();
        }
    }
}
