using System;
using System.Collections.Generic;
using System.IO;
using Path = System.IO.Path;
using Godot;
using Godot.Collections;
using ImageToPaletteGenerator;
using MediaToolkit.Model;
using MediaToolkit.Options;
using PercetualColorSystem;
using File = System.IO.File;

public class VideoProcessor : FileProcessor, IProcessor
{

    private ImageProcessor _imageProcessor;
    
    public VideoProcessor()
    {
        _imageProcessor = new ImageProcessor();
    }

    public List<string> Process(string inputPath, string outputPath, Dictionary args = null)
    {
        processingParams = args;
        ResultFilePaths.Clear();
        FilesProcessed = 0;
        ProcessVideo(inputPath, outputPath);
        return ResultFilePaths;
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
        _imageProcessor.Process(tempImagesPath, tempPalettesPath, processingParams);
        var palettePaths = _imageProcessor.ResultFilePaths;
        var colors = PaletteReader.LoadColorsFromFiles(palettePaths);
        var videoPalette = new ColorPalette(colors, new List<string> { "warm", "fun" });
        var videoPalettePath = WritePaletteToDisk(videoPalette, Path.GetFileNameWithoutExtension(path), savePath);
        palettePaths.ForEach(File.Delete);
        imagePaths.ForEach(File.Delete);
        ResultFilePaths.Add(videoPalettePath);
    }
    
    

    private List<string> VideoToImages(string path)
    {
        List<string> tmpImagePaths = new List<string>();
        var inputFile = new MediaFile {Filename = path};
        var engine = new MediaToolkit.Engine();
        engine.GetMetadata(inputFile);
        float interval = (float)processingParams["interval"];
        var totalFrames = Mathf.FloorToInt( (float) (inputFile.Metadata.Duration.TotalSeconds / interval));
        for (int i = 0; i < totalFrames; i++)
        {
            var outputFilePath = tempImagesPath + Path.GetFileNameWithoutExtension(path) + i + ".jpg";
            GD.PrintErr(outputFilePath);
            var outputFile = new MediaFile {Filename = outputFilePath};
            GD.Print("processing video", outputFilePath);
            double time = interval * i + 3;
            var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(time) };
            engine.GetThumbnail(inputFile, outputFile, options);
            tmpImagePaths.Add(outputFilePath);
        }
        return tmpImagePaths;
        // Saves the frame located on the 15th second of the video.

    }
}