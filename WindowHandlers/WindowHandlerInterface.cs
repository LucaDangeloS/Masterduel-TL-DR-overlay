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
        /// <exception cref="NoWindowFoundException">Thrown if the window has no dimensions.</exception>
        /// <exception cref="NoDimensionsFoundException">Thrown if no window with that name was found.</exception>
        /// <returns>A tuple of points, the first one being the upper-left corner and the second the bottom-right corner of the window in Point coordinates.</returns>
        public (Point, Point) GetWindowPoints(string windowName);
        public bool IsWindowCurrentlySelected();
        public bool GetLeftMousePressed();
        public bool GetLeftMousePressed();

        class NoWindowFoundException : Exception
        {
            public NoWindowFoundException() { }

            public NoWindowFoundException(string name)
                : base(string.Format("Invalid Student Name: {0}", name))
            {

            }
        }


        class NoDimensionsFoundException : Exception
        {
            public NoDimensionsFoundException()
                : base(string.Format("Could not retrieve window size and position"))
            {

            }
        }
    }
}
