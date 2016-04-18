using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsNewAttributes
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method,AllowMultiple =true, Inherited = false)]
    public class LastModifiedAttribute:Attribute
    {
        private readonly DateTime dateModified;
        private readonly string changes;
        public LastModifiedAttribute(string datemodified, string schanges)
        {
            dateModified = DateTime.Parse(datemodified);
            changes = schanges;
        }
        public DateTime DateModified
        {
            get { return dateModified; }
        }
        public string Changes
        {
            get { return changes; }
        }
        public string Issues { get; set; }
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class SupportWhatsNewAttribute : Attribute
    {

    }
}
