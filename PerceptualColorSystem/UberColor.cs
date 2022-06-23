using System;
using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

namespace PercetualColorSystem
{
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
            Position = new Vector2((float)Hsl.S * 1f, (float)Hsl.L * 1f);
        }

        public UberColor(Color color)
        {
            _color = color;
            var sRgb = new sRGB(color.r, color.g, color.b);
            Hsl = ColorConversion.SrgbToOkhsl(sRgb);
            Lab = ColorConversion.LinearSrgbToOklab(sRgb);
            Position = new Vector2((float)Hsl.S * 1f, (float)Hsl.L * 1f);
        }

        public UberColor(HSL hsl)
        {
            Hsl = hsl;
            var sRgb = ColorConversion.OkhslToSrgb(hsl);
            _color = new Color((float)sRgb.R, (float)sRgb.G, (float)sRgb.B);
            Lab = ColorConversion.OkhslToLab(Hsl);
            Position = new Vector2((float)Hsl.S * 1f, (float)Hsl.L * 1f);
        }

        public UberColor(Lab lab)
        {
            Lab = lab;
            var sRgb = ColorConversion.OklabToLinearSrgb(lab);
            _color = new Color((float)sRgb.R, (float)sRgb.G, (float)sRgb.B);
            Hsl = ColorConversion.LabToHSL(Lab);
            Position = new Vector2((float)Hsl.S * 1f, (float)Hsl.L * 1f);
        }

        public UberColor(string hex)
        {
            var newColor = new Color(hex.Substring(1));
            _color = newColor;
            var sRgb = new sRGB(_color.r, _color.g, _color.b);
            Hsl = ColorConversion.SrgbToOkhsl(sRgb);
            Lab = ColorConversion.LinearSrgbToOklab(sRgb);
            Position = new Vector2((float)Hsl.S * 1f, (float)Hsl.L * 1f);
        }

        public Vector2 Position { get; set; }

        public Color Color
        {
            get => _color;
            set => _color = value;
        }

        public HSL Hsl { get; set; }

        public Lab Lab { get; set; }

        public void Randomize()
        {
            throw new NotImplementedException();
        }

        public UberColor GetClosestColor(List<UberColor> collection)
        {
            var closestDist = double.MaxValue;
            var closestColor = new UberColor(Colors.White);
            foreach (var i in collection)
            {
                var distance = Lab.DistanceTo(i.Lab);
                if (distance < closestDist)
                {
                    closestDist = distance;
                    closestColor = i;
                }
            }

            return closestColor;
        }

        public override string ToString()
        {
            return (Hsl.H, Hsl.S, Hsl.L).ToString();
        }
    }
}