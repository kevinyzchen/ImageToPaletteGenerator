using System;

namespace ImageToPaletteGenerator
{

    public struct KmeansArgs
    {
        public int MinK;
        public int MaxK;
        public int Trials;

    }
    public class KMeans
{
    private int K; // number clusters (use lower k for indexing)
    private double[][] data; // data to be clustered
    private int _n; // number data items
    private int _dim; // number values in each data item
    private int _maxIter; // max per single clustering attempt
    private int[] _clustering; // final cluster assignments
    public double[][] Means; // final cluster means aka centroids
    public double Wcss; // final total within-cluster sum of squares (inertia??)
    private int[] _counts; // final num items in each cluster
    private Random _rnd; // for initialization

    public KMeans(int K, double[][] data,  int maxIter, int seed)
    {
        this.K = K;
        this.data = data; // reference copy
        this._maxIter = maxIter;

        _n = data.Length;
        _dim = data[0].Length;

        Means = new double[K][]; // one mean per cluster
        for (var k = 0; k < K; ++k)
            Means[k] = new double[_dim];
        _clustering = new int[_n]; // cell val is cluster ID, index is data item
        _counts = new int[K]; // one cell per cluster
        Wcss = double.MaxValue; // smaller is better

        _rnd = new Random(seed);
    } // ctor

    public void Cluster(int trials)
    {
        for (var trial = 0; trial < trials; ++trial)
            Cluster(); // find a clustering and update bests
    }

    private void Cluster()
    {
        // init clustering[] and means[][] 
        // loop at most maxIter times
        //   update means using curr clustering
        //   update clustering using new means
        // end-loop
        // if clustering is new best, update clustering, means, counts, wcss

        var currClustering = new int[_n]; // [0, 0, 0, 0, .. ]

        var currMeans = new double[K][];
        for (var k = 0; k < K; ++k)
            currMeans[k] = new double[_dim];

        InitPlusPlus(data, currClustering, currMeans, _rnd);


        bool changed; //  result from UpdateClustering (to exit loop)
        var iter = 0;
        while (iter < _maxIter)
        {
            UpdateMeans(currMeans, data, currClustering);
            changed = UpdateClustering(currClustering,
                data, currMeans);
            if (changed == false)
                break; // need to stop iterating
            ++iter;
        }

        var currWCSS = ComputeWithinClusterSS(data,
            currMeans, currClustering);
        if (currWCSS < Wcss) // new best clustering found
        {
            // copy the clustering, means; compute counts; store WCSS
            for (var i = 0; i < _n; ++i)
                _clustering[i] = currClustering[i];

            for (var k = 0; k < K; ++k)
            for (var j = 0; j < _dim; ++j)
                Means[k][j] = currMeans[k][j];

            _counts = ComputeCounts(K, currClustering);
            Wcss = currWCSS;
        }
    } // Cluster()

    private static void InitPlusPlus(double[][] data,
        int[] clustering, double[][] means, Random rnd)
    {
        //  k-means++ init using roulette wheel selection
        // clustering[] and means[][] exist
        var N = data.Length;
        var dim = data[0].Length;
        var K = means.Length;

        // select one data item index at random as 1st meaan
        var idx = rnd.Next(0, N); // [0, N)
        for (var j = 0; j < dim; ++j) means[0][j] = data[idx][j];
        for (var k = 1; k < K; ++k) // find each remaining mean
        {
            var dSquareds = new double[N]; // from each item to its closest mean

            for (var i = 0; i < N; ++i) // for each data item
            {
                // compute distances from data[i] to each existing mean (to find closest)
                var distances = new double[k]; // we currently have k means

                for (var ki = 0; ki < k; ++ki)
                    distances[ki] = EucDistance(data[i], means[ki]);

                var mi = ArgMin(distances); // index of closest mean to curr item
                // save the associated distance-squared
                dSquareds[i] = distances[mi] * distances[mi]; // sq dist from item to its closest mean
            } // i

            // select an item far from its mean using roulette wheel
            // if an item has been used as a mean its distance will be 0
            // so it won't be selected

            var newMeanIdx = ProporSelect(dSquareds, rnd);
            for (var j = 0; j < dim; ++j)
                means[k][j] = data[newMeanIdx][j];
        } // k remaining means

        //Console.WriteLine("");
        //ShowMatrix(means, 4, 10);
        //Console.ReadLine();

        UpdateClustering(clustering, data, means);
    } // InitPlusPlus

    private static int ProporSelect(double[] vals, Random rnd)
    {
        // roulette wheel selection
        // on the fly technique
        // vals[] can't be all 0.0s
        var n = vals.Length;

        var sum = 0.0;
        for (var i = 0; i < n; ++i)
            sum += vals[i];

        var cumP = 0.0; // cumulative prob
        var p = rnd.NextDouble();

        for (var i = 0; i < n; ++i)
        {
            cumP += vals[i] / sum;
            if (cumP > p) return i;
        }

        return n - 1; // last index
    }

