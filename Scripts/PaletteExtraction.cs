using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Kneedle;
using PercetualColors;

namespace ImageToPaletteGenerator
{
    /// <summary>
    /// Helper class with functions for extracting palettes out of images
    /// </summary>
    public static class PaletteExtraction
    {
        public struct ExtractionArgs
        {
            
            public KmeansArgs KmeansArgs;
            // How close colors can be to each other in one color space before they're discarded
            public float Threshold; 
            // Highest resolution possible for an analyzed image. If the input image resolution is higher, it will be down sampled to the max resolution while maintain it's aspect ratio.
            public int MaxRes; 
            // The length of time between taking a thumbnail to analyze when extract a palette fro ma video
            public float Interval;
        }

        private static UberColor[,] GetImageColors(Image image, out Bitmap thumbBmp, int maxRes = 128)
        {
            int thumbWidth;
            int thumbHeight;

            if (image.Height < maxRes && image.Width < maxRes)
            {
                thumbBmp = new Bitmap(image);
                thumbWidth = thumbBmp.Width;
                thumbHeight = thumbBmp.Height;
            }
            else
            {
                var aspectRatio = image.Width / (float)image.Height;
                var imageSize = image.Height > image.Width
                    ? new Vector2(maxRes, maxRes / aspectRatio)
                    : new Vector2(maxRes / aspectRatio, maxRes);
                thumbWidth = (int)imageSize.X;
                thumbHeight = (int)imageSize.Y;
                thumbBmp =
                    new Bitmap(image.GetThumbnailImage(thumbWidth
                        , thumbHeight, ThumbnailCallback, IntPtr.Zero));
            }
            
            var uberColors = new UberColor[thumbWidth, thumbHeight];

            for (var i = 0; i < thumbWidth; i++)
            for (var j = 0; j < thumbHeight; j++)
            {
                var col = thumbBmp.GetPixel(i, j);
                uberColors[i, j] = new UberColor(new Godot.Color(Godot.Color.Color8(col.R, col.G, col.B)));
            }

            return uberColors;
        }

        private static bool ThumbnailCallback()
        {
            return false;
        }

        public static List<UberColor> ImageToPalette(string path, ExtractionArgs args, out Bitmap thumbnail )
        {
            var image = Image.FromFile(path);
            var pixels = GetImageColors(image, out Bitmap thumbBmp, args.MaxRes);
            var palette = new List<UberColor>();
            palette = GetPaletteFromColorData(pixels, args);
            thumbnail = thumbBmp;
            return palette;
        }

        private static List<UberColor> GetPaletteFromColorData(UberColor[,] pixelColors, ExtractionArgs args)
        {
            //Change pixelColors to appropriate data type
            var kmeansRNG = new Random();
            var data = new double[pixelColors.Length][];
            var counter = 0;
            foreach (var i in pixelColors)
            {
                data[counter] = new[] { i.Lab.L, i.Lab.a, i.Lab.b };
                counter++;
            }

            //Iterate through K values
            var kMeansFromK =
                new Dictionary<int, KMeans>();
            var x = new double[args.KmeansArgs.MaxK - args.KmeansArgs.MinK];
            var y = new double[args.KmeansArgs.MaxK - args.KmeansArgs.MinK];
            for (var i = 0; i < args.KmeansArgs.MaxK - args.KmeansArgs.MinK; i++)
            {
                var seed = kmeansRNG.Next();
                var inputK = args.KmeansArgs.MinK + i;
                var kMeans = new KMeans(inputK, data, args.KmeansArgs.Trials, seed);
                kMeans.Cluster(3);
                kMeansFromK.Add(inputK, kMeans);
                x[i] = inputK;
                y[i] = kMeans.Wcss;
            }

            //Use knee method to find the best K value
            var k = KneedleAlgorithm.CalculateKneePoints(x, y, CurveDirection.Decreasing, Curvature.Counterclockwise);
            Debug.Assert(k != null, nameof(k) + " != null");
            var chosenKMeans = kMeansFromK[(int)Math.Floor((float)k)];

            return chosenKMeans.Means.Select(i => new Lab((float)i[0], (float)i[1], (float)i[2]))
                .Select(newLab => new UberColor(newLab)).ToList();
        }

        public static List<UberColor> FilterColorListForDistance(List<UberColor> colors, float threshold)
        {
            var shuffled = new List<UberColor>(colors);
            shuffled.Shuffle();
            var remainingColors = new Queue<UberColor>(shuffled);
            var selectedColors = new List<UberColor>();
            while (remainingColors.Count > 0)
            {
                var checkingColor = remainingColors.Dequeue();
                if (remainingColors.Count == 1)
                {
                    selectedColors.Add(checkingColor);
                    break;
                }

                var compareColors = new List<UberColor>(remainingColors);
                compareColors.Remove(checkingColor);
                var success = true;
                foreach (var compareColor in compareColors)
                {
                    var dist = (float)checkingColor.Hsl.DistanceTo(compareColor.Hsl);
                    if (dist < threshold)
                    {
                        success = false;
                        break;
                    }
                }

                if (success) selectedColors.Add(checkingColor);
            }

            return selectedColors;
        }

        public static void Shuffle<T>(this IList<T> ts)
        {
            var rng = new Random();
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = rng.Next(i, count);
                (ts[i], ts[r]) = (ts[r], ts[i]);
            }
        }
    }
}