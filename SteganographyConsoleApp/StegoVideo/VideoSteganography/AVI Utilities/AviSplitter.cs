using AviFile;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoSteganography
{
    public class AviSplitter
    {
        public string Path { get; private set; }
        public string TempAudioFile { get; private set; }
        private AviManager AviManager { get; set; }
        
        public AviSplitter(string path)
        {
            AviManager = new AviManager(path, true);
        }

        public AviSplitter(string path, string tempAudioPath) : this(path)
        {
            TempAudioFile = tempAudioPath;
        }

        public double GetFPS()
        {
            VideoStream stream = AviManager.GetVideoStream();
            double fps = stream.FrameRate;
            stream.GetFrameClose();
            stream.Close();
            return fps;
        }

        public int GetStuff()
        {
            VideoStream vs = AviManager.GetVideoStream();
            int res = vs.StreamInfo.dwRate;
            vs.GetFrameClose();
            vs.Close();
            return res;
        }

        public IList<Bitmap> GetFrames()
        {
            IList<Bitmap> frames = new List<Bitmap>();
            
            VideoStream stream = AviManager.GetVideoStream();
            stream.GetFrameOpen();
            
            for(int c = 0; c < stream.CountFrames; c++)
            {
                frames.Add(stream.GetBitmap(c));
            }

            stream.GetFrameClose();
            return frames;
        }

        public string GetWaveFile()
        {
            AudioStream stream = AviManager.GetWaveStream();
            stream.ExportStream(TempAudioFile);
            stream.Close();
            
            return TempAudioFile;
        }

        public void GetWaveFile(string path)
        {
            AudioStream stream = AviManager.GetWaveStream();
            stream.ExportStream(path);
            stream.Close();
        }
    }
}
