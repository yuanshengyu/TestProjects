using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestImage
{
    class Program
    {
        static void Main(string[] args)
        {
            GenerateThumbnail.GenThumbnail(@"E:\Desert.jpg", @"E:\缩略图.jpg", 101, 76);

        }
    }
}
