using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masterduel_TLDR_overlay.Screen
{
    internal static class Masterduel
    {
        public static readonly string WINDOW_NAME = "masterduel";
        public static readonly float ASPECT_RATIO = 0.5625f;
        public static readonly int WIDTH_BORDER_OFFSET = 16;
        public static readonly int HEIGHT_BORDER_OFFSET = 39;
        public static readonly int X_INIT_OFFSET = 40;
        public static readonly int Y_INIT_OFFSET = 152;
        public static readonly int X_END_OFFSET = 310;
        public static readonly int Y_END_OFFSET = 193;

        

        // Define static methods to calculate SCALEs and check if it's windowed or not to account for Offset.

        // Public Check if Windowed and change offsets
        // Private Recalculate Offset and scales
        // Public Getters for Scales (Offsets can be private)
        // Make atributes private and just expose getters
        // Maybe a handle to check if the window changed? Or store last Window state to avoid recalculating too often

        // 16 * 39 offset for borders
        public static class Window {
            public static bool WINDOWED_MODE = true;

            struct WindowPos {
            }
            protected static class RelPos
            {
                public static float X_REL_INIT_POS = 0.021658f;
                public static float Y_REL_INIT_POS = 0.16613f;
                public static float X_REL_END_POS = 0.18983f;
                public static float Y_REL_END_POS = 0.20553f;
            };
            private static void UpdateAspectRatio(Size size)
            {
                var aspectRatio = (float)size.Height / size.Width;
                Debug.WriteLine(aspectRatio);
                WINDOWED_MODE = !(aspectRatio == ASPECT_RATIO);
            }

            public static (Point, Point) GetCardTitleCoords((Point, Point) wp)
            {
                Size size = new(Math.Abs(wp.Item2.X - wp.Item1.X), Math.Abs(wp.Item2.Y - wp.Item1.Y));
                UpdateAspectRatio(size);


                var newPoints = (new Point(X_INIT_OFFSET + wp.Item1.X,
                                           Y_INIT_OFFSET + wp.Item1.Y),
                                 new Point((int) (RelPos.X_REL_END_POS * size.Width) + wp.Item1.X,
                                           Y_END_OFFSET + wp.Item1.Y));
                return newPoints;
            }

            public static 
        }
    }
}
