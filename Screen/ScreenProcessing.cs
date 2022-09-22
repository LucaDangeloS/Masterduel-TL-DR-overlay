using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
