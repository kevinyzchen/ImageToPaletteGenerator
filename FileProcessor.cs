using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Kneedle;
using MediaToolkit.Model;
using MediaToolkit.Options;
using PercetualColors;

namespace ImageToPaletteGenerator
{
    public class FileProcessor 
    {
        private List<string> ProcessResultFilePaths { get; set; } = new List<string>();
        private KmeansArgs _processingParams;
        private int _filesProcessed;
        private int _totalFiles;
        private float _interval; //Snapshot every x seconds if the file is a video
        private float _threshold; //How close colors can be to each other in one color space before they're discarded
        
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
        
        public List<string> Process(string inputPath, string outputPath, KmeansArgs args, float interval = .5f, float threshold = .1f)
        {
            ProcessResultFilePaths.Clear();
            _interval = interval;
            _threshold = threshold;
            _processingParams = args;
            var filePaths = FindFilePaths(inputPath, new List<string>());
            _totalFiles = filePaths.Count;
            _filesProcessed = 0;
            foreach (var path in filePaths)
            {
                var extension = Path.GetExtension(path);
                string result = null;
                if (ExtensionLists.ImageExtensions.Contains(extension)) result = ProcessImage(path, outputPath);
                else if (ExtensionLists.VideoExtensions.Contains(extension)) result = ProcessVideo(path, outputPath);
                if (result != null) ProcessResultFilePaths.Add(result);
            }
            return ProcessResultFilePaths;
        }

        private string ProcessImage(string path, string savePath)
        {
            var palette = ImageToPalette(path);
            var selectedColors = FilterColorListForDistance(palette);
            var colorPalette = new ColorSpace(selectedColors);
            var result = ColorSpaceIO.WriteColorSpaceToDisk(colorPalette, Path.GetFileNameWithoutExtension(path), savePath);
            _filesProcessed++;
            return result;
        }

        private List<UberColor> ImageToPalette(string path)
        {
            var pixels = ImageIO.GetImagePixels(path);
            var palette = new List<UberColor>();
            palette = UseKMeans(pixels);
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
            var kMeans = GetKMeans(data, _processingParams.minK, _processingParams.maxK);
            return kMeans.means.Select(i => new Lab((float)i[0], (float)i[1], (float)i[2])).Select(newLab => new UberColor(newLab)).ToList();
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
                var kMeans = new KMeans(inputK, data, 3, 0);
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

        private List<UberColor> FilterColorListForDistance( List<UberColor> colors)
        {
            List<UberColor> shuffled = new List<UberColor>(colors);
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
                    if (dist < _threshold)
                    {
                        success = false;
                        break;
                    }
                }
                if (success) selectedColors.Add(checkingColor);
            }
            return selectedColors;
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
            var colors = ColorSpaceIO.LoadColorsFromFiles(tmpPalettePaths);
            var selectedColors = FilterColorListForDistance(colors);
            var videoPalette = new ColorSpace(selectedColors);
            var videoPalettePath = ColorSpaceIO.WriteColorSpaceToDisk(videoPalette, Path.GetFileNameWithoutExtension(path), savePath);
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
            var totalFrames = (int)Math.Floor((float)(inputFile.Metadata.Duration.TotalSeconds / _interval));
            for (var i = 0; i < totalFrames; i++)
            {
                var outputFilePath = path + Path.GetFileNameWithoutExtension(path) + i + ".png";
                var outputFile = new MediaFile { Filename = outputFilePath };
                double time = _interval * i + 3;
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