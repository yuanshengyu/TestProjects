using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TestImage
{
    public class GenerateThumbnail
    {
        /// <summary>
        /// 生成缩略图的静态方法
        /// </summary>
        /// <param name="pathfrom">源图的路径(含文件名及扩展名)</param>
        /// <param name="pathto">生成的缩略图所保存的路径(含文件名及扩展名)</param>
        /// <param name="width">欲生成的缩略图的宽度</param>
        /// <param name="height">欲生成的缩略图的高度</param>
        public static void GenThumbnail(string pathfrom, string pathto, int width, int height)
        {
            Image imageFrom = null;
            try
            {
                imageFrom = Image.FromFile(pathfrom);
            }
            catch
            {
                throw;
            }
            if (imageFrom == null)
                return;
            //源图宽度及高度
            int imageFromWidth = imageFrom.Width;
            int imageFromHeight = imageFrom.Height;
            //生成的缩略图
            int bitmapWidth = width;
            int bitmapHeight = height;
            int X = 0;
            int Y = 0;
            if (bitmapHeight * imageFromWidth > bitmapWidth * imageFromHeight)
            {
                bitmapHeight = imageFromHeight * width / imageFromWidth;
                Y = (height - bitmapHeight) / 2;
            }
            else
            {
                bitmapWidth = imageFromWidth * height / imageFromHeight;
                X = (width - bitmapWidth) / 2;
            }
            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.DrawImage(imageFrom, new Rectangle(X, Y, bitmapWidth, bitmapHeight), new Rectangle(0, 0, imageFromWidth, imageFromHeight), GraphicsUnit.Pixel);
            try
            {
                bmp.Save(pathto, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch
            {
                throw;
            }
            finally
            {
                imageFrom.Dispose();
                bmp.Dispose();
                g.Dispose();
            }
        }

        public static Image GetImageFromFile(string filename)
        {
            Stream s = File.Open(filename, FileMode.Open, FileAccess.Read);
            byte[] picbyte = new byte[s.Length];
            s.Read(picbyte, 0, picbyte.Length);
            s.Close();
            MemoryStream ms = new MemoryStream(picbyte);
            return new BinaryFormatter().Deserialize(ms) as Image;
        }
    }
}
