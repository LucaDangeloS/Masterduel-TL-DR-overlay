using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Masterduel_TLDR_overlay.Screen
{
    internal class ScreenProcessing
    {
        /// <summary>
        /// Takes a screenshot of the rectangle delimited <c>from</c> a point <c>to</c> a point.
        /// </summary>
        /// <param name="from">Upper-left corner of the rectangle.</param>
        /// <param name="to">Bottom-right corner of the rectangle.</param>
        /// <returns>Returns a <see cref="Bitmap"/> object with the screenshot data.</returns>
        public static Bitmap TakeScreenshotFromArea(Point from, Point to)
        {
            Size size = new(Math.Abs(to.X - from.X), Math.Abs(to.Y - from.Y));
            Bitmap bm = new Bitmap(size.Width, size.Height);
            Graphics g = Graphics.FromImage(bm);

            g.CopyFromScreen(from, Point.Empty, bm.Size);
            g.Dispose();

            return bm;
        }

        /// <summary>
        /// Compares two bitmap images and returns the difference coefficient rangind from 0 to 1.
        /// </summary>
        /// <param name="bm1">First bitmap.</param>
        /// <param name="bm2">Second bitmap.</param>
        /// <returns>The difference coefficient between the images</returns>
        public static float CompareImages(Bitmap bm1, Bitmap bm2)
        {
            var size = (32, 32);
            List<bool> iHash1 = getImageHash(bm1, size);
            List<bool> iHash2 = getImageHash(bm2, size);
            // Debug.WriteLine(iHash1.Count * sizeof(bool));

            //determine the number of equal pixel (x of 256)
            int equalElements = iHash1.Zip(iHash2, (i, j) => i == j).Count(eq => eq);
            return (float)equalElements / (size.Item1 * size.Item2);
        }

        /// <summary>
        /// Creates a hash list from an image bitmap to make comparison efficient. The underlying metric used is a greyscale normalization function.
        /// </summary>
        /// <param name="bm1">The image Bitmap.</param>
        /// <param name="size">Dimensions of the image hash.</param>
        /// <returns>A <see cref="List"/> of <see cref="bool"/> values representing the hashmap.</returns>
        public static List<bool> getImageHash(Bitmap bm, (int width, int height) size)
        {
            int bytesPerPixel = 4; //Format32bppArgb
            int maxPointerLenght = size.width * size.height * bytesPerPixel;
            int stride = size.width * bytesPerPixel;
            var bytes = new byte[size.height * stride];
            byte R, G, B, A;

            List<bool> lResult = new List<bool>();
            Bitmap bmpMin = new Bitmap(bm, new Size(size.width, size.height));
            bmpMin.Save(@"Temp2.jpg"); // Remove
            BitmapData bData = bmpMin.LockBits(
                new System.Drawing.Rectangle(0, 0, size.width, size.height),
                ImageLockMode.ReadWrite, bmpMin.PixelFormat);

            Marshal.Copy(bData.Scan0, bytes, 0, bytes.Length);

            for (int i = 0; i < maxPointerLenght; i += 4)
            {
                B = bytes[i + 0];
                G = bytes[i + 1];
                R = bytes[i + 2];
                A = bytes[i + 3];
                //reduce colors to true / false                
                lResult.Add(PixelMetric(Color.FromArgb(A, R, G, B)));
                //Debug.Write(PixelMetric(Color.FromArgb(A, R, G, B)) ? "1" : "0");
                //if (i % stride == 0)
                //{
                //    Debug.WriteLine("");
                //}
            }
            bmpMin.UnlockBits(bData);

            return lResult;
        }

        private static bool PixelMetric(Color pixel)
        {
            // Grey scale > 0.5
            return ((pixel.R + pixel.G + pixel.B) / 3) <= 128;
        }
    }
}
