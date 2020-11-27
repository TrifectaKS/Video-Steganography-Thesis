using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSteganography
{
    public class WaveFile
    {
        //http://soundfile.sapp.org/doc/WaveFormat/

        //Header Data
        public List<byte[]> Header { get; private set; }
        public byte[] Data { get; set; }
        public int ChunkID { get { return BitConverter.ToInt32(Header[0], 0); } }
        public int ChunkSize { get { return BitConverter.ToInt32(Header[1], 0); } }
        public int Format { get { return BitConverter.ToInt32(Header[2], 0); } }
        public int Subchunk1ID { get { return BitConverter.ToInt32(Header[3], 0); } }
        public int Subchunk1Size { get { return BitConverter.ToInt32(Header[4], 0); } }
        public int AudioFormat { get { return BitConverter.ToInt16(Header[5], 0); } }
        public int NumChannels { get { return BitConverter.ToInt16(Header[6], 0); } }
        public int SampleRate { get { return BitConverter.ToInt32(Header[7], 0); } }
        public int ByteRate { get { return BitConverter.ToInt32(Header[8], 0); } }
        public int BlockAlign { get { return BitConverter.ToInt16(Header[9], 0); } }
        public int BitsPerSample { get { return BitConverter.ToInt16(Header[10], 0); } }
        public int Subchunk2ID { get { return BitConverter.ToInt32(Header[11], 0); } }
        public int Subchunk2Size { get { return BitConverter.ToInt32(Header[12], 0); } }

        public WaveFile(string path):this(new FileStream(path, FileMode.Open, FileAccess.Read)) { }

        public WaveFile(FileStream stream)
        {
            Header = new List<byte[]>();


            List<byte> tempData = new List<byte>();
            BinaryReader br = new BinaryReader(stream);
            Header.Add(br.ReadBytes(4)); //0 ChunkID 
            Header.Add(br.ReadBytes(4)); //1 ChunkSize
            Header.Add(br.ReadBytes(4)); //2 Format
            Header.Add(br.ReadBytes(4)); //3 Subchunk1ID
            Header.Add(br.ReadBytes(4)); //4 Subchunk1Size
            Header.Add(br.ReadBytes(2)); //5 AudioFormat
            Header.Add(br.ReadBytes(2)); //6 NumChannels
            Header.Add(br.ReadBytes(4)); //7 SampleRate
            Header.Add(br.ReadBytes(4)); //8 ByteRate
            Header.Add(br.ReadBytes(2)); //9 BlockAlign
            Header.Add(br.ReadBytes(2)); //10 BitsPerSample
            Header.Add(br.ReadBytes(4)); //11 Subchunk2ID
            Header.Add(br.ReadBytes(4)); //12 Subchunk2Size
            Header.Add(br.ReadBytes(1)); //Grace Byte


            while(br.BaseStream.Position != br.BaseStream.Length)
                tempData.Add(br.ReadByte());

            br.Close();
            stream.Close();


            Data = tempData.ToArray();
        }

        public void Extract(string path)
        {
            List<byte> data = new List<byte>();
            foreach(byte[] hdata in Header)
            {
                foreach(byte b in hdata)
                    data.Add(b);
            }

            foreach (byte b in Data)
                data.Add(b);


            File.WriteAllBytes(path, data.ToArray<byte>());
        }

    }
}
