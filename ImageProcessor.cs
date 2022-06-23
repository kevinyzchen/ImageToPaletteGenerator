using System.Collections.Generic;
using ImageToPaletteGenerator;
using PercetualColorSystem;
using Image = Godot.Image;
using Path = System.IO.Path;
using Godot;

public class ImageProcessor : FileProcessor
{
    private int _k = 24;
    
    private void StartProcessing(string inputPath, string outputPath)
    {
        FilesProcessed = 0;
        var process = new Process(ProcessImage);
        ProcessFolder(process, inputPath, outputPath);
    }

    private void ProcessImage(string path, string savePath)
    {
        if (path.Extension() == "png" || path.Extension() == "jpeg" || path.Extension() == "jpg")
        {
            var palette = ImageToPalette(path);
            var colorPalette = new ColorPalette(palette, new List<string> { "warm", "fun" });
            WritePaletteToDisk(colorPalette, Path.GetFileNameWithoutExtension(path), savePath);
            FilesProcessed++;
        }
    }

    private List<UberColor> ImageToPalette(string path)
    {
        var pixels = GetImagePixels(path);
        var data = ColorToData(pixels);
        var km = new KMeans(_k, data, "plusplus", 6, 0);
        km.Cluster(3);
        var palette = new List<UberColor>();
        foreach (var i in km.means)
        {
            var newLab = new Lab((float)i[0], (float)i[1], (float)i[2]);
            var newColor = new UberColor(newLab);
            palette.Add(newColor);
        }

        return palette;
    }

    private UberColor[,] GetImagePixels(string imagePath)
    {
        var imageTexture = new ImageTexture();
        var image = new Image();
        image.Load(imagePath);
        GD.Print(imagePath);
        imageTexture.CreateFromImage(image);
        return GetImagePixels(imageTexture);
    }

    public List<List<UberColor>> GetImagePalette(List<UberColor> possibleColors)
    {
        return new List<List<UberColor>>();
    }

    private double[][] ColorToData(UberColor[,] pixels)
    {
        var data = new double[pixels.Length][];
        var counter = 0;
        foreach (var i in pixels)
        {
            data[counter] = new[] { i.Lab.L, i.Lab.a, (double)i.Lab.b };
            counter++;
        }
        return data;
    }

    public List<Vector3> GetColorPoints(UberColor[,] pixels)
    {
        var colorPoints = new List<Vector3>();
        foreach (var color in pixels)
        {
            var pos = new Vector3((float)color.Lab.L, (float)color.Lab.a, (float)color.Lab.b);
            colorPoints.Add(pos);
        }

        return colorPoints;
    }

    private UberColor[,] GetImagePixels(ImageTexture image)
    {
        var height = image.GetHeight();
        var width = image.GetWidth();
        var data = image.GetData();
        data.Lock();
        var pixelArray = new UberColor[width, height];
        for (var i = 0; i < width; i++)
        for (var j = 0; j < height; j++)
            pixelArray[i, j] = new UberColor(data.GetPixel(i, j));

        data.Unlock();
        return pixelArray;
    }
}