using AlemdarLabs.ColorPalette.Extensions;
using AlemdarLabs.ColorPalette.Helpers;
using AlemdarLabs.ColorPalette.Imaging;
using AlemdarLabs.ColorPalette.Qauntizers;
using AlemdarLabs.ColorPalette.Quantizers.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace AlemdarLabs.ColorPalette.Quantizers.MedianCut
{
    /// <summary>
    /// Median cut color quantization algorithm.
    /// </summary>
    /// 
    /// <remarks><para>The class implements <a href="http://en.wikipedia.org/wiki/Median_cut">median cut</a>
    /// <a href="http://en.wikipedia.org/wiki/Median_cut">color quantization</a> algorithm.</para>
    /// </remarks>
    /// 
    /// <seealso cref="ColorImageQuantizer"/>
    /// 
    public class MedianCutQuantizer : BaseColorQuantizer
    {
        private readonly Bitmap SourceBitmap;
        private ConcurrentBag<MedianCutCube> cubeList;

        public MedianCutQuantizer(Bitmap sourceBitmap) : base(true)
        {
            SourceBitmap = sourceBitmap;
            cubeList = new ConcurrentBag<MedianCutCube>();
        }

        public override async Task<List<Color>> QuantizeAsync(int colorCount)
        {
            int parallelTaskCount = AllowParallel ? 4 : 1;
            List<Color> targetPalette = null;

            // quantization process
            return await Task.Factory.StartNew(() =>
                targetPalette = Quantize(colorCount, parallelTaskCount),
                TaskCreationOptions.LongRunning);

            // TODO: return palette
            //return quantization;
        }
        private List<Color> Quantize(int colorCount, int parallelTaskCount)
        {
            // checks parameters
            Guard.CheckNull(SourceBitmap, "SourceBitmap");

            using (ImageBuffer source = new ImageBuffer(SourceBitmap, ImageLockMode.ReadOnly))
            {
                // creates a target bitmap in an appropriate format
                PixelFormat targetPixelFormat = PixelFormatExtensions.GetFormatByColorCount(colorCount);
                var targetBitmap = new Bitmap(SourceBitmap.Width, SourceBitmap.Height, targetPixelFormat);

                // wraps source image to a buffer
                using (ImageBuffer target = new ImageBuffer(targetBitmap, ImageLockMode.WriteOnly))
                {
                    return Quantize(source, target, colorCount, parallelTaskCount);
                }
            }
        }
        private List<Color> Quantize(ImageBuffer source, ImageBuffer target, int colorCount, int parallelTaskCount)
        {
            // checks parameters
            Guard.CheckNull(source, "source");
            Guard.CheckNull(target, "target");

            // initializes quantization parameters
            bool isTargetIndexed = target.PixelFormat.IsIndexed();

            // Step 1 - scans the source image for the colors
            ScanColors(source, target, parallelTaskCount);

            // Step 2 - generate the palette, and returns the result
            return GetPalette(colorCount);
        }

        private void ScanColors(ImageBuffer source, ImageBuffer target, int parallelTaskCount = 4)
        {
            // determines which method of color retrieval to use
            IList<Point> path = GetPointPath(target.Width, target.Height);
            Guard.CheckNull(path, "path");

            // use different scanning method depending whether the image format is indexed
            //ProcessPixelFunction scanColors = pixel =>
            //{
            //    AddColor(GetColorFromPixel(pixel), pixel.X, pixel.Y);
            //    return false;
            //};

            // prepares the per pixel task
            void processPerPixel(TaskRange lineTask)
            {
                // initializes variables per task
                Pixel pixel = new Pixel(source.PixelFormat, source.IsIndexed);

                for (int pathOffset = lineTask.StartOffset; pathOffset < lineTask.EndOffset; pathOffset++)
                {
                    Point point = path[pathOffset];

                    // enumerates the pixel, and returns the control to the outside
                    pixel.Update(point.X, point.Y, source.GetBitOffset(point.X));

                    // when read is allowed, retrieves current value (in bytes)
                    if (source.CanRead)
                    {
                        source.ReadPixel(pixel);
                    }

                    AddColor(GetColorFromPixel(pixel, source), pixel.X, pixel.Y);

                    //// process the pixel by custom user operation
                    //allowWrite = scanColors(pixel);

                    //// when write is allowed, copies the value back to the row buffer
                    //if (source.CanWrite && allowWrite)
                    //{
                    //    WritePixel(pixel);
                    //}
                }
            }

            // updates the palette
            source.UpdatePalette();

            // prepares parallel processing
            double pointsPerTask = (1.0 * path.Count) / parallelTaskCount;
            TaskRange[] lineTasks = new TaskRange[parallelTaskCount];
            double pointOffset = 0.0;

            // creates task for each batch of rows
            for (int index = 0; index < parallelTaskCount; index++)
            {
                lineTasks[index] = new TaskRange((int)pointOffset, (int)(pointOffset + pointsPerTask));
                pointOffset += pointsPerTask;
            }

            // process the image in a parallel manner
            Parallel.ForEach(lineTasks, processPerPixel);
        }
        public Color GetColorFromPixel(Pixel pixel, ImageBuffer source)
        {
            Color result;

            // determines whether the format is indexed
            if (pixel.IsIndexed)
            {
                result = source.GetPaletteColor(pixel.Index);
            }
            else // gets color from a non-indexed format
            {
                result = pixel.Color;
            }

            // returns the found color
            return result;
        }

        /// <summary>
        /// Clear internals of the algorithm, like accumulated color table, etc.
        /// </summary>
        /// 
        /// <remarks><para>The methods resets internal state of a color quantization algorithm returning
        /// it to initial state.</para></remarks>
        /// 
        public override void Dispose()
        {
            base.Dispose();
        }

        protected override List<Color> GetQuantizerPalette(int colorCount)
        {
            // creates the initial cube covering all the pixels in the image
            MedianCutCube initalMedianCutCube = new MedianCutCube(UniqueColors.Keys);
            cubeList.Add(initalMedianCutCube);

            // finds the minimum iterations needed to achieve the cube count (color count) we need
            int iterationCount = 1;
            while ((1 << iterationCount) < colorCount) { iterationCount++; }

            for (int iteration = 0; iteration < iterationCount; iteration++)
            {
                SplitCubes(colorCount);
            }

            // initializes the result palette
            List<Color> colors = new List<Color>();
            int paletteIndex = 0;

            // sort color by luminance and get results
            var orderedCubeList = cubeList
                .OrderBy(cube => cube.Color.GetBrightness())
                .Take(colorCount);

            // adds all the cubes' colors to the palette, and mark that cube with palette index for later use
            foreach (MedianCutCube cube in orderedCubeList)
            {
                colors.Add(cube.Color);
                cube.SetPaletteIndex(paletteIndex++);
            }

            // returns the palette (should contain <= ColorCount colors)
            return colors;
        }

        /// <summary>
        /// Splits all the cubes on the list.
        /// </summary>
        /// <param name="colorCount">The color count.</param>
        private void SplitCubes(int colorCount)
        {
            // creates a holder for newly added cubes
            List<MedianCutCube> newCubes = new List<MedianCutCube>();

            foreach (MedianCutCube cube in cubeList)
            {
                // if another new cubes should be over the top; don't do it and just stop here
                //if (newCubes.Count >= colorCount) break;

                MedianCutCube newMedianCutCubeA, newMedianCutCubeB;

                // splits the cube along the red axis
                if (cube.RedSize >= cube.GreenSize && cube.RedSize >= cube.BlueSize)
                {
                    cube.SplitAtMedian(0, out newMedianCutCubeA, out newMedianCutCubeB);
                }
                else if (cube.GreenSize >= cube.BlueSize) // splits the cube along the green axis
                {
                    cube.SplitAtMedian(1, out newMedianCutCubeA, out newMedianCutCubeB);
                }
                else // splits the cube along the blue axis
                {
                    cube.SplitAtMedian(2, out newMedianCutCubeA, out newMedianCutCubeB);
                }

                // adds newly created cubes to our list; but one by one and if there's enough cubes stops the process
                newCubes.Add(newMedianCutCubeA);
                //if (newCubes.Count >= colorCount) break;
                newCubes.Add(newMedianCutCubeB);
            }

            // clears the old cubes
            cubeList = new ConcurrentBag<MedianCutCube>();

            // adds the new cubes to the official cube list
            foreach (MedianCutCube medianCutCube in newCubes)
            {
                cubeList.Add(medianCutCube);
            }
        }

    }
}
