using AlemdarLabs.ColorPalette.Quantizers.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AlemdarLabs.ColorPalette.Quantizers
{
    public abstract class BaseColorQuantizer : IColorQuantizer
    {
        protected readonly ConcurrentDictionary<int, short> UniqueColors;
        public bool AllowParallel { get; }
        private IPathProvider pathProvider;
        private long uniqueColorIndex;
        private bool paletteFound;

        protected BaseColorQuantizer(bool allowParallel = false)
        {
            UniqueColors = new ConcurrentDictionary<int, short>();
            AllowParallel = allowParallel;
            pathProvider = null;
            uniqueColorIndex = -1;
            paletteFound = false;
        }

        public virtual async Task<List<Color>> QuantizeAsync(int colorCount)
        {
            return await Task.FromResult<List<Color>>(null);
        }

        public virtual List<Color> Quantize(int colorCount)
        {
            return null;
        }

        /// <summary>
        /// See <see cref="IPathProvider.GetPointPath"/> for more details.
        /// </summary>
        protected IList<Point> GetPointPath(int width, int heigth)
        {
            return GetPathProvider().GetPointPath(width, heigth);
        }

        /// <summary>
        /// See <see cref="IColorQuantizer.AddColor"/> for more details.
        /// </summary>
        protected void AddColor(Color color, int x, int y)
        {
            color = QuantizationHelper.ConvertAlpha(color, out int key);
            OnAddColor(color, key, x, y);
        }

        /// <summary>
        /// Called when color is to be added.
        /// </summary>
        protected virtual void OnAddColor(Color color, int key, int x, int y)
        {
            UniqueColors.AddOrUpdate(key,
                colorKey => (byte)Interlocked.Increment(ref uniqueColorIndex),
                (colorKey, colorIndex) => colorIndex);
        }

        /// <summary>
        /// Find most variant color in pixel color list
        /// </summary>
        /// <param name="bucketOfPixelColors"></param>
        /// <returns></returns>
        public virtual Color FindDominantColor(List<Color> bucketOfPixelColors)
        {
            int max = int.MinValue;
            int maxVarianceIndex = 0;

            for (int index = 0; index < bucketOfPixelColors.Count; index++)
            {
                var color = bucketOfPixelColors[index];

                // Remap each RGB value to a variance by taking the max component 
                // from the min component.
                var variance =
                    Math.Max(Math.Max(color.R, color.G), color.B) -
                    Math.Min(Math.Min(color.R, color.G), color.B);

                // Then step through each value and 
                // find which has the largest value.
                if (variance > max)
                {
                    maxVarianceIndex = index;
                    max = variance;
                }
            }

            return bucketOfPixelColors[maxVarianceIndex];
        }


        /// <summary>
        /// Redirection to retrieve palette to be cached, if palette is not available yet.
        /// </summary>
        protected abstract List<Color> GetQuantizerPalette(int colorCount);

        /// <summary>
        /// Called when quantized palette is needed.
        /// </summary>
        protected virtual List<Color> GetDefaultPalette(Int32 colorCount)
        {
            // early optimalization, in case the color count is lower than total unique color count
            if (UniqueColors.Count > 0 && colorCount >= UniqueColors.Count)
            {
                // palette was found
                paletteFound = true;

                // generates the palette from unique numbers
                return UniqueColors.
                    OrderBy(pair => pair.Value).
                    Select(pair => Color.FromArgb(pair.Key)).
                    Select(color => Color.FromArgb(255, color.R, color.G, color.B)).
                    ToList();
            }

            // otherwise make it descendant responsibility
            return null;
        }

        public virtual List<Color> GetPalette(int colorCount)
        {
            var palette = GetQuantizerPalette(colorCount) ?? GetDefaultPalette(colorCount);
            return palette;
        }

        /// <summary>
        /// Clear internals of the algorithm, like accumulated color table, etc.
        /// </summary>
        /// 
        /// <remarks><para>The methods resets internal state of a color quantization algorithm returning
        /// it to initial state.</para></remarks>
        ///
        public virtual void Dispose()
        {

        }

        #region Helper Methods

        private IPathProvider GetPathProvider()
        {
            // if there is no path provider, it attempts to create a default one; integrated in the quantizer
            pathProvider = pathProvider ?? (pathProvider = CreateDefaultPathProvider());

            // if the provider exists; or default one was created for these purposes.. use it
            if (pathProvider == null)
            {
                string message = $"The path provider is not initialized! Please use SetPathProvider() method on quantizer.";
                throw new ArgumentNullException(message);
            }

            // provider was obtained somehow, use it
            return pathProvider;
        }

        /// <summary>
        /// Called when a need to create default path provider arisen.
        /// </summary>
        protected virtual IPathProvider CreateDefaultPathProvider()
        {
            return new StandardPathProvider();
        }

        #endregion Helper Methods

    }
}
