using System.Collections.Generic;
using Newtonsoft.Json;
using PercetualColorSystem;

namespace ImageToPaletteGenerator
{
    public class ColorSpace
    {
        [JsonConstructor]
        public ColorSpace(List<UberColor> colors,  List<string> tags, string name)
        {
            Name = name;
            Colors = colors;
            Tags = tags;
        }
        public List<UberColor> Colors { get; }
        public List<string> Tags { get; }
        public string Name { get; }
    }
}