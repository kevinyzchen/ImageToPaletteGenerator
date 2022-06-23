
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
}
}