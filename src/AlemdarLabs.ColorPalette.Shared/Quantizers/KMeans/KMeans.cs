using System;

namespace AlemdarLabs.ColorPalette.ColorReduction.Qauntizers
{
    // Resournces: 
    // * https://visualstudiomagazine.com/Articles/2013/12/01/K-Means-Data-Clustering-Using-C.aspx
    // * https://visualstudiomagazine.com/Articles/2020/05/06/data-clustering-k-means.aspx
    public partial class KMeans
    {
        /// <summary>
        /// Final cluster assignments. 
        /// Cell value is cluster ID, index is data item
        /// </summary>
        public int[] Labels; // final cluster assignments, cell val is cluster ID, index is data item

        /// <summary>
        /// Final cluster means aka centroids
        /// </summary>
        public double[][] ClusterCenters;

        private readonly Random Random; // central randomization for initialization

        /// <summary>
        /// The total WCSS is a measure of how good a particular clustering of data is. 
        /// Smaller values are better. There is a WCSS for each cluster, 
        /// computed as the sum of the squared differences between 
        /// data items in a cluster and their cluster mean. 
        /// The total WCSS is the sum of the WCSS values for each cluster. 
        /// The k-means algorithm minimizes the total WCSS.
        /// final total within-cluster sum of squares (inertia??)
        /// </summary>
        private double WCSS;

        public KMeans(int? seed = 0)
        {
            Random = new Random(seed ?? 0);
            WCSS = double.MaxValue;
        }

        public void Cluster(double[][] data, int numberOfClusters)
        {
            Initialization(data.Length, data[0].Length, numberOfClusters);
            Cluster(data, numberOfClusters, InitializationMethod.KMeansPlusPlus);
        }

        public void Cluster(double[][] data, 
            int numberOfClusters,
            int trialCount,
            InitializationMethod initializationMethod = InitializationMethod.KMeansPlusPlus)
        {
            Initialization(data.Length, data[0].Length, numberOfClusters);

            for (int trial = 0; trial < trialCount; trial++)
            {
                Cluster(data, numberOfClusters, initializationMethod);
            }
        }

        private void Cluster(double[][] data,
            int numberOfClusters,
            InitializationMethod initializationMethod = InitializationMethod.KMeansPlusPlus)
        {
            // init clustering[] and means[][] 
            // loop at most maxIter times
            //   update means using curr clustering
            //   update clustering using new means
            // end-loop
            // if clustering is new best, update clustering, means, counts, wcss

            int numberOfTuples = data.Length;
            int dimension = data[0].Length;
            double[][] currentMeans = AllocateMatrix(numberOfClusters, dimension);
            int[] currentClustering = new int[numberOfTuples];  // [0, 0, 0, 0, .. ]

            // track whether or not any of the data tuples changed clusters,
            // or equivalently, whether or not the clustering has changed
            bool changed = true;

            // whether the means of the clusters were able to be computed
            bool success = true;

            switch (initializationMethod)
            {
                case InitializationMethod.Random:
                    RandomInit(currentClustering, numberOfClusters);
                    break;

                case InitializationMethod.KMeansPlusPlus:
                default:
                    KMeansPlusPlusInit(data, currentClustering, currentMeans);
                    break;
                case InitializationMethod.Forgy:
                    throw new NotImplementedException();
            }

            int maxIteration = numberOfTuples * 2;
            int currentIteration = 0;
            while (success == true && changed == true && currentIteration < maxIteration)
            {
                success = UpdateMeans(data, currentClustering, currentMeans);
                changed = UpdateClustering(data, currentClustering, currentMeans);
                ++currentIteration;
            }

            double currentWCSS = ComputeWithinClusterSS(data,
                  currentMeans, currentClustering);
            if (currentWCSS < WCSS)  // new best clustering found
            {
                // copy the clustering, means; compute counts; store WCSS
                for (int i = 0; i < numberOfTuples; ++i)
                    Labels[i] = currentClustering[i];

                for (int k = 0; k < numberOfClusters; ++k)
                    for (int j = 0; j < dimension; ++j)
                        ClusterCenters[k][j] = currentMeans[k][j];

                WCSS = currentWCSS;
            }

            //Labels = currentClustering;
            //ClusterCenters = currentMeans;
        }

