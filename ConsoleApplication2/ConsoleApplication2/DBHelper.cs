using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace TestReflection
{
    public class DBHelper
    {
        public static List<T> GetEntity<T>(DataTable dt)
            where T:BaseEntity, new() 
        {
            List<T> results = new List<T>();
            PropertyInfo[] pis = typeof(T).GetProperties();
            foreach (DataRow dr in dt.Rows)
            {
                T result = new T();
                foreach (DataColumn dc in dt.Columns)
                {
                    bool flag = false;
                    string caption = dc.Caption.ToUpper();
                    foreach (PropertyInfo pi in pis)
                    {
                        object[] attrs = pi.GetCustomAttributes(false);
                        foreach (object obj in attrs)
                        {
                            if (obj is MappingFieldAttribute)
                            {
                                string name = ((MappingFieldAttribute)obj).Name;
                                bool nullable = ((MappingFieldAttribute)obj).Nullable;
                                if (name.ToUpper() == caption)
                                {
                                    string value = dr[caption].ToString();
                                    if (!pi.PropertyType.IsGenericType)
                                    {
                                        pi.SetValue(result, string.IsNullOrEmpty(value) ? null : Convert.ChangeType(value, pi.PropertyType), null);
                                    }
                                    else
                                    {
                                        Type generic = pi.PropertyType.GetGenericTypeDefinition();
                                        if (generic == typeof(Nullable<>))
                                        {
                                            pi.SetValue(result, string.IsNullOrEmpty(value) ? null : Convert.ChangeType(value, Nullable.GetUnderlyingType(pi.PropertyType)), null);
                                        }
                                    }
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (flag)
                        {
                            break;
                        }
                    }
                }
                results.Add(result);
            }
            return results;
        }
    }
}
