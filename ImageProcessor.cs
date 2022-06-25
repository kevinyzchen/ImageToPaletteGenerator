using System.Collections.Generic;
using ImageToPaletteGenerator;
using PercetualColorSystem;
using Path = System.IO.Path;
using Godot;
using Godot.Collections;

public class ImageProcessor : FileProcessor, IProcessor
{

    private void ProcessImage(string path, string savePath)
    {
        GD.PrintErr(savePath);
        if (path.Extension() != "png" && path.Extension() != "jpeg" && path.Extension() != "jpg") return;
        var palette = ImageToPalette(path);
        GD.Print("got palette", palette.Count);
        var colorPalette = new ColorPalette(palette, new List<string> { "warm", "fun" });
        var result = WritePaletteToDisk(colorPalette, Path.GetFileNameWithoutExtension(path), savePath);
        ResultFilePaths.Add(result);
        FilesProcessed++;
    }

    private List<UberColor> ImageToPalette(string path)
    {
        var pixels = ImageUtility.GetImagePixels(path);
        var data =  ImageUtility.ColorToData(pixels);
        GD.Print(data.Length, "image data");
        int minK = (int)processingParams["min_k"];
        int maxK = (int)processingParams["max_k"];
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


    public List<string> Process(string inputPath, string outputPath, Dictionary args = null)
    {
        processingParams = args;
        ResultFilePaths.Clear();
        FilesProcessed = 0;
        var process = new Process(ProcessImage);
        ProcessFolder(process, inputPath, outputPath);
        return ResultFilePaths;
    }
    
    public List<string> Process(List<string> filePaths, string outputPath, Dictionary args = null)
    {
        processingParams = args;
        ResultFilePaths.Clear();
        FilesProcessed = 0;
        var process = new Process(ProcessImage);
        foreach (var file in filePaths)
        {
            process(file, outputPath);
        }
        return ResultFilePaths;
    }
    
}