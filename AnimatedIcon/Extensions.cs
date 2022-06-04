using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace AnimatedIcon
{
    public static class Extensions
    {
        public static double GetDistance(this Point source, Point destination)
        {
            return Math.Sqrt(Math.Pow((source.X - destination.X), 2) + Math.Pow((source.Y - destination.Y), 2));
        }

        public static Point TopLeft(this Rectangle rect)
        {
            return rect.Location;
        }

        public static Point BottomRight(this Rectangle rect)
        {
            return new Point(rect.X + rect.Width, rect.Y + rect.Height);
        }

        public static double GetDiagonal(this Rectangle rect)
        {
            return rect.TopLeft().GetDistance(rect.BottomRight());
        }
    }
}
