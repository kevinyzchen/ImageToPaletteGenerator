using System;
using System.Drawing;
using System.Numerics;
using PercetualColors;
using Image = System.Drawing.Image;

namespace ImageToPaletteGenerator
{
    /// <summary>
    /// Helper class with functions to deal with writing and reading images
    /// </summary>
    public static class ImageIO
    {
        public static UberColor[,] GetImageColors (Image image, int maxRes = 128)
        {

            int thumbWidth;
            int thumbHeight;
            Bitmap thumbBmp;
            
            if (image.Height < maxRes && image.Width < maxRes)
            {
                thumbBmp = new Bitmap(image);
                thumbWidth = thumbBmp.Width;
                thumbHeight = thumbBmp.Height;
            }
            else
            {
                float aspectRatio = image.Width / (float)image.Height;
                Vector2 imageSize = image.Height > image.Width ? new Vector2(maxRes, maxRes / aspectRatio) : new Vector2(maxRes / aspectRatio, maxRes);
                thumbWidth = (int)imageSize.X;
                thumbHeight = (int)imageSize.Y;
                thumbBmp = 
                    new Bitmap(image.GetThumbnailImage(thumbWidth
                        , thumbHeight, ThumbnailCallback, IntPtr.Zero));
            }

            UberColor[,] uberColors = new UberColor[thumbWidth,thumbHeight];

            for (int i = 0; i < thumbWidth; i++)
            {
                for (int j = 0; j < thumbHeight; j++)
                {
                    Color col = thumbBmp.GetPixel(i, j);
                    uberColors[i,j] = new UberColor(new Godot.Color(Godot.Color.Color8(col.R, col.G,col.B)));
                }                
            }
            return uberColors;
        }

        private static bool ThumbnailCallback() { return false; }
        
    }
}