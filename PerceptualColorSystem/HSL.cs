using System;
using System.Numerics;
using ImageToPaletteGenerator;

namespace PercetualColorSystem
{
    public class HSL
    {
        public double H;
        public double S;
        public double L;
        
        public HSL(double h, double s, double l)
        {
            H = h;
            S = s;
            L = l;
        }

        public double DistanceTo(HSL target)
        {
            var dist = MathUtility.CylindricalDistance(new Vector3((float)(H * Math.PI * 2.0d), (float)S, (float)L),
                new Vector3((float)(target.H * Math.PI * 2.0d), (float)target.S, (float)target.L));
            return dist;
        }
        
        public double SquaredDistanceTo(HSL target)
        {
            var dist = MathUtility.CylindricalSquaredDistance(new Vector3((float)(H * Math.PI * 2.0d), (float)S, (float)L),
                new Vector3((float)(target.H * Math.PI * 2.0d), (float)target.S, (float)target.L));
            return dist;
        }

 
    }
}