using System.Collections.Generic;
using Newtonsoft.Json;
using PercetualColors;

namespace ImageToPaletteGenerator
{
    /// <summary>
    /// A color space is a collection of colors. 
    /// </summary>
    public class Palette
    {
        [JsonConstructor]
        public Palette(List<UberColor> colors)
        {
            Colors = colors;
        }
        public List<UberColor> Colors { get; }
    }
}