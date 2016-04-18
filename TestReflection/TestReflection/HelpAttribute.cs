using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestReflection
{
    [AttributeUsage(AttributeTargets.All)]
    class HelpAttribute:System.Attribute
    {
        public readonly string Url;
        private string topic;
        public string Topic
        {
            get { return topic; }
            set { topic = value; }
        }
        public HelpAttribute(string url)
        {
            this.Url = url;
        }
        public override string  ToString()
        {
 	         return Url;
        }
    }
}
