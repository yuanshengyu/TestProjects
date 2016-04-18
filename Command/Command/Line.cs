using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Command
{
    public class Line : IGraphCommand
    {
        private Point startPoint;
        private Point endPoint;
        public Line(Point start, Point end)
        {
            startPoint = start;
            endPoint = end;
        }

        public void Draw()
        {
            Console.WriteLine("Draw Line:{0} To {1}", startPoint.ToString(), endPoint.ToString());
        }

        public void Undo()
        {
            Console.WriteLine("Erase Line:{0} To {1}", startPoint.ToString(), endPoint.ToString());
        }
    }

    public class Rectangle : IGraphCommand
    {
        private Point topLeftPoint;
        private Point bottomRightPoint;
        public Rectangle(Point topLeft, Point bottomRight)
        {
            topLeftPoint = topLeft;
            bottomRightPoint = bottomRight;
        }

        public void Draw()
        {
            Console.WriteLine("Draw Rectangle: Top Left Point {0},  Bottom Right Point {1}", topLeftPoint.ToString(), bottomRightPoint.ToString());
        }

        public void Undo()
        {
            Console.WriteLine("Erase Rectangle: Top Left Point {0},  Bottom Right Point {1}", topLeftPoint.ToString(), bottomRightPoint.ToString());
        }
    }

    public class Circle : IGraphCommand
    {
        private Point centerPoint;
        private int radius;
        public Circle(Point center, int radius)
        {
            centerPoint = center;
            this.radius = radius;
        }

        public void Draw()
        {
            Console.WriteLine("Draw Circle: Center Point {0},  Radius {1}", centerPoint.ToString(), radius.ToString());
        }

        public void Undo()
        {
            Console.WriteLine("Erase Circle: Center Point {0},  Radius {1}", centerPoint.ToString(), radius.ToString());
        }
    }
}
