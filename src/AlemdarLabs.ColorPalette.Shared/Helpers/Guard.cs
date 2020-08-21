using System;

namespace AlemdarLabs.ColorPalette.Helpers
{
    public static class Guard
    {
        /// <summary>
        /// Checks if an argument is null
        /// </summary>
        /// <param name="argument">argument</param>
        /// <param name="argumentName">argument name</param>
        public static void CheckNull(object argument, string argumentName)
        {
            if (argument == null)
            {
                string message = $"Cannot use '{argumentName}' when it is null!";
                throw new ArgumentNullException(message);
            }
        }
    }
}
