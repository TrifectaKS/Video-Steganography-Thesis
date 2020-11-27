using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class BitMask
    {
        public static byte ChangeBit(int b, int pos, bool bit)
        {
            if (bit)
            {
                b |= 1 << pos;
            }
            else
            {
                b &= ~(1 << pos);
            }

            return (byte)b;
        }

        public static bool GetBit(int b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }
    }
}
