using Godot;
using PercetualColors;

namespace ImageToPaletteGenerator
{
    public static class ImageUtility
    {
        public static UberColor[,] GetImagePixels(string imagePath)
        {
            var imageTexture = new ImageTexture();
            var image = new Image();
            image.Load(imagePath);
            GD.Print(imagePath);
            imageTexture.CreateFromImage(image);
            return GetImagePixels(imageTexture);
        }

        private static UberColor[,] GetImagePixels(ImageTexture image)
        {
            var height = image.GetHeight();
            var width = image.GetWidth();
            var data = image.GetData();
            data.Lock();
            var pixelArray = new UberColor[width, height];
            for (var i = 0; i < width; i++)
            for (var j = 0; j < height; j++)
                pixelArray[i, j] = new UberColor(data.GetPixel(i, j));

            data.Unlock();
            return pixelArray;
        }
    }
}