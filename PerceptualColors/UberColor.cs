using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

namespace PercetualColors
{
    /// <summary>
    /// Color class containing HSL/HSV/LAB data and helper functions
    /// </summary>
    public class UberColor
    {
        private Color _color;

        //Need to optimize conversions
        [JsonConstructor]
        public UberColor(Color color, HSL hsl, Lab lab, Vector2 position)
        {
            Color = color;
            Hsl = hsl;
            Lab = lab;
        }

        public UberColor(Color color)
        {
            _color = color;
            var sRgb = new sRGB(color.r, color.g, color.b);
            Hsl = ColorConversion.SrgbToOkhsl(sRgb);
            Lab = ColorConversion.LinearSrgbToOklab(sRgb);
        }

        public UberColor(HSL hsl)
        {
            Hsl = hsl;
            var sRgb = ColorConversion.OkhslToSrgb(hsl);
            _color = new Color((float)sRgb.R, (float)sRgb.G, (float)sRgb.B);
            Lab = ColorConversion.OkhslToLab(Hsl);
        }

        public UberColor(Lab lab)
        {
            Lab = lab;
            var sRgb = ColorConversion.OklabToLinearSrgb(lab);
            _color = new Color((float)sRgb.R, (float)sRgb.G, (float)sRgb.B);
            Hsl = ColorConversion.LabToHSL(Lab);
        }

        public UberColor(string hex)
        {
            var newColor = new Color(hex.Substring(1));
            _color = newColor;
            var sRgb = new sRGB(_color.r, _color.g, _color.b);
            Hsl = ColorConversion.SrgbToOkhsl(sRgb);
            Lab = ColorConversion.LinearSrgbToOklab(sRgb);
        }

        public Color Color
        {
            get => _color;
            set => _color = value;
        }

        public HSL Hsl { get; set; }

        public Lab Lab { get; set; }

        public UberColor GetClosestColor(List<UberColor> collection)
        {
            var closestDist = double.MaxValue;
            var closestColor = new UberColor(Colors.White);
            foreach (var i in collection)
            {
                var distance = Lab.SquaredDistanceTo(i.Lab);
                if (distance < closestDist)
                {
                    closestDist = distance;
                    closestColor = i;
                }
            }
            return closestColor;
        }

        public double GetColorDistance(UberColor color)
        {
            return Lab.DistanceTo(color.Lab);
        }

        
        public override string ToString()
        {
            return (Hsl.H, Hsl.S, Hsl.L).ToString();
        }
    }
}