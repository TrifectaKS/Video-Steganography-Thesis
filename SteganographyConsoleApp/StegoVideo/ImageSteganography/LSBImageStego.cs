using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Utilities;

namespace ImageSteganography
{
    public class LSBImageStego
    {
        private List<Bitmap> Frames { get; set; }

        private byte[] secretMessage;
        private NonRepeatingRandom nrr { get; set; }

        public LSBImageStego(int seed)
        {
            nrr = new NonRepeatingRandom(seed);
        }

        public LSBImageStego(IEnumerable<Bitmap> images, string key)
        {
            int numKey = 0;
            foreach (char c in key.ToCharArray())
            {
                numKey += c;
            }

            nrr = new NonRepeatingRandom(numKey);
            Frames = (List<Bitmap>)images;
        }

        public LSBImageStego(string k, string message, List<Bitmap> images) : this(images, k)
        {
            secretMessage = Encoding.ASCII.GetBytes(message);
        }

        public List<Bitmap> EmbedData()
        {
            //Bytes
            int length = secretMessage.Length;
            Console.WriteLine("Image Message Length: " + length);

            //Embed Metadata in first frame
            byte[] sms = BitConverter.GetBytes(length);
            EmbedInFrame(Frames[0], Bits.ToBits(sms), nrr.GetRandomSequence(0, (Frames[0].Width * Frames[0].Height) - 1, 4).ToList(), 0);

            //Embed secret data
            /*
             divide frames by bytes
             bytes per frames
            */

            //Bytes Per Frame

            int bpf = (int)Math.Ceiling((double)length / (double)(Frames.Count - 1));
            BinaryReader br = new BinaryReader(new MemoryStream(secretMessage));
            try
            {
                int byteCounter = 0;
                for (int i = 1; i < Frames.Count; i++)
                {
                    if (byteCounter + bpf >= length)
                        bpf = length - byteCounter;

                    EmbedInFrame(Frames[i], new Bits(br.ReadBytes(bpf)), nrr.GetRandomSequence(0, (Frames[0].Width * Frames[0].Height) - 1, bpf).ToList(), i);

                    byteCounter += bpf;
                }
            }
            catch (Exception e) { throw new Exception(e.Message); }
            finally
            {
                br.Close();
            }

            return Frames;
        }


        public void EmbedInFrame(Bitmap frame, Bits bits, List<int> sequence, int iteration)
        {
            //CHECK BITS AND SEQUENCE - Should be equal
            if (bits.Count != sequence.Count() * 8)
                throw new Exception("Number of bits (" + bits.Count + ") do not match the number of positions (" + sequence.Count() * 8 + ") on iteration " + iteration);

            //Calculate XY Points
            List<Point> points = new List<Point>();
            PosToXY posToXY = new PosToXY(frame.Width, frame.Height);
            foreach (int i in sequence)
            {
                points.Add(posToXY.ToXY(i));
            }


            int pos;
            foreach (Point p in points)
            {
                for (int i = 0; i < 3; i++) //3 channels
                {
                    if (i == 0) //R
                    {
                        foreach (int k in nrr.GetRandomSequence(0, 3, 2))
                        {
                            pos = k % 4;
                            bool b = bits.Pop();
                            frame.SetPixel(p.X, p.Y,
                                Color.FromArgb(
                                    BitMask.ChangeBit(frame.GetPixel(p.X, p.Y).R, pos, b),
                                    frame.GetPixel(p.X, p.Y).G,
                                    frame.GetPixel(p.X, p.Y).B));
                        }
                    }
                    else if (i == 1) //G
                    {
                        foreach (int k in nrr.GetRandomSequence(0, 3, 3))
                        {
                            pos = k % 4;

                            bool b = bits.Pop();

                            frame.SetPixel(p.X, p.Y,
                                Color.FromArgb(
                                    frame.GetPixel(p.X, p.Y).R,
                                    BitMask.ChangeBit(frame.GetPixel(p.X, p.Y).G, pos, b),
                                    frame.GetPixel(p.X, p.Y).B));
                        }
                    }
                    else if (i == 2) //B
                    {
                        foreach (int k in nrr.GetRandomSequence(0, 3, 3))
                        {
                            pos = k % 4;

                            bool b = bits.Pop();
                            frame.SetPixel(p.X, p.Y,
                                    Color.FromArgb(
                                        frame.GetPixel(p.X, p.Y).R,
                                        frame.GetPixel(p.X, p.Y).G,
                                        BitMask.ChangeBit(frame.GetPixel(p.X, p.Y).B, pos, b)));
                        }
                    }
                }

            }
        }

        public byte[] ExtractData()
        {
            //Get size
            Bits extractedBits = ExtractFromFrame(Frames[0], nrr.GetRandomSequence(0, (Frames[0].Width * Frames[0].Height) - 1, 4).ToList());

            int length = extractedBits.ToInt32();
            int bpf = (int)Math.Ceiling((double)length / (double)(Frames.Count - 1));

            BinaryWriter bw = new BinaryWriter(new MemoryStream());
            int byteCounter = 0;
            try
            {
                for (int i = 1; i < Frames.Count; i++)
                {
                    if (byteCounter + bpf >= length)
                        bpf = length - byteCounter;

                    extractedBits = ExtractFromFrame(Frames[i], nrr.GetRandomSequence(0, (Frames[i].Width * Frames[i].Height) - 1, bpf).ToList());

                    bw.Write(extractedBits.ToBytes());

                    byteCounter += bpf;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                bw.Close();
            }

            return StreamToBytes.StreamToBytesArray(bw.BaseStream);
        }

        private Bits ExtractFromFrame(Bitmap frame, List<int> sequence)
        {
            //Calculate XY Points
            List<Point> points = new List<Point>();
            PosToXY posToXY = new PosToXY(frame.Width, frame.Height);
            foreach (int i in sequence)
            {
                points.Add(posToXY.ToXY(i));
            }

            Bits bits = new Bits();
            int pos;
            foreach (Point p in points)
            {
                for (int i = 0; i < 3; i++) //3 channels
                {
                    if (i == 0) //R
                    {
                        foreach (int k in nrr.GetRandomSequence(0, 3, 2))
                        {
                            pos = k % 4;
                            var bit = BitMask.GetBit(frame.GetPixel(p.X, p.Y).R, pos);
                            bits.Push(bit);
                        }
                    }
                    else if (i == 1) //G
                    {

                        foreach (int k in nrr.GetRandomSequence(0, 3, 3))
                        {
                            pos = k % 4;
                            var bit = BitMask.GetBit(frame.GetPixel(p.X, p.Y).G, pos);
                            bits.Push(bit);
                        }
                    }
                    else if (i == 2) //B
                    {

                        foreach (int k in nrr.GetRandomSequence(0, 3, 3))
                        {
                            pos = k % 4;
                            var bit = BitMask.GetBit(frame.GetPixel(p.X, p.Y).B, pos);
                            bits.Push(bit);
                        }
                    }
                }
            }
            return bits;
        }
    }
}
