﻿using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using System.Windows.Forms;

namespace Masterduel_TLDR_overlay.Screen
{
    /// <summary>
    ///    This is a sttatic class.
    /// </summary>
    public static class ImageProcessing
    {
        // Public methods
        
        /// <summary>
        /// Takes a screenshot of the rectangle delimited <c>from</c> a point <c>to</c> a point.
        /// </summary>
        /// <param name="from">Upper-left corner of the rectangle.</param>
        /// <param name="to">Bottom-right corner of the rectangle.</param>
        /// <returns>Returns a <see cref="Bitmap"/> object with the screenshot data.</returns>
        public static Bitmap TakeScreenshotFromArea(Point from, Point to)
        {
            Size size = new(Math.Abs(to.X - from.X), Math.Abs(to.Y - from.Y));
            Bitmap bm = new(size.Width, size.Height);
            Graphics g = Graphics.FromImage(bm);

            g.CopyFromScreen(from, Point.Empty, bm.Size);
            g.Dispose();

            return bm;
        }
        public static Bitmap TakeScreenshotFromArea((Point, Point) area)
        {
            return TakeScreenshotFromArea(area.Item1, area.Item2);
        }

        /// <summary>
        /// Compares two bitmap images and returns the difference coefficient rangind from 0 to 1.
        /// </summary>
        /// <param name="bm1">First bitmap.</param>
        /// <param name="bm2">Second bitmap.</param>
        /// <returns>The difference coefficient between the images</returns>
        public static float CompareImages(Bitmap bm1, Bitmap bm2)
        {
            var size = (24, 24);
            List<bool> iHash1 = GetImageHash(bm1, size);
            List<bool> iHash2 = GetImageHash(bm2, size);

            //determine the number of equal pixel (x of 256)
            int equalElements = iHash1.Zip(iHash2, (i, j) => i == j).Count(eq => eq);
            return (float)equalElements / (size.Item1 * size.Item2);
        }
        public static float CompareImages(List<bool> iHash1, List<bool> iHash2)
        {
            if (iHash1.Count != iHash2.Count)
            {
                throw new ArgumentException("The hashes must have the same size.");
            }
            var temp = (int) Math.Sqrt(iHash1.Count);
            var size = (temp, temp);
            //determine the number of equal pixels
            int equalElements = iHash1.Zip(iHash2, (i, j) => i == j).Count(eq => eq);
            return (float) equalElements / (size.Item1 * size.Item2);
        }

        public static (List<bool> hash, float MeanBrightness, float MeanSaturation) GetImageMetrics(Bitmap bm, (int width, int height) size)
        {
            int bytesPerPixel = 4; //Format32bppArgb
            int maxPointerLenght = size.width * size.height * bytesPerPixel;
            int stride = size.width * bytesPerPixel;
            var bytes = new byte[size.height * stride];
            byte R, G, B, A;

            float meanSaturation = 0.0f;
            float meanBrightness = 0.0f;

            List<bool> lResult = new ();
            Bitmap bmpMin = new (bm, new Size(size.width, size.height));
            
            BitmapData bData = bmpMin.LockBits(
                new Rectangle(0, 0, size.width, size.height),
                ImageLockMode.ReadWrite, bmpMin.PixelFormat);

            Marshal.Copy(bData.Scan0, bytes, 0, bytes.Length);

            for (int i = 0; i < maxPointerLenght; i += 4)
            {
                B = bytes[i + 0];
                G = bytes[i + 1];
                R = bytes[i + 2];
                A = bytes[i + 3];
                //reduce colors to true / false                
                Color fa = Color.FromArgb(A, R, G, B);
                lResult.Add(PixelMetric(fa));
                meanSaturation += fa.GetSaturation();
                meanBrightness += fa.GetBrightness();
            }
            meanSaturation = meanSaturation / maxPointerLenght;
            meanBrightness = meanBrightness / maxPointerLenght;
            bmpMin.UnlockBits(bData);

            return (lResult, meanBrightness, meanSaturation);
        }

