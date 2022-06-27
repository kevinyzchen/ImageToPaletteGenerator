using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using Kneedle;
using ML;
using Newtonsoft.Json;
using Path = System.IO.Path;

namespace ImageToPaletteGenerator
{
    public class FileProcessor : Reference
    {
        protected List<string> ProcessResultFilePaths { get; private set; } = new List<string>();
        protected Dictionary ProcessingParams;
        protected delegate void Process(string inputPath, string outputPath);

        protected int FilesProcessed;
        protected int TotalFiles;
        protected float Progress;

        protected void ProcessFolder(Process processFunction, string inputPath, string outputPath)
        {
            var dir = new Directory();
            var files = new List<string>();
            if (dir.Open(inputPath) == Error.Ok)
            {
                files = GetFilePaths(dir, inputPath);
                TotalFiles = files.Count;
            }

            foreach (var file in files) processFunction(file, outputPath);
        }

        public System.Collections.Generic.Dictionary<string, List<Color>> Preview()
        {
            var colorsFromFiles = new System.Collections.Generic.Dictionary<string, List<Color>>();
            foreach (var file in ProcessResultFilePaths)
            {
                var colors = ColorSpaceReader.LoadColorsFromFile(file);
                var gdColors = colors.Select(o => o.Color).ToList();
                colorsFromFiles.Add(Path.GetFileNameWithoutExtension(file), gdColors);
            }

            return colorsFromFiles;
        }

        public static List<string> GetFilePaths(Directory dir, string folderPath)
        {
            var files = new List<string>();
            dir.ListDirBegin();
            var fileName = dir.GetNext();
            while (fileName != string.Empty)
            {
                if (!dir.CurrentIsDir())
                {
                    var path = folderPath + fileName;
                    files.Add(path);
                }

                fileName = dir.GetNext();
            }

            return files;
        }

        protected string WritePaletteToDisk(ColorSpace colorPalette, string name, string path)
        {
            var file = new File();
            var savePath = path + "colors_" + name + ".json";
            file.Open(savePath, File.ModeFlags.Write);
            var output = JsonConvert.SerializeObject(colorPalette, Formatting.Indented);
            file.StoreLine(output);
            file.Close();
            return savePath;
        }

        protected KMeans GetKMeans(double[][] data, int minK, int maxK)
        {
            var kMeansFromK =
                new System.Collections.Generic.Dictionary<int, KMeans>();
            var x = new double[maxK - minK];
            var y = new double[maxK - minK];
            for (var i = 0; i < maxK - minK; i++)
            {
                var inputK = minK + i;
                var kMeans = new KMeans(inputK, data, "plusplus", 3, 0);
                kMeans.Cluster(3);
                kMeansFromK.Add(inputK, kMeans);
                GD.Print(inputK, "  ", kMeans.wcss);
                x[i] = inputK;
                y[i] = kMeans.wcss;
            }

            var k = KneedleAlgorithm.CalculateKneePoints(x, y, CurveDirection.Decreasing, Curvature.Counterclockwise);
            Debug.Assert(k != null, nameof(k) + " != null");
            var chosenKMeans = kMeansFromK[Mathf.FloorToInt((float)k)];
            return chosenKMeans;
        }
        
    }
}