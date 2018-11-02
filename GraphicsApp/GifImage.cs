using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace GraphicsApp
{
    public class GifImage
    {
        #region Declarations
        private Image gifImage;
        private FrameDimension dimension;
        private int frameCount;
        private int currentFrame = -1;
        private bool reverse;
        private int step = 1;
        #endregion

        #region Constructor
        public GifImage(string path)
        {
            gifImage = Image.FromFile(path); //initialize
            dimension = new FrameDimension(gifImage.FrameDimensionsList[0]); //gets the GUID
            frameCount = gifImage.GetFrameCount(dimension); //total frames in the animation
        }
        #endregion

        #region Properties
        public int MaxFrames
        {
            get { return frameCount; }
        }
        public int CurrentFrame
        {
            get { return currentFrame; }
            set { currentFrame = value; }
        }
        public Image Image
        {
            get { return gifImage; }
        }
        #endregion

        #region public methods
        public bool ReverseAtEnd //whether the gif should play backwards when it reaches the end
        {
            get { return reverse; }
            set { reverse = value; }
        }
        public Image GetNextFrame()
        {
            currentFrame += step;          //if the animation reaches a boundary...
            if (currentFrame >= frameCount || currentFrame < 1)
            {
                if (reverse)
                {
                    step *= -1; //...reverse the count
                    currentFrame += step; //apply it
                }
                else
                    currentFrame = 0; //...or start over
            }
            return GetFrame(currentFrame);
        }
        public Image GetFrame(int index)
        {
            gifImage.SelectActiveFrame(dimension, index); //find the frame
            return (Image)gifImage.Clone(); //return a copy of it
        }
        #endregion
    }
}