        /// <summary>
        /// returns false if there is a cluster that has no tuples assigned to it
        /// Suppose a cluster has three height-weight tuples: 
        /// d0 = {64, 110}, d1 = {65, 160}, d2 = {72, 180}. 
        /// The mean of the cluster is computed as 
        /// {(64+65+72)/3, (110+160+180)/3} = {67.0, 150.0}. 
        /// In other words, you just compute the average of each data component
        /// </summary>
        /// <param name="data"></param>
        /// <param name="clustering"></param>
        /// <param name="means">parameter means[][] is really a ref parameter</param>
        /// <returns></returns>
        private bool UpdateMeans(double[][] data, int[] clustering, double[][] means)
        {
            // returns false if there is a cluster that has no tuples assigned to it
            // parameter means[][] is really a ref parameter

            // check existing cluster counts
            // can omit this check if InitClustering and UpdateClustering
            // both guarantee at least one tuple in each cluster (usually true)
            int numberOfClusters = means.Length;
            int[] clusterCounts = new int[numberOfClusters];
            for (int i = 0; i < data.Length; ++i)
            {
                int cluster = clustering[i];
                ++clusterCounts[cluster];
            }

            for (int k = 0; k < numberOfClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false;

            // update, zero-out means so it can be used as scratch matrix 
            //for (int k = 0; k < means.Length; ++k)
            //    for (int j = 0; j < means[k].Length; ++j)
            //        means[k][j] = 0.0;
            for (int k = 0; k < means.Length; ++k)
                Array.Clear(means[k], 0, means[k].Length);

            for (int i = 0; i < data.Length; ++i)
            {
                int cluster = clustering[i];
                for (int j = 0; j < data[i].Length; ++j)
                    means[cluster][j] += data[i][j]; // accumulate sum
            }

            for (int k = 0; k < means.Length; ++k)
                for (int j = 0; j < means[k].Length; ++j)
                    means[k][j] /= clusterCounts[k]; // danger of div by 0

            return true;
        }

        /// <summary>
        /// (re)assign each tuple to a cluster (closest mean)
        /// returns false if no tuple assignments change OR
        /// if the reassignment would result in a clustering where
        /// one or more clusters have no tuples.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="clustering"></param>
        /// <param name="means"></param>
        /// <returns></returns>
        private bool UpdateClustering(double[][] data, int[] clustering, double[][] means)
        {
            // update existing cluster clustering using data and means
            // proposed clustering would have an empty cluster: return false - no change to clustering
            // proposed clustering would be no change: return false, no change to clustering
            // proposed clustering is different and has no empty clusters: return true, clustering is changed

            int numberOfClusters = means.Length;
            bool changed = false;

            int[] newClustering = new int[clustering.Length]; // proposed new clustering (cluster assignments)
            Array.Copy(clustering, newClustering, clustering.Length); // make of copy of existing clustering

            double[] distances = new double[numberOfClusters]; // distances from curr tuple to each mean
            int[] clusterCounts = new int[numberOfClusters]; // check proposed clustering[] cluster counts

            for (int i = 0; i < data.Length; ++i) // walk through each tuple
            {
                for (int k = 0; k < numberOfClusters; ++k)
                    distances[k] = EuclideanDistance(data[i], means[k]); // compute distances from curr tuple to all k means

                int newClusterID = MinDistanceIndex(distances); // find closest mean ID
                if (newClusterID != newClustering[i])
                {
                    changed = true;
                    newClustering[i] = newClusterID; // the proposed clustering is different for at least one item
                }
                ++clusterCounts[newClusterID];
            }

            if (changed == false)
                return false; // no change to clustering -- clustering has converged

            for (int k = 0; k < numberOfClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false; // no change to clustering because would have an empty cluster

            Array.Copy(newClustering, clustering, newClustering.Length); // there was a change and no empty clusters so update clustering
            
            return true; // successful change to clustering so keep looping
        }
    
    }
}
