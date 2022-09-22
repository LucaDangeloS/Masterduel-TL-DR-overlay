using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Masterduel_TLDR_overlay.WindowHandlers
{
    internal interface WindowHandlerInterface
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Boundaries
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        /// <summary>
        /// Method <c>getWindowPoints</c> gets the boundaries of a window given its name.
        /// </summary>
        /// <param name="windowName">Name of the window to search for.</param>
        /// <exception cref="NoWindowFound">Thrown if the window has no dimensions.</exception>
        /// <exception cref="NoDimensionsFound">Thrown if no window with that name was found.</exception>
        /// <returns>A tuple of points, the first one being the upper-left corner and the second the bottom-right corner of the window in Point coordinates.</returns>
        public (Point, Point) GetWindowPoints(string windowName);

        class NoWindowFound : Exception
        {
            public NoWindowFound() { }

            public NoWindowFound(string name)
                : base(string.Format("Invalid Student Name: {0}", name))
            {

            }
        }

        class NoDimensionsFound : Exception
        {
            public NoDimensionsFound()
                : base(string.Format("Could not retrieve window size and position"))
            {

            }
        }
    }
}
