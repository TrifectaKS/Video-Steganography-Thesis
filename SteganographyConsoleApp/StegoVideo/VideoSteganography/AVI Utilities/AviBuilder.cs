using AviFile;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoSteganography
{
    public class AviBuilder
    {

        public string Path { get; private set; }
        public string AudioFile { get; private set; }
        private AviManager AviManager { get; set; }
        List<Bitmap> Frames { get; set; }
        private double FPS { get; set; }

        public AviBuilder(string path, List<Bitmap> frames, double fps)
        {
            Frames = frames;
            Path = path;
            AviManager = new AviManager(Path, false);
            FPS = fps;
        }

        public AviBuilder(string path, List<Bitmap> frames, string audioFile, double fps) : this(path, frames, fps)
        {
            AudioFile = audioFile;
        }
        
        public void CompileAVI()
        {
            VideoStream vstream = AviManager.AddVideoStream(false, FPS, Frames[0]);
            
            vstream.GetFrameOpen();
            for (int i = 1; i < Frames.Count; i++)
            {
                vstream.AddFrame(Frames[i]);
            }

            AviManager.AddAudioStream(AudioFile, 0);
            
            AviManager.Close();
            vstream.GetFrameClose();
        }
    }
}
