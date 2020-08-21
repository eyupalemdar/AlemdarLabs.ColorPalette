using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AlemdarLabs.ColorPalette.Quantizers.MedianCut
{
    public class MedianCutCube
    {
        private readonly ICollection<int> ColorList;
        // red bounds
        private int redLowBound;
        private int redHighBound;

        // green bounds
        private int greenLowBound;
        private int greenHighBound;

        // blue bounds
        private int blueLowBound;
        private int blueHighBound;

        /// <summary>
        /// Gets or sets the index of the palette.
        /// </summary>
        /// <value>The index of the palette.</value>
        public int PaletteIndex { get; private set; }

        public MedianCutCube(ICollection<int> colorList)
        {
            ColorList = colorList;
            Shrink();
        }

        /// <summary>
        /// Shrinks this cube to the least dimensions that covers all the colors in the RGB space.
        /// </summary>
        private void Shrink()
        {
            redLowBound = greenLowBound = blueLowBound = 255;
            redHighBound = greenHighBound = blueHighBound = 0;

            for (int i = 0; i < ColorList.Count; i++)
            {
                Color color = Color.FromArgb(ColorList.ElementAt(i));

                if (color.R < redLowBound) redLowBound = color.R;
                if (color.R > redHighBound) redHighBound = color.R;
                if (color.G < greenLowBound) greenLowBound = color.G;
                if (color.G > greenHighBound) greenHighBound = color.G;
                if (color.B < blueLowBound) blueLowBound = color.B;
                if (color.B > blueHighBound) blueHighBound = color.B;
            }
        }

        /// <summary>
        /// Gets the size of the red side of this cube.
        /// </summary>
        /// <value>The size of the red side of this cube.</value>
        public int RedSize
        {
            get { return redHighBound - redLowBound; }
        }

        /// <summary>
        /// Gets the size of the green side of this cube.
        /// </summary>
        /// <value>The size of the green side of this cube.</value>
        public int GreenSize
        {
            get { return greenHighBound - greenLowBound; }
        }

        /// <summary>
        /// Gets the size of the blue side of this cube.
        /// </summary>
        /// <value>The size of the blue side of this cube.</value>
        public int BlueSize
        {
            get { return blueHighBound - blueLowBound; }
        }

        /// <summary>
        /// Gets the average color from the colors contained in this cube.
        /// </summary>
        /// <value>The average color.</value>
        public Color Color
        {
            get
            {
                int red = 0, green = 0, blue = 0;

                foreach (int argb in ColorList)
                {
                    Color color = Color.FromArgb(argb);
                    red += color.R;
                    green += color.G;
                    blue += color.B;
                }

                red = ColorList.Count == 0 ? 0 : red / ColorList.Count;
                green = ColorList.Count == 0 ? 0 : green / ColorList.Count;
                blue = ColorList.Count == 0 ? 0 : blue / ColorList.Count;

                // ColorModelHelper.HSBtoRGB(Convert.ToInt32(red/ColorModelHelper.HueFactor), green / 255.0f, blue / 255.0f);

                Color result = Color.FromArgb(255, red, green, blue);
                return result;
            }
        }

        /// <summary>
        /// Splits this cube's color list at median index, and returns two newly created cubes.
        /// </summary>
        /// <param name="componentIndex">Index of the component (red = 0, green = 1, blue = 2).</param>
        /// <param name="firstMedianCutCube">The first created cube.</param>
        /// <param name="secondMedianCutCube">The second created cube.</param>
        public void SplitAtMedian(byte componentIndex, out MedianCutCube firstMedianCutCube, out MedianCutCube secondMedianCutCube)
        {
            List<int> colors = componentIndex switch
            {
                // red colors
                0 => ColorList.OrderBy(argb => Color.FromArgb(argb).R).ToList(),
                // green colors
                1 => ColorList.OrderBy(argb => Color.FromArgb(argb).G).ToList(),
                // blue colors
                2 => ColorList.OrderBy(argb => Color.FromArgb(argb).B).ToList(),
                _ => throw new NotSupportedException("Only three color components are supported (R, G and B)."),
            };

            // retrieves the median index (a half point)
            int medianIndex = ColorList.Count >> 1;

            // creates the two half-cubes
            firstMedianCutCube = new MedianCutCube(colors.GetRange(0, medianIndex));
            secondMedianCutCube = new MedianCutCube(colors.GetRange(medianIndex, colors.Count - medianIndex));
        }

        /// <summary>
        /// Assigns a palette index to this cube, to be later found by a GetPaletteIndex method.
        /// </summary>
        /// <param name="newPaletteIndex">The palette index to be assigned to this cube.</param>
        public void SetPaletteIndex(Int32 newPaletteIndex)
        {
            PaletteIndex = newPaletteIndex;
        }
    }
}