        public class ImageHash : IComparable<ImageHash>
        {
            private List<bool> _hash = new();
            public List<bool> Hash {
                get
                {
                    return _hash;
                }
                set
                {
                    var tmp = (int)Math.Sqrt(_hash.Count);
                    
                    _hash = value;
                    HashSum = _hash.Count((x) => x);
                    Resolution = new(tmp, tmp);
                }
            }
            public int HashSum { get; private set; } = 0;
            
            // Constructors
            public Size Resolution { get; private set; } = new(0, 0);

            public ImageHash(Bitmap bm, Size size)
            {
                Hash = GetImageHash(bm, (size.Width, size.Height));
                HashSum = Hash.Count((x) => x);
                Resolution = size;
            }
            public ImageHash(Bitmap bm, (int w, int h) size)
            {
                Hash = GetImageHash(bm, (size.w, size.h));
                HashSum = Hash.Count((x) => x);
                Resolution = new Size(size.w, size.h);
            }
            public ImageHash(List<bool> hash, Size size)
            {
                Hash = hash;
                HashSum = Hash.Count((x) => x);
                Resolution = size;
            }
            public ImageHash(List<bool> hash, (int w, int h) size)
            {
                Hash = hash;
                HashSum = Hash.Count((x) => x);
                Resolution = new Size(size.w, size.h);
            }
            public ImageHash() { }

            // Public Methods
            int IComparable<ImageHash>.CompareTo(ImageHash? other)
            {
                if (other == null)
                    throw new ArgumentException("ImageHash may not be null.");
                var size = other.Resolution;
                if (size != Resolution)
                    throw new ArgumentException("The hashes must have the same size.");
                // Convert list of bool to a list of ints
                var integerHash1 = Hash.Select(x => x ? 1 : 0).ToList();
                var integerHash2 = other.Hash.Select(x => x ? 1 : 0).ToList();
                return integerHash2.Zip(integerHash1, (i, j) => i - j).Sum();
            }

            public float CompareTo(ImageHash other)
            {
                int equalElements = Hash.Zip(other.Hash, (i, j) => i == j).Count(eq => eq);
                return (float) equalElements / (Resolution.Width * Resolution.Height);
            }
        }

        // Private methods
        private static bool PixelMetric(Color pixel)
        {

            // Grey scale > 0.5
            return ((pixel.R + pixel.G + pixel.B) / 3) <= 128;
        }

        /// <summary>
        /// Creates a hash list from an image bitmap to make comparison efficient. The underlying metric used is a greyscale normalization function.
        /// </summary>
        /// <param name="bm1">The image Bitmap.</param>
        /// <param name="size">Dimensions of the image hash.</param>
        /// <returns>A <see cref="List"/> of <see cref="bool"/> values representing the hashmap.</returns>
        private static List<bool> GetImageHash(Bitmap bm, (int width, int height) size)
        {
            int bytesPerPixel = 4; //Format32bppArgb
            int maxPointerLenght = size.width * size.height * bytesPerPixel;
            int stride = size.width * bytesPerPixel;
            var bytes = new byte[size.height * stride];
            byte R, G, B, A;

            List<bool> lResult = new();
            Bitmap bmpMin = new(bm, new Size(size.width, size.height));

            BitmapData bData = bmpMin.LockBits(
                new Rectangle(0, 0, size.width, size.height),
                ImageLockMode.ReadWrite, bmpMin.PixelFormat);

            Marshal.Copy(bData.Scan0, bytes, 0, bytes.Length);

            for (int i = 0; i < maxPointerLenght; i += 4)
            {
                B = bytes[i + 0];
                G = bytes[i + 1];
                R = bytes[i + 2];
                A = bytes[i + 3];
                //reduce colors to true / false                
                Color fa = Color.FromArgb(A, R, G, B);
                lResult.Add(PixelMetric(fa));
            }
            bmpMin.UnlockBits(bData);

            return lResult;
        }
    }
}
