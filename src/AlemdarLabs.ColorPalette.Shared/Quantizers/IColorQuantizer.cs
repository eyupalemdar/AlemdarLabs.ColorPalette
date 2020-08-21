using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace AlemdarLabs.ColorPalette.Quantizers
{
    /// <summary>
    /// This interface provides a color quantization capabilities.
    /// </summary>
    public interface IColorQuantizer : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether to allow parallel processing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if to allow parallel processing; otherwise, <c>false</c>.
        /// </value>
        public bool AllowParallel { get; }

        /// <summary>
        /// Get palette of the specified size.
        /// </summary>
        /// 
        /// <param name="colorCount">Palette size to return.</param>
        /// 
        /// <returns>Returns reduced color palette for the accumulated/processed colors.</returns>
        /// 
        /// <remarks><para>The method must be called after continuously calling <see cref="AddColor"/> method and
        /// returns reduced color palette for colors accumulated/processed so far.</para></remarks>
        Task<List<Color>> QuantizeAsync(int colorCount);

        /// <summary>
        /// Get palette of the specified size.
        /// </summary>
        /// 
        /// <param name="colorCount">Palette size to return.</param>
        /// 
        /// <returns>Returns reduced color palette for the accumulated/processed colors.</returns>
        /// 
        /// <remarks><para>The method must be called after continuously calling <see cref="AddColor"/> method and
        /// returns reduced color palette for colors accumulated/processed so far.</para></remarks>
        ///
        List<Color> Quantize(int colorCount);
    }
}
