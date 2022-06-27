﻿using System.Collections.Generic;
using Godot.Collections;

namespace ImageToPaletteGenerator
{
    public interface IProcessor
    {
        List<string> Process(string inputPath, string outputPath, Dictionary args = null);
    }
}