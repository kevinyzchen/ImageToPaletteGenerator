using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;
using PercetualColors;
using Path = System.IO.Path;

namespace ImageToPaletteGenerator
{
    public static class ColorSpaceReader
    {

        public static List<UberColor> LoadColorsFromFile(string path) {
            var fileColors = new List<UberColor>();
            var file = new File();
            file.Open(path, File.ModeFlags.Read);
            string fileText= file.GetAsText();
            ColorSpace output = JsonConvert.DeserializeObject<ColorSpace>(fileText);
            if (output != null)
                foreach (var color in output.Colors) {
                    fileColors.Add(color);
                }
            file.Close();
            return fileColors;
        }

        public static Dictionary<string, List<UberColor>> LoadColorsFromFolder(string folderPath)
        {
            Directory dir = new Directory();
            List<string> files = new List<string>();
            if (dir.Open(folderPath) == Error.Ok)
            {
                files = FileProcessor.GetFilePaths(dir, folderPath);
            }

            Dictionary<string, List<UberColor>> colorsFromFile = new Dictionary<string, List<UberColor>>();
            foreach (var file in files)
            {
                if (file.Extension() != "json") continue;
                var colors = LoadColorsFromFile(file);
                var name = Path.GetFileNameWithoutExtension(file);
                colorsFromFile.Add(name, colors);
            }

            return colorsFromFile;
        }

        public static List<UberColor> LoadColorsFromFiles(List<string> colorPalettePaths)
        {
            List<UberColor> allColors = new List<UberColor>();
            foreach (var path in colorPalettePaths)
            {
               var colors =  LoadColorsFromFile(path);
               allColors.AddRange(colors);
            }

            return allColors;
        }
        
    }
}