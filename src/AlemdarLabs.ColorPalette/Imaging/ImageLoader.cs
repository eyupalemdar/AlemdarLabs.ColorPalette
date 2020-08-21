using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace AlemdarLabs.ColorPalette.Imaging
{
    public class ImageLoader
    {
        /// <summary>
        /// Resource: http://csharpexamples.com/fast-image-processing-c/
        /// </summary>
        /// <param name="processedBitmap"></param>
        /// <returns></returns>
        public static double[][] GetBitmapColorMatrix(Bitmap processedBitmap)
        {
            unsafe
            {
                BitmapData bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), ImageLockMode.ReadWrite, processedBitmap.PixelFormat);
                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;
                
                double[][] imageAsArray = new double[bitmapData.Height * bitmapData.Width][];

                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        double blue = currentLine[x];
                        double green = currentLine[x + 1];
                        double red = currentLine[x + 2];

                        int currentIndex = x / bytesPerPixel + y * bitmapData.Width;

                        imageAsArray[currentIndex] = new double[3] {
                            red,
                            green,
                            blue
                        };
                    }
                });


                //for (int y = 0; y < heightInPixels; y++)
                //{
                //    byte* currentLine = ptrFirstPixel + (y * Math.Abs(bitmapData.Stride));
                //    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                //    {
                //        double blue = currentLine[x];
                //        double green = currentLine[x + 1];
                //        double red = currentLine[x + 2];

                //        int currentIndex = x / bytesPerPixel + y * bitmapData.Width;

                //        imageAsArray[currentIndex] = new double[3] {
                //            red,
                //            green,
                //            blue
                //        };
                //    }
                //}

                //for (int i = 0; i < bitmapData.Height; ++i)
                //{
                //    for (int j = 0; j < bitmapData.Width; ++j)
                //    {
                //        //byte* data = scan0 + i * stride + j * bytesPerPixel;
                //        //data is a pointer to the first byte of the 3-byte color data
                //        //data[0] = blueComponent;
                //        //data[1] = greenComponent;
                //        //data[2] = redComponent;

                //        var b = *(firstPixelPtr);
                //        var g = *(firstPixelPtr + 1);
                //        var r = *(firstPixelPtr + 2);
                //        var a = *(firstPixelPtr + 3);

                //        imageAsArray[count++] = new double[3] {
                //            r,
                //            g,
                //            b
                //        };

                //        firstPixelPtr += stride;
                //    }
                //}

                //Parallel.For(0, heightInPixels, y =>
                //{
                //    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                //    for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                //    {
                //        int blue = currentLine[x];
                //        int green = currentLine[x + 1];
                //        int red = currentLine[x + 2];

                //        int currentIndex = x / bytesPerPixel + y * bitmapData.Width;

                //        imageAsArray[currentIndex] = new double[3]
                //        {
                //            red,
                //            green,
                //            blue
                //        };
                //    }
                //});

                processedBitmap.UnlockBits(bitmapData);

                return imageAsArray;
            }
        }

        private void ProcessUsingLockbitsAndUnsafeAndParallel(Bitmap processedBitmap)
        {
            unsafe
            {
                BitmapData bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), ImageLockMode.ReadWrite, processedBitmap.PixelFormat);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                    {
                        int oldBlue = currentLine[x];
                        int oldGreen = currentLine[x + 1];
                        int oldRed = currentLine[x + 2];

                        currentLine[x] = (byte)oldBlue;
                        currentLine[x + 1] = (byte)oldGreen;
                        currentLine[x + 2] = (byte)oldRed;
                    }
                });
                processedBitmap.UnlockBits(bitmapData);
            }
        }
    }
}
