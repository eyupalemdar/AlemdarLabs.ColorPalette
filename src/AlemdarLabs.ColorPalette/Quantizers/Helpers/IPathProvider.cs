﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace AlemdarLabs.ColorPalette.Quantizers.Helpers
{
    public interface IPathProvider
    {
        /// <summary>
        /// Retrieves the path throughout the image to determine the order in which pixels will be scanned.
        /// </summary>
        IList<Point> GetPointPath(int width, int height);
    }
}