    private static int[] ComputeCounts(int K, int[] clustering)
    {
        var result = new int[K];
        for (var i = 0; i < clustering.Length; ++i)
        {
            var cid = clustering[i];
            ++result[cid];
        }

        return result;
    }

    private static void UpdateMeans(double[][] means,
        double[][] data, int[] clustering)
    {
        // compute the K means using data and clustering
        // assumes no empty clusters in clustering

        var K = means.Length;
        var N = data.Length;
        var dim = data[0].Length;

        var counts = ComputeCounts(K, clustering); // needed for means

        for (var k = 0; k < K; ++k) // make sure no empty clusters
            if (counts[k] == 0)
                throw new Exception("empty cluster passed to UpdateMeans()");

        var result = new double[K][]; // new means
        for (var k = 0; k < K; ++k)
            result[k] = new double[dim];

        for (var i = 0; i < N; ++i) // each data item
        {
            var cid = clustering[i]; // which cluster ID?
            for (var j = 0; j < dim; ++j)
                result[cid][j] += data[i][j]; // accumulate
        }

        // divide accum sums by counts to get means
        for (var k = 0; k < K; ++k)
        for (var j = 0; j < dim; ++j)
            result[k][j] /= counts[k];

        // no 0-count clusters so update the means
        for (var k = 0; k < K; ++k)
        for (var j = 0; j < dim; ++j)
            means[k][j] = result[k][j];
    }

    private static bool UpdateClustering(int[] clustering,
        double[][] data, double[][] means)
    {
        // update existing cluster clustering using data and means
        // proposed clustering would have an empty cluster: return false - no change to clustering
        // proposed clustering would be no change: return false, no change to clustering
        // proposed clustering is different and has no empty clusters: return true, clustering is changed

        var K = means.Length;
        var N = data.Length;

        var result = new int[N]; // proposed new clustering (cluster assignments)
        var change = false; // is there a change to the existing clustering?
        var counts = new int[K]; // check if new clustering makes an empty cluster

        for (var i = 0; i < N; ++i) // make of copy of existing clustering
            result[i] = clustering[i];

        for (var i = 0; i < data.Length; ++i) // each data item
        {
            var dists = new double[K]; // dist from curr item to each mean
            for (var k = 0; k < K; ++k)
                dists[k] = EucDistance(data[i], means[k]);

            var cid = ArgMin(dists); // index of the smallest distance
            result[i] = cid;
            if (result[i] != clustering[i])
                change = true; // the proposed clustering is different for at least one item
            ++counts[cid];
        }

        if (change == false)
            return false; // no change to clustering -- clustering has converged

        for (var k = 0; k < K; ++k)
            if (counts[k] == 0)
                return false; // no change to clustering because would have an empty cluster

        // there was a change and no empty clusters so update clustering
        for (var i = 0; i < N; ++i)
            clustering[i] = result[i];

        return true; // successful change to clustering so keep looping
    }

    private static double EucDistance(double[] item, double[] mean)
    {
        // Euclidean distance from item to mean
        // used to determine cluster assignments
        var sum = 0.0;
        for (var j = 0; j < item.Length; ++j)
            sum += (item[j] - mean[j]) * (item[j] - mean[j]);
        return Math.Sqrt(sum);
    }

    private static int ArgMin(double[] v)
    {
        var minIdx = 0;
        var minVal = v[0];
        for (var i = 0; i < v.Length; ++i)
            if (v[i] < minVal)
            {
                minVal = v[i];
                minIdx = i;
            }

        return minIdx;
    }

    private static double ComputeWithinClusterSS(double[][] data,
        double[][] means, int[] clustering)
    {
        // compute total within-cluster sum of squared differences between 
        // cluster items and their cluster means
        // this is actually the objective function, not distance
        var sum = 0.0;
        for (var i = 0; i < data.Length; ++i)
        {
            var cid = clustering[i]; // which cluster does data[i] belong to?
            sum += SumSquared(data[i], means[cid]);
        }

        return sum;
    }

    private static double SumSquared(double[] item, double[] mean)
    {
        // squared distance between vectors
        // surprisingly, k-means minimizes this, not distance
        var sum = 0.0;
        for (var j = 0; j < item.Length; ++j)
            sum += (item[j] - mean[j]) * (item[j] - mean[j]);
        return sum;
    }

    
    // display functions for debugging

    //private static void ShowVector(int[] vec, int wid)  // debugging use
    //{
    //  int n = vec.Length;
    //  for (int i = 0; i < n; ++i)
    //    Console.Write(vec[i].ToString().PadLeft(wid));
    //  Console.WriteLine("");
    //}

    //private static void ShowMatrix(double[][] m, int dec, int wid)  // debugging
    //{
    //  for (int i = 0; i < m.Length; ++i)
    //  {
    //    for (int j = 0; j < m[0].Length; ++j)
    //    {
    //      double x = m[i][j];
    //      if (Math.Abs(x) < 1.0e-5) x = 0.0;
    //      Console.Write(x.ToString("F" + dec).PadLeft(wid));
    //    }
    //    Console.WriteLine("");
    //  }
    //}

} // class KMeans
}