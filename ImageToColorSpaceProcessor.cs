using System;
using System.Collections.Generic;
using System.Diagnostics;
using PercetualColors;
using Path = System.IO.Path;
using Godot;
using Godot.Collections;
using ML;

namespace ImageToPaletteGenerator
{
    public class ImageToColorSpaceProcessor : FileProcessor, IProcessor
    {
        
        private void ProcessImage(string path, string savePath)
        {
            if (path.Extension() != "png" && path.Extension() != "jpeg" && path.Extension() != "jpg") return;
            var palette = ImageToPalette(path);
            var colorPalette = new ColorSpace(palette, new List<string> { "warm", "fun" }, "random name");
            var result = WritePaletteToDisk(colorPalette, Path.GetFileNameWithoutExtension(path), savePath);
            ProcessResultFilePaths.Add(result);
            FilesProcessed++;
        }

        private List<UberColor> ImageToPalette(string path)
        {
            var pixels = ImageUtility.GetImagePixels(path);
            List<UberColor> palette = new List<UberColor>();
            if ((string)ProcessingParams["method"] == "KMEANS")
            {
                palette = UseKMeans(pixels);
            }
            else if ((string)ProcessingParams["method"] == "DBSCAN")
            {
                palette = UseDBScan(pixels);
            }
            return palette;
        }

        private List<UberColor> UseKMeans(UberColor[,] pixelColors)
        {

            //Change pixelColors to appropriate data type
            var data = new double[pixelColors.Length][];
            var counter = 0;
            foreach (var i in pixelColors)
            {
                data[counter] = new[] { i.Lab.L, i.Lab.a, i.Lab.b };
                counter++;
            }
            //
            
            var minK = (int)ProcessingParams["min_k"];
            var maxK = (int)ProcessingParams["max_k"];
            var kMeans = GetKMeans(data, minK, maxK);
            var palette = new List<UberColor>();
            foreach (var i in kMeans.means)
            {
                var newLab = new Lab((float)i[0], (float)i[1], (float)i[2]);
                var newColor = new UberColor(newLab);
                palette.Add(newColor);
            }
            return palette;
        }

        
        private List<UberColor> UseDBScan(UberColor[,] pixelColors)
        {
            //Change pixels to points
            List<DBPoint> points = new List<DBPoint>();
            foreach (var color in pixelColors) {
                int L = Mathf.FloorToInt( (float)color.Lab.L * 1000);
                int a = Mathf.FloorToInt( (float)color.Lab.a * 1000);
                int b = Mathf.FloorToInt( (float)color.Lab.b * 1000);
                DBPoint newPoint = new DBPoint(L,a,b);
                points.Add(newPoint);
            }
            double eps = (float)ProcessingParams["eps"];
            int minPts = (int)ProcessingParams["min_pts"];
            var clusters = DBScan.GetClusters(points, eps, minPts);
            GD.PrintErr(clusters.Count, "count");
            List<UberColor> representativeColors = new List<UberColor>();
            foreach (var pointsList in clusters)
            {
                Random rng = new Random();
                var num = rng.Next(0, pointsList.Count);
                DBPoint chosenPoint = pointsList[num];
                representativeColors.Add(new UberColor(new Lab(chosenPoint.X/1000.0, chosenPoint.Y/1000.0,chosenPoint.Z/1000.0)));
            }
            return representativeColors;
            //need to choose a different color from clusters as representative clusters..
        }

        public List<string> Process(string imagePath, string outputPath, Dictionary args = null)
        {
            ProcessingParams = args;
            ProcessResultFilePaths.Clear();
            FilesProcessed = 0;
            ProcessFolder(ProcessImage, imagePath, outputPath);
            return ProcessResultFilePaths;
        }

        public List<string> Process(List<string> filePaths, string outputPath, Dictionary args = null)
        {
            ProcessingParams = args;
            ProcessResultFilePaths.Clear();
            FilesProcessed = 0;
            foreach (var file in filePaths) ProcessImage(file, outputPath);
            return ProcessResultFilePaths;
        }
    }
}