namespace PercetualColors
{
    public class sRGB
    {
        public double R { get; private set; }
        public double G { get; private set; }
        public double B { get; private set; }

        public sRGB(double r, double g, double b)
        {
            this.R = r;
            this.G = g;
            this.B = b;
        }
    }
}