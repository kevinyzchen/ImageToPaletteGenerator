using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Kneedle;
using MediaToolkit.Model;
using MediaToolkit.Options;
using PercetualColors;

namespace ImageToPaletteGenerator
{
    /// <summary>
    /// Main class that sorts through the input path and then converts all files found to color spaces
    /// </summary>
    public class FileProcessor 
    {
        private List<string> ProcessResultFilePaths { get; set; } = new List<string>();
        private KmeansArgs _processingParams;
        private int _filesProcessed;
        private int _totalFiles;
        private float _interval; //Snapshot every x seconds if the file is a video
        private float _threshold; //How close colors can be to each other in one color space before they're discarded
        private int _maxRes;
        
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
        
        public List<string> Process(string inputPath, string outputPath, KmeansArgs args, float interval = .5f, float threshold = .1f, int maxRes = 128)
        {
            ProcessResultFilePaths.Clear();
            _interval = interval;
            _threshold = threshold;
            _processingParams = args;
            _maxRes = maxRes;
            
            //Get files
            var filePaths = FindFilePaths(inputPath, new List<string>());
            _totalFiles = filePaths.Count;
            _filesProcessed = 0;
            
            //Process files based on extensions
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
            var image = Image.FromFile(path);
            var pixels = ImageIO.GetImageColors(image, _maxRes);
            var palette = new List<UberColor>();
            palette = UseKMeans(pixels);
            return palette;
        }

        private List<UberColor> UseKMeans(UberColor[,] pixelColors)
        {
            //Change pixelColors to appropriate data type
            Random kmeansRNG = new Random();
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
            var x = new double[_processingParams.MaxK - _processingParams.MinK];
            var y = new double[_processingParams.MaxK -  _processingParams.MinK];
            for (var i = 0; i < _processingParams.MaxK -  _processingParams.MinK; i++)
            {
                int seed = kmeansRNG.Next();
                var inputK =  _processingParams.MinK + i;
                var kMeans = new KMeans(inputK, data, _processingParams.Trials, seed );
                kMeans.Cluster(3);
                kMeansFromK.Add(inputK, kMeans);
                x[i] = inputK;
                y[i] = kMeans.Wcss;
            }

            //Use knee method to find the best K value
            var k = KneedleAlgorithm.CalculateKneePoints(x, y, CurveDirection.Decreasing, Curvature.Counterclockwise);
            Debug.Assert(k != null, nameof(k) + " != null");
            var chosenKMeans = kMeansFromK[(int)Math.Floor((float)k)];
            
            return chosenKMeans.Means.Select(i => new Lab((float)i[0], (float)i[1], (float)i[2])).Select(newLab => new UberColor(newLab)).ToList();
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
            //Convert video to an image every x interval and saves them at a tmp path
            var tmpImagePaths = VideoToImages(path);
            List<string> tmpPalettePaths = new List<string>();
            foreach (var file in tmpImagePaths)
            {
                var result = ProcessImage(file, savePath);
                tmpPalettePaths.Add(result);
            }
            
            //Save video palette
            var colors = ColorSpaceIO.LoadColorsFromFiles(tmpPalettePaths);
            var selectedColors = FilterColorListForDistance(colors);
            var videoPalette = new ColorSpace(selectedColors);
            var videoPalettePath = ColorSpaceIO.WriteColorSpaceToDisk(videoPalette, Path.GetFileNameWithoutExtension(path), savePath);
            _filesProcessed++;
            
            //Clean up
            tmpPalettePaths.ForEach(File.Delete);
            tmpImagePaths.ForEach(File.Delete);
            return videoPalettePath;
        }

        private List<string> VideoToImages(string path)
        {
            var tmpImagePaths = new List<string>();
            var inputFile = new MediaFile { Filename = path };
            var engine = new MediaToolkit.Engine();
            engine.GetMetadata(inputFile);
            //Get total frames based on interval
            //Iterate through total frames, find an image every x seconds in the video, and save it
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