using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;
using PercetualColorSystem;
using Path = System.IO.Path;

namespace ImageToPaletteGenerator
{
    public static class ColorSpaceReader
    {
        private static readonly string _colorLibraryFolderPath = "res://Data/";

        public static Dictionary<string, List<UberColor>> GetLibrary() {
            var colorLibrary = new Dictionary<string, List<UberColor>>();
            var dir = new Directory();
            dir.Open(_colorLibraryFolderPath);
            dir.ListDirBegin();
            //while (!string.IsNullOrEmpty(fileName)) {
            for (int i = 0; i < 100; i++) {
                var fileName = dir.GetNext();
                if (fileName.Empty()) break;
                string[] words = fileName.Split('_', '.');
                if (words[0] == "colors") {
                    colorLibrary[words[1]] = LoadColorsFromFile((dir.GetCurrentDir() + "/" + fileName));
                }
            }
            return colorLibrary;
        }

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