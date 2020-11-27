using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace AudioSteganography
{
    public class LSBAudioStego
    {
        private byte[] secretMessage;
        private NonRepeatingRandom nrr;
        private WaveFile waveFile;

        public LSBAudioStego(WaveFile wf, string key)
        {
            int numKey = 0;
            foreach (char c in key.ToCharArray())
            {
                numKey += c;
            }
            waveFile = wf;
            nrr = new NonRepeatingRandom(numKey);
        }

        public LSBAudioStego(string key, string message, WaveFile wf) : this(wf, key)
        {
            secretMessage = Encoding.ASCII.GetBytes(message);
        }

        public WaveFile Embed()
        {
            int length = secretMessage.Length;
            //Embed Metadata in first frame
            Console.WriteLine("Audio Message Length: " + length);
            byte[] lengthBytes = BitConverter.GetBytes(length);

            //region
            double srs = Math.Floor((double)(waveFile.Data.Length - 4) / (double)(length));

            byte[] srsBytes = BitConverter.GetBytes(srs);

            Console.WriteLine("Sample Region Size: " + srs);
            Console.WriteLine("Size: " + waveFile.Data.Length);
            Console.WriteLine("SC2S: " + waveFile.Subchunk2Size);

            //save length as first 32 bits
            EmbedInSampleRegion(nrr.GetRandomSequence(100, 200, 32), new Bits(lengthBytes));

            //save srs in the next 64 bits
            EmbedInSampleRegion(nrr.GetRandomSequence(201, 300, 64), new Bits(srsBytes));

            Console.WriteLine("Embed Length: " + length);
            for (int i = 0, c = 301, bytecount = 0; i < length && c + (int)srs - 1 < waveFile.Data.Length; i++, bytecount++, c += (int)srs)
            {
                EmbedInSampleRegion(nrr.GetRandomSequence(c, c + (int)srs - 1, 8), new Bits(secretMessage[bytecount]));
            }

            return waveFile;

        }

        public byte[] Extract()
        {
            int length = ExtractFromSampleRegion(nrr.GetRandomSequence(100, 200, 32)).ToInt32();
            double srs = ExtractFromSampleRegion(nrr.GetRandomSequence(201, 300, 64)).ToDouble();

            Console.WriteLine("Extract Length: " + length);

            Console.WriteLine("Size: " + waveFile.Data.Length);

            Console.WriteLine("Sample Region Size: " + srs);
            Console.WriteLine("SC2S: " + waveFile.Subchunk2Size);
            List<byte> secretmessage = new List<byte>();

            BinaryWriter bw = new BinaryWriter(new MemoryStream());
            for (int i = 0, c = 301; i < length && c + (int)srs - 1 < waveFile.Data.Length; i++, c += (int)srs)
            {
                byte b = ExtractFromSampleRegion(nrr.GetRandomSequence(c, c + (int)srs - 1, 8)).ToBytes()[0];
                bw.Write(b);
            }

            bw.Close();
            bw.BaseStream.Close();

            return StreamToBytes.StreamToBytesArray(bw.BaseStream);
        }

        private Bits ExtractFromSampleRegion(IEnumerable<int> sequence)
        {
            Bits b = new Bits();
            foreach (int i in sequence)
            {
                byte val = waveFile.Data[i];
                bool bit = BitMask.GetBit(val, 0);
                b.Push(bit);
            }

            return b;
        }

        private void EmbedInSampleRegion(IEnumerable<int> sequence, Bits b)
        {
            foreach (int i in sequence)
            {
                bool bit = b.Pop();
                byte val = BitMask.ChangeBit(waveFile.Data[i], 0, bit);
                waveFile.Data[i] = val;
            }
        }

    }
}
