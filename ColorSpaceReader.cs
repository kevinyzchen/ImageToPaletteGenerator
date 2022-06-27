using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PercetualColors;

namespace ImageToPaletteGenerator
{
    public static class ColorSpaceReader
    {
        public static List<UberColor> LoadColorsFromFile(string path)
        {
            var fileColors = new List<UberColor>();
            var fileText = File.ReadAllText(path);
            var output = JsonConvert.DeserializeObject<ColorSpace>(fileText);
            if (output != null)
                foreach (var color in output.Colors)
                    fileColors.Add(color);
            return fileColors;
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
    }
}