using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsNewAttributes;

[assembly: SupportWhatsNew]
namespace VectorClass
{
    [LastModified("14 Feb 2010", "IEnumerable interface implemented  " +
            "So Vector can now be treated as a collection")]
    [LastModified("10 Feb 2010", "IFormattable interface implemented  " +
            "So Vector now responds to format specifiers N and VE")]
    public class Vector: IFormattable
    {
        public double x, y, z;
        public Vector(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        [LastModified("10 Feb 2010", "Method addid in order to provide formatting support")]
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) return ToString();
            return ToString();
        }
    }
}
