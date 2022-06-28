using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace ImageToPaletteGenerator
{
    /// <summary>
    /// Wrapper class for FileProcessor to decouple the engine from everything else.
    /// </summary>
    public class GodotFileProcessor : Reference, IProcessor
    {
        public GodotFileProcessor()
        {
            _fileProcessor = new FileProcessor();
        }

        private List<string> ProcessResultFilePaths { get; set; } = new List<string>();
        private readonly FileProcessor _fileProcessor;

        public System.Collections.Generic.Dictionary<string, List<Color>> Preview()
        {
            var colorsFromFiles = new System.Collections.Generic.Dictionary<string, List<Color>>();
            foreach (var processResultFilePath in ProcessResultFilePaths)
            {
                var colors = PaletteIO.LoadColorsFromFile(processResultFilePath);
                var gdColors = colors.Select(o => o.Color).ToList();
                colorsFromFiles.Add(System.IO.Path.GetFileNameWithoutExtension(processResultFilePath), gdColors);
            }
            return colorsFromFiles;
        }

        /// <summary>
        /// Process the input path, creating a color space json object for all files found
        /// </summary>
        /// <param name="inputPath">Path to target file/s.</param>
        /// <param name="outputPath">Path to save resulting json files.</param>
        /// <param name="args">Arguments passed in from Godot.</param>
        /// <returns>A list of processed palettes as json objects.</returns>
        public List<string> Process(string inputPath, string outputPath, Dictionary args = null)
        {
            ProcessResultFilePaths.Clear();
            var Kmeans = new KmeansArgs
            {
                MinK = (int)args["min_k"],
                MaxK = (int)args["max_k"],
                Trials = (int)args["trials"]
            };
            var interval = (float)args["interval"];
            var threshold = (float)args["threshold"];
            var maxRes = (int)args["max_res"];
            var result = _fileProcessor.Process(inputPath, outputPath, Kmeans, interval, threshold, maxRes);
            ProcessResultFilePaths.AddRange(result);
            return ProcessResultFilePaths;
        }
    }
}