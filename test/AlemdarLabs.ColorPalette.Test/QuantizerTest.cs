using AlemdarLabs.ColorPalette.Imaging;
using AlemdarLabs.ColorPalette.Quantizers;
using AlemdarLabs.ColorPalette.Quantizers.MedianCut;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Xunit;

namespace AlemdarLabs.ColorPalette.Test
{
    public class QuantizerTest
    {
        [Fact(DisplayName = "Load image and run median cut algorithm as color quantizer against it")]
        public void MedianCutQuantizerTest()
        {
            var inputPath = "Resources/Images/image-1.jpg";

            using var image = new Bitmap(Image.FromFile(inputPath));
            //List<Color> pixelColors = new List<Color>(image.Width * image.Height);
            //int index = 0;

            //for (int width = 0; width < image.Width; width++)
            //{
            //    for (int height = 0; height < image.Height; height++)
            //    {
            //        pixelColors.Insert(index, image.GetPixel(width, height));
            //        ++index;
            //    }
            //}

            using MedianCutQuantizer colorQuantizer = new MedianCutQuantizer(image);
            var palette = colorQuantizer.QuantizeAsync(10).Result;
            //var dominantColor = colorQuantizer.FindDominantColor(palette);
        }

        //[Fact(DisplayName = "Test K-Means algorithm with sample data")]
        //public void KMeanAlgorithmTest()
        //{
        //    Trace.WriteLine("\nBegin k-means clustering demo\n");

        //    double[][] rawData = Seeder.GetSampleData();

        //    Trace.WriteLine("Raw unclustered data:\n");
        //    Trace.WriteLine("    Height Weight");
        //    Trace.WriteLine("-------------------");
        //    ShowData(rawData, 1, true, true);

        //    KMeans kMeans = new KMeans();

        //    int numClusters = 3;
        //    Trace.WriteLine("\nSetting numClusters to " + numClusters);

        //    var normalizedData = kMeans.Normalize(rawData);
        //    kMeans.Cluster(normalizedData, numClusters); // this is it
        //    int[] clustering = kMeans.Labels;

        //    Trace.WriteLine("\nK-means clustering complete\n");

        //    Trace.WriteLine("Final clustering in internal form:\n");
        //    ShowVector(clustering, true);

        //    Trace.WriteLine("Raw data by cluster:\n");
        //    ShowClustered(rawData, clustering, numClusters, 1);

        //    Trace.WriteLine("\nEnd k-means clustering demo\n");
        //}

        //[Fact(DisplayName = "Load image and run k-means algorithm as color quantizer against it")]
        //public void KMeansQuantizerTest()
        //{
        //    var inputPath = "Resources/Images/image-1.jpg";

        //    using var image = new Bitmap(Image.FromFile(inputPath));
        //    var pixelColors = ImageLoader.GetBitmapColorMatrix(image);

        //    using IColorQuantizer colorQuantizer = new KMeansQuantizer(pixelColors);
        //    var palette = colorQuantizer.GetPalette(10);
        //    var dominantColor = colorQuantizer.FindDominantColor(palette);
        //}

        [Fact(DisplayName = "Unsafe multithreaded image loading via potiners must be legit")]
        public void FastImageLoadTest()
        {
            var inputPath = "Resources/Images/image-1.jpg";

            using var image = new Bitmap(Image.FromFile(inputPath));
            var pixelColors = ImageLoader.GetBitmapColorMatrix(image);

            double[][] pixelColors2 = new double[image.Width * image.Height][];
            int index = 0;

            for (int height = 0; height < image.Height; height++)
            {
                for (int width = 0; width < image.Width; width++)
                {
                    var currentPixel = image.GetPixel(width, height);
                    pixelColors2[index] = new double[3] {
                        currentPixel.R,
                        currentPixel.G,
                        currentPixel.B
                    };
                    ++index;
                }
            }

            int maximumNumberOfAttempts = 50;
            Assert.True(pixelColors.Length > maximumNumberOfAttempts);

            Random random = new Random();
            for (int i = 1; i < maximumNumberOfAttempts; i++)
            {
                var currentIndex = random.Next(0, i);
                Assert.Equal(pixelColors[currentIndex], pixelColors2[currentIndex]);
            }
        }

        #region Helper Methods
        static void ShowData(double[][] data, int decimals, bool indices, bool newLine)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                if (indices) Trace.Write(i.ToString().PadLeft(3) + " ");
                for (int j = 0; j < data[i].Length; ++j)
                {
                    if (data[i][j] >= 0.0) Trace.Write(" ");
                    Trace.Write(data[i][j].ToString("F" + decimals) + " ");
                }
                Trace.WriteLine("");
            }
            if (newLine) Trace.WriteLine("");
        }

        static void ShowVector(int[] vector, bool newLine)
        {
            for (int i = 0; i < vector.Length; ++i)
                Trace.Write(vector[i] + " ");
            if (newLine) Trace.WriteLine("\n");
        }

        static void ShowClustered(double[][] data, int[] clustering, int numClusters, int decimals)
        {
            for (int k = 0; k < numClusters; ++k)
            {
                Trace.WriteLine("===================");
                for (int i = 0; i < data.Length; ++i)
                {
                    int clusterID = clustering[i];
                    if (clusterID != k) continue;
                    Trace.Write(i.ToString().PadLeft(3) + " ");
                    for (int j = 0; j < data[i].Length; ++j)
                    {
                        if (data[i][j] >= 0.0) Trace.Write(" ");
                        Trace.Write(data[i][j].ToString("F" + decimals) + " ");
                    }
                    Trace.WriteLine("");
                }
                Trace.WriteLine("===================");
            } // k
        }
        #endregion Helper Methods
    }
}
