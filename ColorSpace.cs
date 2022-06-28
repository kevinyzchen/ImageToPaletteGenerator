using System.Collections.Generic;
using Newtonsoft.Json;
using PercetualColors;

namespace ImageToPaletteGenerator
{
    /// <summary>
    /// A color space is a collection of colors. It's used 
    /// </summary>
    public class ColorSpace
    {
        [JsonConstructor]
        public ColorSpace(List<UberColor> colors)
        {
            Colors = colors;
        }
        public List<UberColor> Colors { get; }
        /// <summary>
        /// The palette from each individual frame/image
        /// </summary>
        public Dictionary<string, List<UberColor>> colorSpaceGroups;
    }
}