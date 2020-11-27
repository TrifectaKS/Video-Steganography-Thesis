using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class PosToXY
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public PosToXY(int w, int h)
        {
            Width = w;
            Height = h;
        }

        public Point ToXY(int pos)
        {
            return new Point(pos % Width, pos / Width);
        }

        public int ToPos(int x, int y)
        {
            return x + Width * y;
        }

    }
}
