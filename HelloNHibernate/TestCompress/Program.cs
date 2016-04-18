using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace TestCompress
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] b1 = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                b1[i] = (byte)(i % 256);
                b1[i] = (byte)(255-i%4);
            }
            DateTime dt1 = DateTime.Now;
            byte[] b2 = Compress(b1);
            DateTime dt2 = DateTime.Now;
            Console.WriteLine(string.Format("耗时{0}ms，长度={1}",(dt2-dt1).TotalMilliseconds,b2.Length));
            byte[] b3= Compress(b2);
            DateTime dt3 = DateTime.Now;
            Console.WriteLine(string.Format("耗时{0}ms，长度={1}", (dt3 - dt2).TotalMilliseconds, b3.Length));
            byte[] b4 = Decompress(b3);
            byte[] b5 = Decompress(b4);
            Console.ReadLine();
        }
        /// <summary>
        /// 压缩字节数组
        /// </summary>
        /// <param name="str"></param>
        public static byte[] Compress(byte[] inputBytes)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                using (GZipStream zipStream = new GZipStream(outStream, CompressionMode.Compress, true))
                {
                    zipStream.Write(inputBytes, 0, inputBytes.Length);
                    zipStream.Close(); //很重要，必须关闭，否则无法正确解压
                    return outStream.ToArray();
                }
            }
        }

        /// <summary>
        /// 解压缩字节数组
        /// </summary>
        /// <param name="str"></param>
        public static byte[] Decompress(byte[] inputBytes)
        {

            using (MemoryStream inputStream = new MemoryStream(inputBytes))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (GZipStream zipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        zipStream.CopyTo(outStream);
                        zipStream.Close();
                        return outStream.ToArray();
                    }
                }

            }
        }
    }
}
