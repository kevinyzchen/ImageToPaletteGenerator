using System;

namespace PercetualColorSystem
{
    public class Lab
    {
        public double L;
        public double a;
        public double b;

        public Lab(double l, double a, double b)
        {
            L = l;
            this.a = a;
            this.b = b;
        }


        public double DistanceTo(Lab other)
        {
            var dist = Math.Sqrt((other.L - this.L) * (other.L - this.L) +
                                 (other.a - this.a) * (other.a - this.a) +
                                 (other.b - this.b) * (other.b - this.b));
            return dist;
        }

        public double SquaredDistanceTo(Lab other)
        {
            var dist = (other.L - this.L) * (other.L - this.L) +
                       (other.a - this.a) * (other.a - this.a) +
                       (other.b - this.b) * (other.b - this.b);
            return dist;
        }
    }
}