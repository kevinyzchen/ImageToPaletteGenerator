
using System;
using System.Numerics;
using ImageToPaletteGenerator;

namespace PercetualColorSystem
{
    public class HSV
{
    public double H { get; private set; }
    public double S { get; private set; }
    public double V { get; private set; }

    public HSV(double h, double s, double v)
    {
        H = h;
        S = s;
        V = v;
    }
    
    public double DistanceTo(HSV target)
    {
        var dist = MathUtility.CylindricalDistance(new Vector3((float)(H * Math.PI * 2.0d), (float)S, (float)V),
            new Vector3((float)(target.H * Math.PI * 2.0d), (float)target.S, (float)target.V));
        return dist;
    }
        
    public double SquaredDistanceTo(HSV target)
    {
        var dist = MathUtility.CylindricalSquaredDistance(new Vector3((float)(H * Math.PI * 2.0d), (float)S, (float)V),
            new Vector3((float)(target.H * Math.PI * 2.0d), (float)target.S, (float)target.V));
        return dist;
    }
}
}