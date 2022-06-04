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
    }
}
