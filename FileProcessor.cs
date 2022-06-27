using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using ImageToPaletteGenerator;
using Kneedle;
using ML;
using Newtonsoft.Json;
using PercetualColors;
using Path = System.IO.Path;
public class FileProcessor : Reference
    {
        protected List<string> ResultFilePaths { get; private set; } = new List<string>();
        protected Dictionary processingParams;

        protected delegate void Process(string inputPath, string outputPath);
        protected int FilesProcessed;
        protected int TotalFiles;
        protected float Progress;
        
        protected void ProcessFolder( Process processFunction, String inputPath, string outputPath) {
            Directory dir = new Directory();
            List<string> files = new List<string>();
            if (dir.Open(inputPath) == Error.Ok)
            {
                files = GetFilePaths(dir, inputPath);
                TotalFiles = files.Count;
            }
            foreach (var file in files)
            {
                processFunction(file, outputPath);
            }
        }
        public System.Collections.Generic.Dictionary<string, List<Color>> Preview()
        {
            System.Collections.Generic.Dictionary<string, List<Color>> colorsFromFiles = new System.Collections.Generic.Dictionary<string, List<Color>>();
            foreach (var file in ResultFilePaths)
            {
                List<UberColor> colors = ColorSpaceReader.LoadColorsFromFile(file);
                List<Color> gdColors = colors.Select(o => o.Color).ToList();
                colorsFromFiles.Add(Path.GetFileNameWithoutExtension(file), gdColors);
            }
            return colorsFromFiles;
        }

        public static List<String> GetFilePaths(Directory dir, String folderPath)
        {
            List<string> files = new List<string>();
            dir.ListDirBegin();
            var fileName = dir.GetNext();
            while (fileName != String.Empty) {
                if (!dir.CurrentIsDir()) {
                    var path = folderPath + fileName;
                    files.Add(path);
                }
                fileName = dir.GetNext();
            }
            return files;
        }
        
        
        protected string WritePaletteToDisk(ColorSpace colorPalette, String name, String path) {
            var file = new File();
            var savePath = path + "colors_"+ name + ".json";
            file.Open(savePath, File.ModeFlags.Write);
            string output = JsonConvert.SerializeObject(colorPalette, Formatting.Indented);
            file.StoreLine(output);
            file.Close();
            return savePath;
        }

        protected KMeans GetKMeans(double[][] data, int minK, int maxK)
        {
            System.Collections.Generic.Dictionary<int,KMeans> kMeansFromK = 
                new System.Collections.Generic.Dictionary<int, KMeans>();
            double[] x = new double[maxK - minK];
            double[] y = new double[maxK - minK];
            for (int i = 0; i < maxK - minK; i++)
            {
                var inputK = minK + i; 
                var kMeans = new KMeans(inputK, data, "plusplus", 3, 0);
                kMeans.Cluster(3);
                kMeansFromK.Add(inputK,kMeans);
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
