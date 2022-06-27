using System;
using System.Collections.Generic;
using Path = System.IO.Path;
using Godot;
using Godot.Collections;
using MediaToolkit.Model;
using MediaToolkit.Options;
using PercetualColors;
using File = System.IO.File;


namespace ImageToPaletteGenerator
{
    public class VideoToColorSpaceProcessor : FileProcessor, IProcessor
    {
        private readonly ImageToColorSpaceProcessor _imageToColorSpaceProcessor;

        public VideoToColorSpaceProcessor()
        {
            _imageToColorSpaceProcessor = new ImageToColorSpaceProcessor();
        }

        public List<string> Process(string imagePath, string outputPath, Dictionary args = null)
        {
            ProcessingParams = args;
            ProcessResultFilePaths.Clear();
            FilesProcessed = 0;
            ProcessVideo(imagePath, outputPath);
            return ProcessResultFilePaths;
        }

        private void ProcessVideo(string inputPath, string outputPath)
        {
            GD.Print("checking format");
            if (inputPath.Extension() != "flv" && inputPath.Extension() != "mp4" && inputPath.Extension() != "mpg" &&
                inputPath.Extension() != "mpeg") return;

            GD.Print("format success ");
            VideoToPalette(inputPath, outputPath);
        }

        private void VideoToPalette(string path, string savePath)
        {
            var imagePaths = VideoToImages(path);
            var palettePaths = _imageToColorSpaceProcessor.Process(imagePaths, savePath, ProcessingParams);
            var colors = ColorSpaceReader.LoadColorsFromFiles(palettePaths);

            //Filter colors for distance
            var shuffled = new List<UberColor>(colors);
            shuffled.Shuffle();
            var remainingColors = new Queue<UberColor>(shuffled);
            var selectedColors = new List<UberColor>();
            while (remainingColors.Count > 0)
            {
                UberColor checkingColor = remainingColors.Dequeue();
                if (remainingColors.Count == 1)
                {
                    selectedColors.Add(checkingColor);
                    break;
                }

                var compareColors = new List<UberColor>(remainingColors);
                compareColors.Remove(checkingColor);
                bool success = true;
                foreach (var compareColor in compareColors)
                {
                    float dist = (float)checkingColor.Hsl.DistanceTo(compareColor.Hsl);
                    if (dist < (float)ProcessingParams["threshold"])
                    {
                        success = false;
                        break;
                    }
                }

                if (success) selectedColors.Add(checkingColor);
            }

            var videoPalette = new ColorSpace(selectedColors, new List<string> { "warm", "fun" }, "random name");
            var videoPalettePath = WritePaletteToDisk(videoPalette, Path.GetFileNameWithoutExtension(path), savePath);
            GD.Print(videoPalettePath);
            palettePaths.ForEach(File.Delete);
            imagePaths.ForEach(File.Delete);
            ProcessResultFilePaths.Add(videoPalettePath);
        }

        private List<string> VideoToImages(string path)
        {
            List<string> tmpImagePaths = new List<string>();
            var inputFile = new MediaFile { Filename = path };
            var engine = new MediaToolkit.Engine();
            engine.GetMetadata(inputFile);
            float interval = (float)ProcessingParams["interval"];
            var totalFrames = Mathf.FloorToInt((float)(inputFile.Metadata.Duration.TotalSeconds / interval));
            for (int i = 0; i < totalFrames; i++)
            {
                var outputFilePath = path + Path.GetFileNameWithoutExtension(path) + i + ".jpg";
                GD.PrintErr(outputFilePath);
                var outputFile = new MediaFile { Filename = outputFilePath };
                GD.Print("processing video", outputFilePath);
                double time = interval * i + 3;
                var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(time) };
                engine.GetThumbnail(inputFile, outputFile, options);
                tmpImagePaths.Add(outputFilePath);
                GD.Print(outputFilePath, " ", outputFile.Filename);
            }

            return tmpImagePaths;
        }
    }
}