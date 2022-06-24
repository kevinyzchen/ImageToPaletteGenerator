using System.Collections.Generic;
using Godot;
using PercetualColorSystem;

namespace ImageToPaletteGenerator
{
    public static class ImageUtility
    {
        public static double[][] ColorToData(List<UberColor> pixels)
        {
            var data = new double[pixels.Count][];
            var counter = 0;
            foreach (var i in pixels)
            {
                data[counter] = new[] { i.Lab.L, i.Lab.a, i.Lab.b };
                counter++;
            }

            return data;
        }

        public static UberColor[,] GetImagePixels(string imagePath)
        {
            var imageTexture = new ImageTexture();
            var image = new Image();
            image.Load(imagePath);
            GD.Print(imagePath);
            imageTexture.CreateFromImage(image);
            return GetImagePixels(imageTexture);
        }

        public static List<List<UberColor>> GetImagePalette(List<UberColor> possibleColors)
        {
            return new List<List<UberColor>>();
        }

        public static double[][] ColorToData(UberColor[,] pixels)
        {
            var data = new double[pixels.Length][];
            var counter = 0;
            foreach (var i in pixels)
            {
                data[counter] = new[] { i.Lab.L, i.Lab.a, i.Lab.b };
                counter++;
            }

            return data;
        }

        public static List<Vector3> GetColorPoints(UberColor[,] pixels)
        {
            var colorPoints = new List<Vector3>();
            foreach (var color in pixels)
            {
                var pos = new Vector3((float)color.Lab.L, (float)color.Lab.a, (float)color.Lab.b);
                colorPoints.Add(pos);
            }

            return colorPoints;
        }

        public static UberColor[,] GetImagePixels(ImageTexture image)
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