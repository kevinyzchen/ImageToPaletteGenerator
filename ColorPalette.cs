using System.Collections.Generic;
using Newtonsoft.Json;
using PercetualColorSystem;

namespace ImageToPaletteGenerator
{
    public class ColorPalette
    {
        // private Dictionary<UberColor, List<UberColor>> _shades;
        // private Dictionary<UberColor, List<UberColor>> _tint;
        
        [JsonConstructor]
        public ColorPalette(List<UberColor> mainColors,  List<string> tags) {
            MainColors = mainColors;
            Tags = tags;
        }
        public List<UberColor> MainColors { get; set; }
        public List<string> Tags { get; set; }
    }
}