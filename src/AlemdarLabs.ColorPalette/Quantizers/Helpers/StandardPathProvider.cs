using System.Collections.Generic;
using System.Drawing;

namespace AlemdarLabs.ColorPalette.Quantizers.Helpers
{
    public class StandardPathProvider : IPathProvider
    {
        public IList<Point> GetPointPath(int width, int height)
        {
            List<Point> result = new List<Point>(width * height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Point point = new Point(x, y);
                    result.Add(point);
                }

            return result;
        }
    }
}
