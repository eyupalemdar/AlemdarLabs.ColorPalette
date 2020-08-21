using System;

namespace AlemdarLabs.ColorPalette.Qauntizers
{
    public partial class KMeans
    {
        #region Normalization Methods
        public const double ColorNormalizationDivider = 255;

        /// <summary>
        /// Convert to doubles instead of the default 32 bit byte coding. 
        /// Dividing by <see cref="NormalizeMean"/> 255 is important so that
        /// We can work well on data range between [0-1]
        /// </summary>
        /// <param name="mean"></param>
        public double[][] Normalize(double[][] rawData,
            NormalizationMethod normalizationMethod = NormalizationMethod.Gaussian)
        {
            double[][] normalizedData = normalizationMethod switch
            {
                NormalizationMethod.Color => NormalizeByMean(rawData, ColorNormalizationDivider),
                _ => GaussianNormalization(rawData),
            };
            return normalizedData;
        }

        /// <summary>
        /// /// Dividing by <see cref="NormalizeMean"/> 255 is important so that
        /// We can work well on data range between [0-1]
        /// </summary>
        /// <param name="rawData"></param>
        /// <param name="mean"></param>
        /// <returns></returns>
        private double[][] NormalizeByMean(double[][] rawData, double mean)
        {
            double[][] normalizedData = new double[rawData.Length][];
            for (int i = 0; i < rawData.Length; ++i)
            {
                normalizedData[i] = new double[rawData[i].Length];

                for (int j = 0; j < rawData[i].Length; j++)
                {
                    normalizedData[i][j] = rawData[i][j] / mean;
                }
            }
            return normalizedData;
        }

        /// <summary>
        /// Gaussian Normalization: 
        /// Each raw value v in a column is converted 
        /// to a normalized value v' by subtracting 
        /// the arithmetic mean of all the values in the column and 
        /// then dividing by the standard deviation of the values. 
        /// Normalized values will almost always be between -10 and +10. 
        /// For raw data -- which follows a bell-shaped distribution -- 
        /// most normalized values will be between -3 and +3
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        private double[][] GaussianNormalization(double[][] rawData)
        {
            double[][] normalizedData = new double[rawData.Length][];
            for (int i = 0; i < rawData.Length; ++i)
            {
                normalizedData[i] = new double[rawData[i].Length];
                Array.Copy(rawData[i], normalizedData[i], rawData[i].Length);
            }

            for (int j = 0; j < normalizedData[0].Length; ++j)
            {
                double columnSum = 0.0;
                for (int i = 0; i < normalizedData.Length; ++i)
                    columnSum += normalizedData[i][j];

                double mean = columnSum / normalizedData.Length;
                double sum = 0.0;
                for (int i = 0; i < normalizedData.Length; ++i)
                    sum += (normalizedData[i][j] - mean) * (normalizedData[i][j] - mean);

                double standardDeviation = sum / normalizedData.Length;
                for (int i = 0; i < normalizedData.Length; ++i)
                    normalizedData[i][j] = (normalizedData[i][j] - mean) / standardDeviation;
            }
            return normalizedData;
        }
        #endregion Normalization Methods

        #region Initialization Methods
        /// <summary>
        /// Initialize the clustering array by assigning each data tuple 
        /// to a randomly selected cluster ID. 
        /// The method arbitrarily assigns tuples 0, 1 and 2 to 
        /// clusters 0, 1 and 2, respectively, 
        /// so that each cluster is guaranteed to have 
        /// at least one data tuple assigned to it
        /// </summary>
        /// <param name="clustering"></param>
        /// <param name="numberOfClusters"></param>
        /// <returns></returns>
        private void RandomInit(int[] clustering, int numberOfClusters)
        {
            for (int i = 0; i < numberOfClusters; ++i)
                clustering[i] = i;
            for (int i = numberOfClusters; i < clustering.Length; ++i)
                clustering[i] = Random.Next(0, numberOfClusters);
        }

