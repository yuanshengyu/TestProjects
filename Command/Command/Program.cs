using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Command
{
    class Program
    {
        static void Main(string[] args)
        {
            Line line = new Line(new Point(10, 10), new Point(100, 10));
            Rectangle rectangle = new Rectangle(new Point(20, 20), new Point(50, 30));
            Circle circle = new Circle(new Point(500, 500), 200);

            Graphics graphics = new Graphics();
            graphics.Draw(line);
            graphics.Draw(rectangle);
            graphics.Undo();
            graphics.Draw(circle);

            Console.ReadLine();
        }
    }
}
