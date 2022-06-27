using System;
using System.Numerics;

namespace PercetualColors
{
    public static class MathUtility
    {
        public static double CylindricalDistance(Vector3 pointA, Vector3 pointB)
        {
            Vector3 target = CylindricalToCartesian(pointB);
            Vector3 origin = CylindricalToCartesian(pointA);
            float dist = (origin - target).Length();
            return dist;
        }

        public static double CylindricalSquaredDistance(Vector3 from, Vector3 to)
        {
            Vector3 target = CylindricalToCartesian(to);
            Vector3 origin = CylindricalToCartesian(from);
            float dist = (target - origin).LengthSquared();
            return dist;
        }
        
        public static Vector3 CylindricalToCartesian(Vector3 point)
        {
            Vector3 cartesian = new Vector3((float)(point.Y * Math.Cos(point.X )),
                (float)(point.Y * Math.Sin(point.X)), (float)(point.Z));
            return cartesian;
        }

        public static double CartesianDistance(Vector3 from, Vector3 to)
        {
            var dist = (to - from).Length();
            return dist;
        }
      
        public static double CartesianSquaredDistance(Vector3 from, Vector3 to)
        {
            var dist = (to - from).LengthSquared();
            return dist;
        }  
        

    }
}