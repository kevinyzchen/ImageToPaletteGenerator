using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PercetualColors;

namespace ImageToPaletteGenerator
{
    /// <summary>
    /// Helper class with functions for reading and writing Color Space json objects
    /// </summary>
    public static class PaletteIO
    {
        public static List<UberColor> LoadColorsFromFile(string path)
        {
            var fileColors = new List<UberColor>();
            var output = LoadPaletteFromFile(path);
            if (output != null)
                foreach (var color in output.Colors)
                    fileColors.Add(color);
            return fileColors;
        }

        private static Palette LoadPaletteFromFile(string path)
        {
            var fileText = File.ReadAllText(path);
            Palette output = JsonConvert.DeserializeObject<Palette>(fileText);
            return output;
        }
        
        public static Dictionary<string, List<UberColor>> LoadColorsFromFolder(string folderPath)
        {
            var files = Directory.GetFiles(folderPath);
            var colorsFromFile = new Dictionary<string, List<UberColor>>();
            foreach (var file in files)
            {
                if (Path.GetExtension(file) != "json") continue;
                var colors = LoadColorsFromFile(file);
                var name = Path.GetFileNameWithoutExtension(file);
                colorsFromFile.Add(name, colors);
            }
            return colorsFromFile;
        }

        public static List<UberColor> LoadColorsFromFiles(List<string> colorPalettePaths)
        {
            var allColors = new List<UberColor>();
            foreach (var path in colorPalettePaths)
            {
                var colors = LoadColorsFromFile(path);
                allColors.AddRange(colors);
            }

            return allColors;
        }
        
        public static string WritePaletteToDisk(Palette colorPalette, string name, string path)
        {
            var savePath = path + "colorspace_" + name + ".json";
            var output = JsonConvert.SerializeObject(colorPalette, Formatting.Indented);
            File.WriteAllText(savePath, output);
            return savePath;
        }
    }
}