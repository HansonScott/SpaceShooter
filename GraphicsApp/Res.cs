#region using
using System;
using System.IO;
using System.Drawing;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;
using Buffer = Microsoft.DirectX.DirectSound.Buffer;
using System.Windows.Forms;
using System.Reflection;
using System.Media;
#endregion 

namespace GraphicsApp
{
    public class Res
    {
        public static bool USE_SOUNDS = false;
        public static string appPath = System.Environment.CurrentDirectory + Path.DirectorySeparatorChar;
        public static string ImagePath = appPath + "Images" + Path.DirectorySeparatorChar;
        public static string SoundPath = appPath + "Sounds" + Path.DirectorySeparatorChar;
        public static string AnimationPath = appPath + "Animations" + Path.DirectorySeparatorChar;

        #region data members
        private static Hashtable pics = new Hashtable();
        private static Hashtable sounds = new Hashtable();
        private static Hashtable animations = new Hashtable();
        #endregion

        #region static methods
        public static void setupRes()
        {
        }
        public static Bitmap getPic(String filename)
    {
        Bitmap result = (Bitmap)pics[filename];
        if (result == null) 
        {
            string fullFilePath = ImagePath + filename;
            if (System.IO.File.Exists(fullFilePath))
            {
                pics[filename] = new Bitmap(fullFilePath);
                result = (Bitmap)pics[filename];
            }
            else
            {
                string blank = ImagePath + "blank.gif";
                result = (Bitmap)pics[blank];
            }
        }
        return result;
    }

        public static GifImage getAnimation(string filename)
        {
            GifImage result = (GifImage)animations[filename];
            if (result == null)
            {
                string fullFilePath = AnimationPath + filename;
                if (System.IO.File.Exists(fullFilePath))
                {
                    animations[filename] = new GifImage(fullFilePath);
                    result = (GifImage)animations[filename];
                }
            }
            return result;
        }

        #region Sounds
        public static void playSound(String filename)
        {
            playSound(filename, false);
        }
        public static void playSound(String filename, bool loop)
        {
            if (USE_SOUNDS == false)
            {
                return;
            }

            if (filename == null) { return; }

            if (sounds[filename] == null)
            {
                SoundPlayer player = new SoundPlayer(SoundPath + filename);
                player.LoadAsync();
                sounds[filename] = player;
            }

            if (loop)
            {
                (sounds[filename] as SoundPlayer).PlayLooping();
            }
            else
            {
                (sounds[filename] as SoundPlayer).Play();
            }
        }
        public static void stopSound(string filename) 
        {
            if (sounds[filename] == null) { return; }

            (sounds[filename] as SoundPlayer).Stop();
        }
        #endregion
    #endregion

    #region private methods

    #endregion
    } // end class
} // end namespace