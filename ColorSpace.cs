using System.Collections.Generic;
using Newtonsoft.Json;
using PercetualColors;

namespace ImageToPaletteGenerator
{
    /// <summary>
    /// A color space is a collection of colors. 
    /// </summary>
    public class ColorSpace
    {
        [JsonConstructor]
        public ColorSpace(List<UberColor> colors)
        {
            Colors = colors;
        }
        public List<UberColor> Colors { get; }
    }
}