using System;

namespace ML
{
    public class DBPoint
    {
        public const int NOISE = -1;
        public const int UNCLASSIFIED = 0;
        public int X, Y, Z, ClusterId;
        public DBPoint(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Y = z;
        }
        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y, Z);
        }
        public static int DistanceSquared(DBPoint p1, DBPoint p2)
        {
            int diffX = p2.X - p1.X;
            int diffY = p2.Y - p1.Y;
            int diffZ = p2.Z - p1.Z;
            return diffX * diffX + diffY * diffY + diffZ * diffZ;
        }
    }
}