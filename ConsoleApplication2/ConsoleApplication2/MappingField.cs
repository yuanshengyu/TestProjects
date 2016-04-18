using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestReflection
{
    [AttributeUsage(AttributeTargets.All)]
    public class MappingFieldAttribute:Attribute
    {
        /// <summary>
        /// 对应数据表中的字段名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 是否可空
        /// </summary>
        public bool Nullable { get; set; }
        public MappingFieldAttribute(string name, bool nullable)
        {
            Name = name;
            Nullable = nullable;
        }
        public MappingFieldAttribute(string name)
        {
            Name = name;
            Nullable = true;
        }
    }
}
