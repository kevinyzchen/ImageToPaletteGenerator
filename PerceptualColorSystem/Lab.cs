using System;
using System.Numerics;
using ImageToPaletteGenerator;

namespace PercetualColorSystem
{
    public class Lab
    {
        public readonly double L;
        public readonly double a;
        public readonly double b;

        public Lab(double l, double a, double b)
        {
            L = l;
            this.a = a;
            this.b = b;
        }

        public double DistanceTo(Lab other)
        {
            var dist = MathUtility.CartesianDistance(new Vector3((float)L, (float)a, (float)b), new Vector3((float)other.L, (float)other.a, (float)other.b));
            return dist;
        }
        public double SquaredDistanceTo(Lab other)
        {
            var dist = MathUtility.CartesianSquaredDistance(new Vector3((float)L, (float)a, (float)b), new Vector3((float)other.L, (float)other.a, (float)other.b));
            return dist;
        }
    }
}