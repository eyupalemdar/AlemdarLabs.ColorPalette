using System.Drawing;

namespace AlemdarLabs.ColorPalette.Quantizers.Helpers.Pixels
{
    public interface INonIndexedPixel
    {
        // components
        int Alpha { get; }
        int Red { get; }
        int Green { get; }
        int Blue { get; }

        // higher-level values
        int Argb { get; }
        ulong Value { get; set; }

        // color methods
        Color GetColor();
        void SetColor(Color color);
    }
}