        private void KMeansPlusPlusInit(double[][] data,
              int[] clustering, double[][] means)
        {
            //  k-means++ init using roulette wheel selection
            // clustering[] and means[][] exist
            int dataCount = data.Length;
            int dimension = data[0].Length;
            int numberOfClusters = means.Length;

            // select one data item index at random as 1st meaan
            int idx = Random.Next(0, dataCount); // [0, N)
            for (int j = 0; j < dimension; ++j)
                means[0][j] = data[idx][j];

            for (int k = 1; k < numberOfClusters; ++k) // find each remaining mean
            {
                double[] dSquareds = new double[dataCount]; // from each item to its closest mean

                for (int i = 0; i < dataCount; ++i) // for each data item
                {
                    // compute distances from data[i] to each existing mean (to find closest)
                    double[] distances = new double[k]; // we currently have k means

                    for (int ki = 0; ki < k; ++ki)
                        distances[ki] = EuclideanDistance(data[i], means[ki]);

                    int mi = MinDistanceIndex(distances);  // index of closest mean to curr item
                                                 // save the associated distance-squared
                    dSquareds[i] = distances[mi] * distances[mi];  // sq dist from item to its closest mean
                } // i

                // select an item far from its mean using roulette wheel
                // if an item has been used as a mean its distance will be 0
                // so it won't be selected

                int newMeanIdx = ProportionalSelection(dSquareds);
                for (int j = 0; j < dimension; ++j)
                    means[k][j] = data[newMeanIdx][j];
            } // k remaining means

            UpdateClustering(data, clustering, means);
        }
        #endregion Initialization Methods

        #region Compute Within-Cluster Sum of Squares (WCSS)
        private double ComputeWithinClusterSS(double[][] data,
              double[][] means, int[] clustering)
        {
            // compute total within-cluster sum of squared differences between 
            // cluster items and their cluster means
            // this is actually the objective function, not distance
            double sum = 0.0;
            for (int i = 0; i < data.Length; ++i)
            {
                int cid = clustering[i];  // which cluster does data[i] belong to?
                sum += SumSquared(data[i], means[cid]);
            }
            return sum;
        }

        private double SumSquared(double[] items, double[] means)
        {
            // squared distance between vectors
            // surprisingly, k-means minimizes this, not distance
            double sum = 0.0;
            for (int j = 0; j < items.Length; ++j)
                sum += (items[j] - means[j]) * (items[j] - means[j]);
            return sum;
        }
        #endregion Compute Within-Cluster Sum of Squares (WCSS)

        #region Helper Methods
        private void Initialization(int numberOfTuples, int dimension, int numberOfClusters)
        {
            ClusterCenters = AllocateMatrix(numberOfClusters, dimension);
            Labels = new int[numberOfTuples];
        }

        /// <summary>
        /// Distance between a data tuple and a cluster mean.
        /// Euclidean distance between two vectors for UpdateClustering()
        /// consider alternatives such as Manhattan distance
        /// </summary>
        /// <param name="tuple"></param>
        /// <param name="mean"></param>
        /// <returns></returns>
        private double EuclideanDistance(double[] tuple, double[] mean)
        {
            double sum = 0.0;
            for (int j = 0; j < tuple.Length; ++j)
                sum += Math.Pow((tuple[j] - mean[j]), 2);
            return Math.Sqrt(sum);
        }

        /// <summary>
        /// Find index of minimum distance in distance array
        /// </summary>
        /// <param name="distances"></param>
        /// <returns></returns>
        private int MinDistanceIndex(double[] distances)
        {
            int indexOfMin = 0;
            double smallestDistance = distances[0];
            for (int k = 0; k < distances.Length; ++k)
            {
                if (distances[k] < smallestDistance)
                {
                    smallestDistance = distances[k];
                    indexOfMin = k;
                }
            }
            return indexOfMin;
        }

        /// <summary>
        /// Helper method for allocating means matrix
        /// </summary>
        /// <param name="numberOfRows"></param>
        /// <param name="numberOfColumns"></param>
        /// <returns></returns>
        private double[][] AllocateMatrix(int numberOfRows, int numberOfColumns)
        {
            double[][] matrix = new double[numberOfRows][];
            for (int k = 0; k < numberOfRows; ++k)
                matrix[k] = new double[numberOfColumns];
            return matrix;
        }

        private int ProportionalSelection(double[] values)
        {
            // roulette wheel proportional selection
            // on the fly technique
            // values[] can't be all 0.0s
            int n = values.Length;

            double sum = 0.0;
            for (int i = 0; i < n; ++i)
                sum += values[i];

            if (sum == 0)
            {
                throw new Exception("Proportional selection sum can not be zero.");
            }

            double cumP = 0.0;  // cumulative prob
            double p = Random.NextDouble();

            for (int i = 0; i < n; ++i)
            {
                cumP += (values[i] / sum);
                if (cumP > p) return i;
            }
            return n - 1;  // last index
        }

        #endregion Helper Methods
    }
}
