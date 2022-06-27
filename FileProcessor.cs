using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Kneedle;
using MediaToolkit.Model;
using MediaToolkit.Options;
using ML;
using Newtonsoft.Json;
using PercetualColors;

namespace ImageToPaletteGenerator
{
    public class FileProcessor : Godot.Reference, IProcessor
    {
        private List<string> ProcessResultFilePaths { get; set; } = new List<string>();
        private Godot.Collections.Dictionary _processingParams;
        private int _filesProcessed;
        private int _totalFiles;

        private static List<string> FindFilePaths(string inputPath, List<string> allFilePaths)
        {
            if (Directory.Exists(inputPath))
            {
                var filePaths = Directory.GetFiles(inputPath);
                allFilePaths.AddRange(filePaths);

                var dirPaths = Directory.GetDirectories(inputPath);
                foreach (var path in dirPaths)
                {
                    FindFilePaths(path, allFilePaths);
                }
            }
            else if(File.Exists(inputPath))
            {
                allFilePaths.Add(inputPath);
            }
            return allFilePaths;
        }
        
        public List<string> Process(string inputPath, string outputPath, Godot.Collections.Dictionary args = null)
        {
            _processingParams = args;
            var filePaths = FindFilePaths(inputPath, new List<string>());
            _totalFiles = filePaths.Count;
            _filesProcessed = 0;
            foreach (var path in filePaths)
            {
                var extension = Path.GetExtension(path);
                string result = null;
                if (extension == ".jpg" || extension == ".png" || extension == ".jpeg")
                {
                    result = ProcessImage(path, outputPath);
                }
                else if (extension == ".mp4" || extension == ".flv" || extension == ".mpeg" || extension == ".mpg")
                {
                    result = ProcessVideo(path, outputPath);
                }
                if (result != null) ProcessResultFilePaths.Add(result);
            }
            return ProcessResultFilePaths;
        }

        public Dictionary<string, List<Godot.Color>> Preview()
        {
            var colorsFromFiles = new Dictionary<string, List<Godot.Color>>();
            foreach (var processResultFilePath in ProcessResultFilePaths)
            {
                var colors = ColorSpaceReader.LoadColorsFromFile(processResultFilePath);
                var gdColors = colors.Select(o => o.Color).ToList();
                colorsFromFiles.Add(Path.GetFileNameWithoutExtension(processResultFilePath), gdColors);
            }
            return colorsFromFiles;
        }
            
        private string WritePaletteToDisk(ColorSpace colorPalette, string name, string path)
        {
            var savePath = path + "colorspace_" + name + ".json";
            var output = JsonConvert.SerializeObject(colorPalette, Formatting.Indented);
            File.WriteAllText(savePath, output);
            return savePath;
        }

        private string ProcessImage(string path, string savePath)
        {
            var palette = ImageToPalette(path);
            var colorPalette = new ColorSpace(palette, new List<string>(),
                (string)_processingParams["name"]);
            var result = WritePaletteToDisk(colorPalette, Path.GetFileNameWithoutExtension(path), savePath);
            _filesProcessed++;
            return result;
        }

        private List<UberColor> ImageToPalette(string path)
        {
            var pixels = ImageUtility.GetImagePixels(path);
            var palette = new List<UberColor>();
            if ((string)_processingParams["method"] == "KMEANS")
                palette = UseKMeans(pixels);
            else if ((string)_processingParams["method"] == "DBSCAN") palette = UseDBScan(pixels);
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

            var minK = (int)_processingParams["min_k"];
            var maxK = (int)_processingParams["max_k"];
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

        private KMeans GetKMeans(double[][] data, int minK, int maxK)
        {
            var kMeansFromK =
                new Dictionary<int, KMeans>();
            var x = new double[maxK - minK];
            var y = new double[maxK - minK];
            for (var i = 0; i < maxK - minK; i++)
            {
                var inputK = minK + i;
                var kMeans = new KMeans(inputK, data, "plusplus", 3, 0);
                kMeans.Cluster(3);
                kMeansFromK.Add(inputK, kMeans);
                x[i] = inputK;
                y[i] = kMeans.wcss;
            }

            var k = KneedleAlgorithm.CalculateKneePoints(x, y, CurveDirection.Decreasing, Curvature.Counterclockwise);
            Debug.Assert(k != null, nameof(k) + " != null");
            var chosenKMeans = kMeansFromK[(int)Math.Floor((float)k)];
            return chosenKMeans;
        }

        private List<UberColor> UseDBScan(UberColor[,] pixelColors)
        {
            //Change pixels to points
            var points = new List<DBPoint>();
            foreach (var color in pixelColors)
            {
                var L = (int)Math.Floor((float)color.Lab.L * 1000);
                var a = (int)Math.Floor((float)color.Lab.a * 1000);
                var b = (int)Math.Floor((float)color.Lab.b * 1000);
                var newPoint = new DBPoint(L, a, b);
                points.Add(newPoint);
            }

            double eps = (float)_processingParams["eps"];
            var minPts = (int)_processingParams["min_pts"];
            var clusters = DBScan.GetClusters(points, eps, minPts);
            var representativeColors = new List<UberColor>();
            foreach (var pointsList in clusters)
            {
                var rng = new Random();
                var num = rng.Next(0, pointsList.Count);
                var chosenPoint = pointsList[num];
                representativeColors.Add(new UberColor(new Lab(chosenPoint.X / 1000.0, chosenPoint.Y / 1000.0,
                    chosenPoint.Z / 1000.0)));
            }

            return representativeColors;
            //need to choose a different color from clusters as representative clusters..
        }
        
        private string ProcessVideo(string path, string savePath)
        {
            var tmpImagePaths = VideoToImages(path);
            List<string> tmpPalettePaths = new List<string>();
            foreach (var file in tmpImagePaths)
            {
                var result = ProcessImage(file, savePath);
                tmpPalettePaths.Add(result);
            }
            var colors = ColorSpaceReader.LoadColorsFromFiles(tmpPalettePaths);
            //Filter colors for distance
            colors.Shuffle();
            var remainingColors = new Queue<UberColor>(colors);
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
                    if (!(dist < (float)_processingParams["threshold"])) continue;
                    success = false;
                    break;
                }
                if (success) selectedColors.Add(checkingColor);
            }

            var videoPalette = new ColorSpace(selectedColors, new List<string>(),
                (string)_processingParams["name"]);
            var videoPalettePath = WritePaletteToDisk(videoPalette, (string)_processingParams["name"], savePath);
            tmpPalettePaths.ForEach(File.Delete);
            tmpImagePaths.ForEach(File.Delete);
            _filesProcessed++;
            return videoPalettePath;
        }

        private List<string> VideoToImages(string path)
        {
            var tmpImagePaths = new List<string>();
            var inputFile = new MediaFile { Filename = path };
            var engine = new MediaToolkit.Engine();
            engine.GetMetadata(inputFile);
            var interval = (float)_processingParams["interval"];
            var totalFrames = (int)Math.Floor((float)(inputFile.Metadata.Duration.TotalSeconds / interval));
            for (var i = 0; i < totalFrames; i++)
            {
                var outputFilePath = path + Path.GetFileNameWithoutExtension(path) + i + ".png";
                var outputFile = new MediaFile { Filename = outputFilePath };
                double time = interval * i + 3;
                var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(time) };
                engine.GetThumbnail(inputFile, outputFile, options);
                tmpImagePaths.Add(outputFilePath);
            }
            return tmpImagePaths;
        }

        public float GetProgress()
        {
            return _filesProcessed / (float)_totalFiles;
        }


    }
}