namespace AlemdarLabs.ColorPalette.Quantizers.Helpers
{
    public class TaskRange
    {
        /// <summary>
        /// Gets or sets the start offset.
        /// </summary>
        public int StartOffset { get; private set; }

        /// <summary>
        /// Gets or sets the end offset.
        /// </summary>
        public int EndOffset { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskRange"/> class.
        /// </summary>
        public TaskRange(int startOffset, int endOffset)
        {
            StartOffset = startOffset;
            EndOffset = endOffset;
        }
    }
}
