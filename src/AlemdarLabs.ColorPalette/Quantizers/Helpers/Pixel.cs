using AlemdarLabs.ColorPalette.Quantizers.Helpers.Pixels;
using AlemdarLabs.ColorPalette.Quantizers.Helpers.Pixels.Indexed;
using AlemdarLabs.ColorPalette.Quantizers.Helpers.Pixels.NonIndexed;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace AlemdarLabs.ColorPalette.Quantizers.Helpers
{
    /// <summary>
    /// This is a pixel format independent pixel.
    /// </summary>
    public class Pixel : IDisposable
    {
        #region Constants

        internal const byte Zero = 0;
        internal const byte One = 1;
        internal const byte Two = 2;
        internal const byte Four = 4;
        internal const byte Eight = 8;

        internal const byte NibbleMask = 0xF;
        internal const byte ByteMask = 0xFF;

        internal const int AlphaShift = 24;
        internal const int RedShift = 16;
        internal const int GreenShift = 8;
        internal const int BlueShift = 0;
                       
        internal const int AlphaMask = ByteMask << AlphaShift;
        internal const int RedGreenBlueMask = 0xFFFFFF;

        #endregion Constants

        private object pixelData;
        private IntPtr pixelDataPointer;
        
        private readonly PixelFormat PixelFormat;
        private readonly Type PixelType;

        public int X { get; private set; }
        public int Y { get; private set; }
        public int BitOffset { get; private set; }
        public bool IsIndexed { get; private set; }

        public Pixel(PixelFormat pixelFormat, bool isIndexed)
        {
            PixelFormat = pixelFormat;
            IsIndexed = isIndexed;
            PixelType = IsIndexed ? GetIndexedType(PixelFormat) : GetNonIndexedType(PixelFormat);

            Initialize();
        }

        public void Update(int x, int y, int bitOffset)
        {
            X = x;
            Y = y;
            BitOffset = bitOffset;
        }

        /// <summary>
        /// Reads the raw data.
        /// </summary>
        /// <param name="imagePointer">The image pointer.</param>
        public void ReadRawData(IntPtr imagePointer)
        {
            pixelData = Marshal.PtrToStructure(imagePointer, PixelType);
        }

        /// <summary>
        /// Reads the data.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        public void ReadData(byte[] buffer, int offset, int bytesPerPixel)
        {
            Marshal.Copy(buffer, offset, pixelDataPointer, bytesPerPixel);
            pixelData = Marshal.PtrToStructure(pixelDataPointer, PixelType);
        }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>The index.</value>
        public byte Index
        {
            get { return ((IIndexedPixel)pixelData).GetIndex(BitOffset); }
            set { ((IIndexedPixel)pixelData).SetIndex(BitOffset, value); }
        }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        public Color Color
        {
            get { return ((INonIndexedPixel)pixelData).GetColor(); }
            set { ((INonIndexedPixel)pixelData).SetColor(value); }
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(pixelDataPointer);
        }

        #region Helper Methods
        private void Initialize()
        {
            // creates pixel data
            NewExpression newType = Expression.New(PixelType);
            UnaryExpression convertNewType = Expression.Convert(newType, typeof(Object));
            Expression<Func<object>> indexedExpression = Expression.Lambda<Func<Object>>(convertNewType);
            pixelData = indexedExpression.Compile().Invoke();
            pixelDataPointer = MarshalToPointer(pixelData);
        }

        /// <summary>
        /// Gets the type of the indexed pixel format.
        /// </summary>
        internal Type GetIndexedType(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format1bppIndexed: return typeof(PixelData1Indexed);
                case PixelFormat.Format4bppIndexed: return typeof(PixelData4Indexed);
                case PixelFormat.Format8bppIndexed: return typeof(PixelData8Indexed);

                default:
                    string message = $"This pixel format '{pixelFormat}' is either non-indexed, or not supported.";
                    throw new NotSupportedException(message);
            }
        }

        /// <summary>
        /// Gets the type of the non-indexed pixel format.
        /// </summary>
        internal Type GetNonIndexedType(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format16bppArgb1555: return typeof(PixelDataArgb1555);
                case PixelFormat.Format16bppGrayScale: return typeof(PixelDataGray16);
                case PixelFormat.Format16bppRgb555: return typeof(PixelDataRgb555);
                case PixelFormat.Format16bppRgb565: return typeof(PixelDataRgb565);
                case PixelFormat.Format24bppRgb: return typeof(PixelDataRgb888);
                case PixelFormat.Format32bppRgb: return typeof(PixelDataRgb8888);
                case PixelFormat.Format32bppArgb: return typeof(PixelDataArgb8888);
                case PixelFormat.Format48bppRgb: return typeof(PixelDataRgb48);
                case PixelFormat.Format64bppArgb: return typeof(PixelDataArgb64);

                default:
                    string message = $"This pixel format '{pixelFormat}' is either indexed, or not supported.";
                    throw new NotSupportedException(message);
            }
        }

        private IntPtr MarshalToPointer(object data)
        {
            int size = Marshal.SizeOf(data);
            IntPtr pointer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(data, pointer, false);
            return pointer;
        }
        #endregion Helper Methods

    }
}
