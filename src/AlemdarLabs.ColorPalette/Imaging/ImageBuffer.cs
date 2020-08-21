using AlemdarLabs.ColorPalette.Extensions;
using AlemdarLabs.ColorPalette.Quantizers.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace AlemdarLabs.ColorPalette.Imaging
{
    public class ImageBuffer : IDisposable
    {
        private readonly Bitmap Bitmap;
        private readonly BitmapData BitmapData;
        private readonly ImageLockMode ImageLockMode;
        private int[] fastBitX;
        private int[] fastByteX;
        private int[] fastY;
        private List<Color> cachedPalette;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public int Size { get; private set; }
        public int Stride { get; private set; }
        public int BitDepth { get; private set; }
        public int BytesPerPixel { get; private set; }

        public bool IsIndexed { get; private set; }
        public PixelFormat PixelFormat { get; private set; }

        public ImageBuffer(Bitmap bitmap, ImageLockMode imageLockMode)
        {
            // locks the image data
            Bitmap = bitmap;
            ImageLockMode = imageLockMode;

            // gathers the informations
            Width = bitmap.Width;
            Height = bitmap.Height;
            PixelFormat = bitmap.PixelFormat;
            IsIndexed = PixelFormat.IsIndexed();
            BitDepth = PixelFormat.GetBitDepth();
            BytesPerPixel = Math.Max(1, BitDepth >> 3);

            // determines the bounds of an image, and locks the data in a specified mode
            Rectangle bounds = Rectangle.FromLTRB(0, 0, Width, Height);

            // locks the bitmap data
            lock (Bitmap) BitmapData = Bitmap.LockBits(bounds, ImageLockMode, PixelFormat);

            // creates internal buffer
            Stride = Math.Abs(BitmapData.Stride);
            Size = Stride * Height;

            // precalculates the offsets
            Precalculate();
        }

        public List<Color> GetPalette()
        {
            if (cachedPalette == null)
            {
                cachedPalette = Bitmap.GetPalette();
            }
            return cachedPalette;
        }

        private void Precalculate()
        {
            fastBitX = new int[Width];
            fastByteX = new int[Width];
            fastY = new int[Height];

            // precalculates the x-coordinates
            for (int x = 0; x < Width; x++)
            {
                fastBitX[x] = x * BitDepth;
                fastByteX[x] = fastBitX[x] >> 3;
                fastBitX[x] = fastBitX[x] % 8;
            }

            // precalculates the y-coordinates
            for (int y = 0; y < Height; y++)
            {
                fastY[y] = y * BitmapData.Stride;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this buffer can be read.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can read; otherwise, <c>false</c>.
        /// </value>
        public bool CanRead
        {
            get { return ImageLockMode == ImageLockMode.ReadOnly || ImageLockMode == ImageLockMode.ReadWrite; }
        }

        /// <summary>
        /// Gets a value indicating whether this buffer can written to.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can write; otherwise, <c>false</c>.
        /// </value>
        public bool CanWrite
        {
            get { return ImageLockMode == ImageLockMode.WriteOnly || ImageLockMode == ImageLockMode.ReadWrite; }
        }

        public int GetBitOffset(int x)
        {
            return fastBitX[x];
        }

        public void ReadPixel(Pixel pixel, byte[] buffer = null)
        {
            // determines pixel offset at [x, y]
            int offset = fastByteX[pixel.X] + fastY[pixel.Y];

            // reads the pixel from a bitmap
            if (buffer == null)
            {
                pixel.ReadRawData(BitmapData.Scan0 + offset);
            }
            else // reads the pixel from a buffer
            {
                pixel.ReadData(buffer, offset, BytesPerPixel);
            }
        }

        public Color GetPaletteColor(int paletteIndex)
        {
            return cachedPalette[paletteIndex];
        }

        public List<Color> UpdatePalette(Boolean forceUpdate = false)
        {
            if (IsIndexed && (cachedPalette == null || forceUpdate))
            {
                cachedPalette = Bitmap.GetPalette();
            }

            return cachedPalette;
        }

        public void Dispose()
        {
            // releases the image lock
            lock (Bitmap) Bitmap.UnlockBits(BitmapData);
        }
    }
}
