using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRLibrary
{
    class Point: IEquatable<Point>
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }


        public bool Equals(Point another)
        {
            return X == another.X && Y == another.Y;
        }

        public override int GetHashCode()
        {
            return X + Y;
        }

        public override string ToString()
        {
            return X+":"+Y;
        }
    }
}
