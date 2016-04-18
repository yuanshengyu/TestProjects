using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace ConsoleApplication5
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Func();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
        static private void Func(){
            try{
                int b = 0;
                int a = 3/b;
            }
            catch(Exception ex){
                Console.WriteLine("before string");
                throw new Exception("new exception: "+ex.Message);
            }
            finally{
                Console.WriteLine("first string");

            }
            Console.WriteLine("second string");
        }
        static private void CreateImage(List<string> valueCounter, string filename)//像素分布统计柱状图
        {
            int height = 500, width = 700;
            Bitmap image = new Bitmap(width, height);
            //创建Graphics类对象
            Graphics g = Graphics.FromImage(image);
            try
            {
                //清空图片背景色
                g.Clear(Color.White);
                Font font = new Font("Arial", 10, FontStyle.Regular);
                Font font1 = new Font("宋体", 20, FontStyle.Bold);
                System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height),
                Color.Blue, Color.BlueViolet, 1.2f, true);
                g.FillRectangle(Brushes.WhiteSmoke, 0, 0, width, height);
                // Brush brush1 = new SolidBrush(Color.Blue);
                g.DrawString("InfeconBDTest像素分布统计柱状图", font1, brush, new PointF(70, 30));
                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Blue), 0, 0, image.Width - 1, image.Height - 1);

                Pen mypen = new Pen(brush, 1);
                Pen mypen1 = new Pen(Color.Blue, 2);

                //绘制线条
                PointF contentPoint = new PointF(60, 80);//坐标区域左上角
                SizeF contentSize = new SizeF(620, 380);//坐标区域大小
                int vLineCount = valueCounter.Count;//竖线数量
                int hLineCount = valueCounter.Count;//横线数量
                float pillarWith = 20;//柱状图宽度
                float pillarHeadTitleSplite = 16;//柱状图头部文字间隔

                #region 绘制坐标轴
                //绘制纵向线条
                g.DrawLine(mypen1, contentPoint.X, contentPoint.Y, contentPoint.X, contentSize.Height + contentPoint.Y);
                for (float i = 0; i < vLineCount; i++)
                {
                    float xx = contentPoint.X + (i + 1) * (contentSize.Width / vLineCount);
                    g.DrawLine(mypen, xx, contentPoint.Y, xx, contentSize.Height + contentPoint.Y);
                }
                //绘制横向线条
                g.DrawLine(mypen1, contentPoint.X, contentSize.Height + contentPoint.Y, contentSize.Width + contentPoint.X, contentSize.Height + contentPoint.Y);
                for (float i = 0; i < hLineCount; i++)
                {
                    float yy = contentPoint.Y + (i) * (contentSize.Height / hLineCount);
                    g.DrawLine(mypen, contentPoint.X, yy, contentSize.Width + contentPoint.X, yy);
                }
                #endregion

                #region x轴下表和y轴刻度的绘制
                //x轴下标-这里绘制的是10根柱子柱子下的数字并不是刻度，而是名称，所以是均分x轴。
                int max = 0;
                int subTitle = 7;
                int[] values = new int[vLineCount];
                for (int i = 0; i < vLineCount; i++)
                {
                    values[i] = Convert.ToInt32(valueCounter[i]);
                    if (values[i] > max)
                        max = values[i];

                    float xx = contentPoint.X + (i + 1) * (contentSize.Width / vLineCount) - (contentSize.Width / vLineCount) / 2;
                    g.DrawString(subTitle.ToString(), font, Brushes.Blue, xx, contentSize.Height + contentPoint.Y + 5); //设置文字内容及输出位置
                    subTitle += 5;
                }
                //y轴
                for (float i = 0; i < hLineCount; i++)
                {
                    float yy = contentPoint.Y + (i) * (contentSize.Height / hLineCount);

                    if (max == 0)
                    {
                        if (i != hLineCount - 1)
                            continue;
                    }

                    g.DrawString(Convert.ToInt32((max * ((hLineCount - i) / hLineCount))).ToString(), font, Brushes.Blue, 5, yy); //设置文字内容及输出位置
                }
                #endregion

                #region 绘制柱状图
                //绘制柱状图.
                Font font2 = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
                SolidBrush mybrush = new SolidBrush(Color.Red);

                for (int i = 0; i < values.Length; i++)
                {
                    float xx = contentPoint.X + (i + 1) * (contentSize.Width / vLineCount) - (contentSize.Width / vLineCount) / 2 - pillarWith / 2;
                    float relValue = 0;

                    if (max != 0)
                        relValue = Convert.ToSingle((decimal)values[i] / (decimal)max * (decimal)contentSize.Height);


                    g.FillRectangle(mybrush, xx, contentPoint.Y + contentSize.Height - relValue, pillarWith, relValue);
                    g.DrawString(values[i].ToString(), font2, Brushes.Red, xx, contentPoint.Y + contentSize.Height - relValue - pillarHeadTitleSplite);
                }
                #endregion

                //绘制标识
                //Font font3 = new System.Drawing.Font("Arial", 10, FontStyle.Regular);
                //g.DrawRectangle(new Pen(Brushes.Blue), 170, 400, 250, 50); //绘制范围框
                //g.FillRectangle(Brushes.Red, 270, 410, 20, 10); //绘制小矩形
                //g.DrawString("报名人数", font3, Brushes.Red, 292, 408);
                //g.FillRectangle(Brushes.Green, 270, 430, 20, 10);
                //g.DrawString("通过人数", font3, Brushes.Green, 292, 428);

                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                image.Save(filename);
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }
    }
}
