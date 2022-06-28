using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;
using Path = System.IO.Path;

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

        public List<Tuple<string, string>> ProcessResultFilePaths { get; set; } = new List<Tuple<string, string>>();
        private readonly FileProcessor _fileProcessor;

        public Dictionary Preview()
        {
            var namesFromFiles = new List<string>();
            var colorsFromFiles = new Array();
            var thumbnailsPaths = new List<string>();

            foreach (var processResultFilePath in ProcessResultFilePaths)
            {
                var colors = PaletteIO.LoadColorsFromFile(processResultFilePath.Item1);
                GD.PrintErr(colors.Count);
                var colorArray = new Array();
                var gdColors = colors.Select(o => o.Color);
                foreach (var color in gdColors) colorArray.Add(color);
                colorsFromFiles.Add(colorArray);
                thumbnailsPaths.Add(processResultFilePath.Item2);
                namesFromFiles.Add(Path.GetFileNameWithoutExtension(processResultFilePath.Item1));
            }

            var results = new Dictionary();
            results["names"] = namesFromFiles;
            results["thumbnails"] = thumbnailsPaths;
            results["palettes"] = colorsFromFiles;
            return results;
        }

        /// <summary>
        /// Process the input path, creating a color space json object for all files found
        /// </summary>
        /// <param name="inputPath">Path to target file/s.</param>
        /// <param name="outputPath">Path to save resulting json files.</param>
        /// <param name="args">Arguments passed in from Godot.</param>
        /// <returns>A list of processed palettes as json objects.</returns>
        public void Process(string inputPath, string outputPath, Dictionary args = null)
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
            ProcessResultFilePaths = result;
        }
    }
}