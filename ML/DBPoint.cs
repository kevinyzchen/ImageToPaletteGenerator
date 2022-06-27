using System;
using System.Numerics;

namespace ML
{
    public class DBPoint
    {
        public const int NOISE = -1;
        public const int UNCLASSIFIED = 0;
        public int X, Y, Z, ClusterId;
        public DBPoint(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
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