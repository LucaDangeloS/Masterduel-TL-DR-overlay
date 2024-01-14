﻿using System.Drawing;
using System.Runtime.InteropServices;

namespace TLDROverlay.WindowHandler
{
    public interface IWindowHandler
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
        public (Point, Point) GetWindowPoints();
        public bool IsWindowCurrentlySelected();
        public bool GetLeftMousePressed();

        public class NoWindowFoundException : Exception
        {
            public NoWindowFoundException() { }

            public NoWindowFoundException(string name)
                : base(string.Format("No window found: {0}", name))
            {

            }
        }


        public class NoDimensionsFoundException : Exception
        {
            public NoDimensionsFoundException()
                : base(string.Format("Could not retrieve window size and position"))
            {

            }
        }
    }
}
