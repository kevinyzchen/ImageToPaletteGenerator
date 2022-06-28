using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace ImageToPaletteGenerator
{
    public class GodotFileProcessor : Reference, IProcessor
    {
        public GodotFileProcessor()
        {
            _fileProcessor = new FileProcessor();
        }

        private List<string> ProcessResultFilePaths { get; set; } = new List<string>();
        private FileProcessor _fileProcessor;
        
        public System.Collections.Generic.Dictionary<string, List<Color>> Preview()
        {
            var colorsFromFiles = new System.Collections.Generic.Dictionary<string, List<Color>>();
            foreach (var processResultFilePath in ProcessResultFilePaths)
            {
                var colors = ColorSpaceIO.LoadColorsFromFile(processResultFilePath);
                var gdColors = colors.Select(o => o.Color).ToList();
                colorsFromFiles.Add(System.IO.Path.GetFileNameWithoutExtension(processResultFilePath), gdColors);
            }

            return colorsFromFiles;
        }

        public List<string> Process(string inputPath, string outputPath, Dictionary args = null)
        {
            GD.PrintErr(args.Count);
            var Kmeans = new KmeansArgs
            {
                minK = (int)args["min_k"],
                maxK = (int)args["max_k"],
                trials = (int)args["trials"],
            };
            float interval = (float)args["interval"];
            float threshold = (float)args["threshold"];
            var result = _fileProcessor.Process(inputPath, outputPath, Kmeans, interval, threshold);
            ProcessResultFilePaths.AddRange(result);
            return ProcessResultFilePaths;
        }
    }
}