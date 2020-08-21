using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AlemdarLabs.ColorPalette.Extensions
{
    public static class ImageExtensions
    {
        /// <summary>
        /// Gets the palette of an indexed image.
        /// </summary>
        /// <param name="image">The source image.</param>
        public static List<Color> GetPalette(this Image image)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                string message = "Cannot assign a palette to a null image.";
                throw new ArgumentNullException(message);
            }

            // checks if the image has an indexed format
            if (!image.PixelFormat.IsIndexed())
            {
                string message = $"Cannot retrieve a palette from a non-indexed image with pixel format '{image.PixelFormat}'.";
                throw new InvalidOperationException(message);
            }

            // retrieves and returns the palette
            return image.Palette.Entries.ToList();
        }
    }
}
