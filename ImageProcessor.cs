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
        if (path.Extension() != "png" && path.Extension() != "jpeg" && path.Extension() != "jpg") return;
        var palette = ImageToPalette(path);
        var colorPalette = new ColorSpace(palette, new List<string> { "warm", "fun" }, "random name");
        var result = WritePaletteToDisk(colorPalette, Path.GetFileNameWithoutExtension(path), savePath);
        ResultFilePaths.Add(result);
        FilesProcessed++;
    }

    private List<UberColor> ImageToPalette(string path)
    {
        var pixels = ImageUtility.GetImagePixels(path);
        var data = ImageUtility.ColorToData(pixels);
        GD.Print(data.Length, "image data");
        var minK = (int)processingParams["min_k"];
        var maxK = (int)processingParams["max_k"];
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

    public List<string> Process(string imagePath, string outputPath, Dictionary args = null)
    {
        processingParams = args;
        ResultFilePaths.Clear();
        FilesProcessed = 0;
        ProcessFolder(ProcessImage, imagePath, outputPath);
        return ResultFilePaths;
    }

    public List<string> Process(List<string> filePaths, string outputPath, Dictionary args = null)
    {
        processingParams = args;
        ResultFilePaths.Clear();
        FilesProcessed = 0;
        foreach (var file in filePaths) ProcessImage(file, outputPath);
        return ResultFilePaths;
    }
}