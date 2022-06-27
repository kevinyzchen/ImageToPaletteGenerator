using System.Collections.Generic;
using System.Linq;

namespace ML
{
       public static class DBScan
    {
        public static List<List<DBPoint>> GetClusters(List<DBPoint> points, double eps, int minPts)
        {
            if (points == null) return null;
            List<List<DBPoint>> clusters = new List<List<DBPoint>>();
            eps *= eps; // square eps
            int clusterId = 1;
            for (int i = 0; i < points.Count; i++)
            {
                DBPoint p = points[i];
                if (p.ClusterId == DBPoint.UNCLASSIFIED)
                {
                    if (ExpandCluster(points, p, clusterId, eps, minPts)) clusterId++;
                }
            }
            // sort out DBPoints into their clusters, if any
            int maxClusterId = points.OrderBy(p => p.ClusterId).Last().ClusterId;
            if (maxClusterId < 1) return clusters; // no clusters, so list is empty
            for (int i = 0; i < maxClusterId; i++) clusters.Add(new List<DBPoint>());
            foreach (DBPoint p in points)
            {
                if (p.ClusterId > 0) clusters[p.ClusterId - 1].Add(p);
            }
            return clusters;
        }
        public static List<DBPoint> GetRegion(List<DBPoint> points, DBPoint p, double eps)
        {
            List<DBPoint> region = new List<DBPoint>();
            for (int i = 0; i < points.Count; i++)
            {
                int distSquared = DBPoint.DistanceSquared(p, points[i]);
                if (distSquared <= eps) region.Add(points[i]);
            }
            return region;
        }
        public static bool ExpandCluster(List<DBPoint> points, DBPoint p, int clusterId, double eps, int minPts)
        {
            List<DBPoint> seeds = GetRegion(points, p, eps);
            if (seeds.Count < minPts) // no core DBPoint
            {
                p.ClusterId = DBPoint.NOISE;
                return false;
            }
            else // all DBPoints in seeds are density reachable from DBPoint 'p'
            {
                for (int i = 0; i < seeds.Count; i++) seeds[i].ClusterId = clusterId;
                seeds.Remove(p);
                while (seeds.Count > 0)
                {
                    DBPoint currentP = seeds[0];
                    List<DBPoint> result = GetRegion(points, currentP, eps);
                    if (result.Count >= minPts)
                    {
                        for (int i = 0; i < result.Count; i++)
                        {
                            DBPoint resultP = result[i];
                            if (resultP.ClusterId == DBPoint.UNCLASSIFIED || resultP.ClusterId == DBPoint.NOISE)
                            {
                                if (resultP.ClusterId == DBPoint.UNCLASSIFIED) seeds.Add(resultP);
                                resultP.ClusterId = clusterId;
                            }
                        }
                    }
                    seeds.Remove(currentP);
                }
                return true;
            }
        }
    }
}