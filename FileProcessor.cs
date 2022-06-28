using System;
using System.Collections.Generic;
using System.IO;
using MediaToolkit.Model;
using MediaToolkit.Options;
using static ImageToPaletteGenerator.PaletteExtraction;

namespace ImageToPaletteGenerator
{
    /// <summary>
    /// Main class that sorts through the input path and then converts all files found to color spaces
    /// </summary>
    public class FileProcessor
    {
        private List<string> ProcessResultFilePaths { get; set; } = new List<string>();
        private ExtractionArgs _extractionArgs;
        private int _filesProcessed;
        private int _totalFiles;

        private static List<string> FindFilePaths(string inputPath, List<string> allFilePaths)
        {
            if (Directory.Exists(inputPath))
            {
                var filePaths = Directory.GetFiles(inputPath);
                allFilePaths.AddRange(filePaths);

                var dirPaths = Directory.GetDirectories(inputPath);
                foreach (var path in dirPaths) FindFilePaths(path, allFilePaths);
            }
            else if (File.Exists(inputPath))
            {
                allFilePaths.Add(inputPath);
            }

            return allFilePaths;
        }

        public List<string> Process(string inputPath, string outputPath, KmeansArgs args, float interval = .5f,
            float threshold = .1f, int maxRes = 128)
        {
            ProcessResultFilePaths.Clear();
            _extractionArgs = new ExtractionArgs
            {
                Threshold = threshold,
                KmeansArgs = args,
                MaxRes = maxRes,
                Interval = interval
            };

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
            var palette = ImageToPalette(path, _extractionArgs);
            var selectedColors = FilterColorListForDistance(palette, _extractionArgs.Threshold);
            var colorPalette = new Palette(selectedColors);
            var result =
                PaletteIO.WritePaletteToDisk(colorPalette, Path.GetFileNameWithoutExtension(path), savePath);
            _filesProcessed++;
            return result;
        }

        private string ProcessVideo(string path, string savePath)
        {
            //Convert video to an image every x interval and saves them at a tmp path
            var tmpImagePaths = VideoToImages(path);
            var tmpPalettePaths = new List<string>();
            foreach (var file in tmpImagePaths)
            {
                var result = ProcessImage(file, savePath);
                tmpPalettePaths.Add(result);
            }

            //Save video palette
            var colors = PaletteIO.LoadColorsFromFiles(tmpPalettePaths);
            var selectedColors = FilterColorListForDistance(colors, _extractionArgs.Threshold);
            var videoPalette = new Palette(selectedColors);
            var videoPalettePath =
                PaletteIO.WritePaletteToDisk(videoPalette, Path.GetFileNameWithoutExtension(path), savePath);
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
            var totalFrames =
                (int)Math.Floor((float)(inputFile.Metadata.Duration.TotalSeconds / _extractionArgs.Interval));
            for (var i = 0; i < totalFrames; i++)
            {
                var outputFilePath = path + Path.GetFileNameWithoutExtension(path) + i + ".png";
                var outputFile = new MediaFile { Filename = outputFilePath };
                double time = _extractionArgs.Interval * i + 3;
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