using System;
using System.Collections.Generic;
using System.Drawing;

namespace AlemdarLabs.ColorPalette.ColorReduction
{
    /// <summary>
    /// * K-means Clustering is the most simplest Unsupervised Learning Algorithm
    /// * K-means clustering aims to partition n observations into k clusters
    /// * Used to cluster observations into groups of related observations without any prior knowledge of those relationships
    /// </summary>
    public class KMeansQuantizer : BaseColorQuantizer
    {
        public KMeansQuantizer(double[][] pixelColors)
        {
            kMeans = new KMeans();
            this.pixelColors = pixelColors;
        }

        private readonly KMeans kMeans;
        private readonly double[][] pixelColors;

        public override List<Color> GetPalette(int colorCount)
        {
            // Don't allow large values to dominate
            var normalizedPixelColors = kMeans.Normalize(pixelColors, NormalizationMethod.Color);
            kMeans.Cluster(normalizedPixelColors, colorCount);
            return ConvertRawDataToColors(kMeans.ClusterCenters);
        }

        #region Helper Methods
        private List<Color> ConvertRawDataToColors(double[][] rawData)
        {
            var colors = new List<Color>(rawData.Length);
            
            for (int i = 0; i < rawData.Length; i++)
            {
                var currentData = rawData[i];
                colors.Insert(i, Color.FromArgb(
                    Convert.ToInt32(currentData[0] * KMeans.ColorNormalizationDivider),
                    Convert.ToInt32(currentData[1] * KMeans.ColorNormalizationDivider),
                    Convert.ToInt32(currentData[2] * KMeans.ColorNormalizationDivider)));
            }

            return colors;
        }
        #endregion Helper Methods
    }
}
