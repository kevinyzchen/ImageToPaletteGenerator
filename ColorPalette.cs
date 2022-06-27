using System.Collections.Generic;
using Newtonsoft.Json;
using PercetualColors;


namespace ImageToPaletteGenerator
{
    public class ColorPalette
    {
        [JsonConstructor]
        public ColorPalette(List<UberColor> mainColors,  List<string> tags) {
            MainColors = mainColors;
            Tags = tags;
        }
        private Dictionary<UberColor, List<UberColor>> _shades;
        private Dictionary<UberColor, List<UberColor>> _tint;
        private Dictionary<List<UberColor>, UberColor> _neutrals;
        public List<UberColor> MainColors { get; set; }
        public List<string> Tags { get; set; }
    }
}