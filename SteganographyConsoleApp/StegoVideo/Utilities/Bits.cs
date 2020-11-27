using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class Bits
    {
        Dictionary<long, bool> _values;

        public long Count { get { return _values.Count; } }
        
        public int ByteCount { get { return _values.Count / 8; } }

        public Bits()
        {
            _values = new Dictionary<long, bool>();
        }

        public Bits(byte[] data)
        {
            _values = ToBits(data)._values;
        }

        public Bits(byte data)
        {
            _values = ToBits(new byte[] { data })._values;
        }

        public void Push(char c)
        {
            if (c == '0') Push(false);
            else Push(true);
        }

        public void Push(int v)
        {
            if (v == 0) Push(false);
            else Push(true);
        }

        public void Push(bool b)
        {
            _values.Add(Count, b);
        }
        
        public bool Pop()
        {
             bool val = _values.First().Value;
            _values.Remove(_values.First().Key);
            return val;
        }

        public bool GetParityBit()
        {
            return _values.Values.Last();
        }

        public void SetParityBit(bool b)
        {
            bool val = _values.Last().Value;
            long key = _values.Last().Key;
            _values.Remove(key);
            _values.Add(key, b);
        }

        public void FlipParityBit()
        {
            bool val = _values.Last().Value;
            val = !val;
            long key = _values.Last().Key;
            _values.Remove(key);
            _values.Add(key, val);
        }

        public Bits PopBits(int amount)
        {
            Bits temp = new Bits();
                for (int i = 0; i < amount && Count > 0; i++)
                {
                    temp.Push(Pop());
                }
            return temp;
        }
        
        public bool this[int i]
        {
            get
            {
                return _values[i];
            }
            set
            {
                _values[i] = value;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(bool b in _values.Values)
            {
                if (b) sb.Append("1");
                else sb.Append("0");
            }

            return sb.ToString();
        }

        public int ToInt32()
        {
            byte[] b = ToBytes();
            byte[] fourb = new byte[4];
            for(int i = 0; i < 4; i++)
            {
                try { fourb[i] = b[i]; } catch { fourb[i] = 0; }
            }
            return BitConverter.ToInt32(fourb, 0);
        }

        public double ToDouble()
        {
            byte[] b = ToBytes();
            byte[] eightb = new byte[8];
            for(int i = 0; i < 8; i++)
            {
                try { eightb[i] = b[i]; } catch { eightb[i] = 0; }
            }
            return BitConverter.ToDouble(eightb, 0);
        }

        public byte[] ToBytes()
        {
            string input = ToString();
            int numOfBytes = input.Length / 8;
            byte[] bytes;

            if (numOfBytes == 0)
            {
                bytes = new byte[1];
                input = input.PadLeft(8, '0');
                bytes[0] = Convert.ToByte(input,2);
            }
            else
            {
                bytes = new byte[numOfBytes];
                for (int i = 0; i < numOfBytes; ++i)
                {
                    bytes[i] = Convert.ToByte(input.Substring(8 * i, 8), 2);
                }
            }

            return bytes;
        }

        public static Bits ToBits(byte[] data)
        {
            Bits b = new Bits();
            foreach (byte d in data)
            {
                foreach (char c in Convert.ToString(d, 2).PadLeft(8, '0'))
                {
                    b.Push(c);
                }
            }
            return b;
        }

        public static byte[] ToBytes(Bits data)
        {
            string input = data.ToString();
            int numOfBytes = input.Length / 8;
            byte[] bytes = new byte[numOfBytes];
            for (int i = 0; i < numOfBytes; ++i)
            {
                bytes[i] = Convert.ToByte(input.Substring(8 * i, 8), 2);
            }

            return bytes;
        }

    }
}
