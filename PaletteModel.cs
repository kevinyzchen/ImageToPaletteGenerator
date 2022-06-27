using System.Collections.Generic;
using PercetualColorSystem;

namespace ImageToPaletteGenerator
{
    public class PaletteModel
    {
        private Dictionary<UberColor, Dictionary<UberColor, float>> colorCohesionMap;
        private Dictionary<UberColor, float> colorScores;
        private ColorSpace _refColorSpace;

        public PaletteModel(ColorSpace referenceColorSpace)
        {
            _refColorSpace = referenceColorSpace;
            InitializeModel();
        }

        private void InitializeModel()
        {
            var colors = _refColorSpace.Colors;
            colorScores = new Dictionary<UberColor, float>();
            colorCohesionMap = new Dictionary<UberColor, Dictionary<UberColor, float>>();
            foreach (var color in colors)
            {
                colorScores.Add(color, 0.0f);
            }

            foreach (var color in colors)
            {
                Dictionary<UberColor, float> cohesionScores = new Dictionary<UberColor, float>(colorScores);
                cohesionScores.Remove(color);
                colorCohesionMap.Add(color, cohesionScores);
            }
        }
    }
} 