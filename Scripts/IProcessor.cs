using System;
using System.Collections.Generic;
using Godot.Collections;

namespace ImageToPaletteGenerator
{
    public interface IProcessor
    {
        void Process(string inputPath, string outputPath, Dictionary args = null);
        List<Tuple<string, string>> ProcessResultFilePaths { get; set; }
    }
}