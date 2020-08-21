using System.Collections.Generic;
using System.Drawing;

namespace AlemdarLabs.ColorPalette.Qauntizers
{
    public class RedComparer : IComparer<Color>
    {
        public int Compare(Color c1, Color c2)
        {
            return c1.R.CompareTo(c2.R);
        }
    }

    public class GreenComparer : IComparer<Color>
    {
        public int Compare(Color c1, Color c2)
        {
            return c1.G.CompareTo(c2.G);
        }
    }

    public class BlueComparer : IComparer<Color>
    {
        public int Compare(Color c1, Color c2)
        {
            return c1.B.CompareTo(c2.B);
        }
    }

    public class LuminanceComparer : IComparer<Color>
    {
        public int Compare(Color c1, Color c2)
        {
            return c1.GetBrightness().CompareTo(c2.GetBrightness());
        }
    }
}
