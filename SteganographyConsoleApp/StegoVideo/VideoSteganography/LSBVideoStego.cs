using AudioSteganography;
using ImageSteganography;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VideoSteganography.Cipher;

namespace VideoSteganography
{
    public class LSBVideoStego
    {
        /*
         Get: Key, Secret Message, Video, DistributionRatio

         Encrypt Message With Key

         Load Video

         Split Video

         Split Secret Message

         Image Steganography (key, secret msg, frames)

         Audio Steganography (key, secret msg, audiopath)

         Rebuild Video

         */

        public static void Embed(string key, string message, string videoPath, float distributionRatio, string extractPath)
        {
            //Encrypt Secret Data
            string secretData = CipherUtility.Encrypt<AesManaged>(message, key, "abcdefghij");

            //Load Video
            AviSplitter aviSplitter = new AviSplitter(videoPath, Path.GetDirectoryName(videoPath) + "\\tempaudio.wav");

            //Split Video
            List<Bitmap> frames = (List<Bitmap>)aviSplitter.GetFrames();
            string audio = aviSplitter.GetWaveFile();
            
            //Split Secret Message
            string audioSecretData, framesSecretData;
            int charPos = Convert.ToInt32(Math.Round(secretData.Length* distributionRatio));
            framesSecretData = secretData.Substring(0, charPos);
            audioSecretData = secretData.Substring(charPos);

            //Image Steganography
            LSBImageStego imagestego = new LSBImageStego(key, framesSecretData, frames);
            List<Bitmap> stegoframes = imagestego.EmbedData();

            //Audio Steganography
            LSBAudioStego audiostego = new LSBAudioStego(key, audioSecretData, new WaveFile(audio));
            WaveFile wf = audiostego.Embed();
            wf.Extract(extractPath + "\\" + Path.GetFileNameWithoutExtension(videoPath) + "_stego" + distributionRatio + ".wav");
            double fps = stegoframes.Count / 10;
            //Rebuild AVI
            AviBuilder aviBuilder = new AviBuilder(extractPath + "\\"+ Path.GetFileNameWithoutExtension(videoPath) + "_stego"+distributionRatio+".avi", stegoframes, extractPath + "\\"+ Path.GetFileNameWithoutExtension(videoPath) + "_stego" + distributionRatio + ".wav", fps);
            aviBuilder.CompileAVI();
        }

        public static void Embed(string key, string framesmessage, string audiomessage, string videoPath, string extractPath, int index)
        {
            //Load Video
            AviSplitter aviSplitter = new AviSplitter(videoPath, Path.GetDirectoryName(videoPath) + "\\tempaudio.wav");

            //Split Video
            List<Bitmap> frames = (List<Bitmap>)aviSplitter.GetFrames();
            string audio = aviSplitter.GetWaveFile();

            //Image Steganography
            LSBImageStego imagestego = new LSBImageStego(key, framesmessage, frames);
            List<Bitmap> stegoframes = imagestego.EmbedData();

            //Audio Steganography
            LSBAudioStego audiostego = new LSBAudioStego(key, audiomessage, new WaveFile(audio));
            WaveFile wf = audiostego.Embed();
            wf.Extract(extractPath + "\\" + Path.GetFileNameWithoutExtension(videoPath) + "_stego"+index+".wav");

            //Rebuild AVI
            AviBuilder aviBuilder = new AviBuilder(extractPath + "\\" + Path.GetFileNameWithoutExtension(videoPath) + "_stego" + index + ".avi", stegoframes, extractPath + "\\" + Path.GetFileNameWithoutExtension(videoPath) + "_stego" + index + ".wav", aviSplitter.GetFPS());
            aviBuilder.CompileAVI();
        }


        public static void EmbedFrames(string key, string message, string videoPath, string extractPath)
        {
            //Encrypt Secret Data
            string secretData = CipherUtility.Encrypt<AesManaged>(message, key, "abcdefghij");

            //Load Video
            AviSplitter aviSplitter = new AviSplitter(videoPath, Path.GetDirectoryName(videoPath) + "\\"+ Path.GetFileNameWithoutExtension(videoPath) + "_stego.wav");

            //Split Video
            List<Bitmap> frames = (List<Bitmap>)aviSplitter.GetFrames();
            
            //Image Steganography
            LSBImageStego imagestego = new LSBImageStego(key, secretData, frames);
            List<Bitmap> stegoframes = imagestego.EmbedData();
            
            //Rebuild AVI
            AviBuilder aviBuilder = new AviBuilder(Path.GetDirectoryName(extractPath) + "\\"+ Path.GetFileNameWithoutExtension(videoPath) + "_stego.avi", stegoframes, aviSplitter.GetFPS());
            aviBuilder.CompileAVI();
        }

        /*
            Load Video
            
            Split Video

            Extract from Frames

            Extract from audio

            Combine extracted message
        */

        public static void Extract(string key, string videoPath)
        {
            AviSplitter aviSplitter = new AviSplitter(videoPath, Path.GetDirectoryName(videoPath) + "\\tempaudio1.wav");

            List<Bitmap> frames = (List<Bitmap>)aviSplitter.GetFrames();
            string audio = aviSplitter.GetWaveFile();

            LSBImageStego imagestego = new LSBImageStego(frames, key);
            byte[] imageData = imagestego.ExtractData();

            LSBAudioStego audiostego = new LSBAudioStego(new WaveFile(audio), key);
            byte[] audioData = audiostego.Extract();

            string imageMsg = Encoding.ASCII.GetString(imageData);
            string audioMsg = Encoding.ASCII.GetString(audioData);
            
            Console.WriteLine(imageMsg);
            Console.WriteLine(audioMsg);
        }

        public static void ExtractSeperate(string key, string videoPath)
        {
            AviSplitter aviSplitter = new AviSplitter(videoPath, Path.GetDirectoryName(videoPath) + "\\tempaudio1.wav");

            List<Bitmap> frames = (List<Bitmap>)aviSplitter.GetFrames();
            string audio = aviSplitter.GetWaveFile();

            LSBImageStego imagestego = new LSBImageStego(frames, key);
            byte[] imageData = imagestego.ExtractData();

            LSBAudioStego audiostego = new LSBAudioStego(new WaveFile(audio), key);
            byte[] audioData = audiostego.Extract();

            string imageMsg = Encoding.ASCII.GetString(imageData);
            string audioMsg = Encoding.ASCII.GetString(audioData);
           
            Console.WriteLine(imageMsg);
            Console.WriteLine(audioMsg);
        }

        public static void ExtractFrames(string key, string videoPath)
        {
            AviSplitter aviSplitter = new AviSplitter(videoPath, Path.GetDirectoryName(videoPath) + "\\tempaudio1.wav");

            List<Bitmap> frames = (List<Bitmap>)aviSplitter.GetFrames();

            LSBImageStego imagestego = new LSBImageStego(frames, key);
            byte[] imageData = imagestego.ExtractData();
            
            string imageMsg = Encoding.ASCII.GetString(imageData);
            
            imageMsg = CipherUtility.Decrypt<AesManaged>(imageMsg, key, "abcdefghij");

            Console.WriteLine(imageMsg);
        }
    }
}
