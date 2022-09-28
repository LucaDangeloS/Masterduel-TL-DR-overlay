﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masterduel_TLDR_overlay.Screen
{
    internal static class MasterduelWindow
    {
        public static readonly string WINDOW_NAME = "masterduel";
        private static readonly float ASPECT_RATIO = 0.5625f;
        private static readonly int WIDTH_BORDER_OFFSET = 16;
        private static readonly int HEIGHT_BORDER_OFFSET = 39;

        // Maybe a handle to check if the window changed? Or store last Window state to avoid recalculating too often

        // 16 * 39 offset for borders
        public static class Window {
            protected interface RelPos
            {
                public float X_REL_INIT_POS { get; }
                public float Y_REL_INIT_POS { get; }
                public float X_REL_END_POS { get; }
                public float Y_REL_END_POS { get; }
            }
            protected class TextRelPos : RelPos
            {
                public float X_REL_INIT_POS => 0.0191f;
                public float Y_REL_INIT_POS => 0.14f;
                public float X_REL_END_POS => 0.19f;
                public float Y_REL_END_POS => 0.18f;
            };
            protected class DeckEditorTextRelPos : RelPos
            {
                public float X_REL_INIT_POS => 0.03f;
                public float Y_REL_INIT_POS => 0.11f;
                public float X_REL_END_POS => 0.211f;
                public float Y_REL_END_POS => 0.155f;
            };
            protected class SplashRelPos : RelPos
            {
                public float X_REL_INIT_POS => 0.030f;
                public float Y_REL_INIT_POS => 0.245f;
                public float X_REL_END_POS => 0.0922f;
                public float Y_REL_END_POS => 0.3546f;
            };
            private static bool IsWindowed(Size size)
            {
                var aspectRatio = (float)size.Height / size.Width;
                return !(aspectRatio == ASPECT_RATIO);
            }

            /// <summary>
            /// Returns the coordinates where the currently selected card name text field is located at the screen in "masterduel".
            /// <code></code>The coordinates take into acount the window size, resolution and the window mode.
            /// </summary>
            /// <param name="wp">The window points coordinates. In the format <code>(Point(x1, y1), Point(x2, y2))</code> being (x1, y1) the upper-left corner 
            /// and (x2, y2) the lower-right corner.</param>
            /// <returns>Returns absolute the set  of points coordinates that delimit the rectangle where the name of the currently selected card text is located.</returns>
            public static (Point, Point) GetCardTitleCoords((Point, Point) wp)
            {
                Size size = new(Math.Abs(wp.Item2.X - wp.Item1.X), Math.Abs(wp.Item2.Y - wp.Item1.Y));

                if (IsWindowed(size))
                {
                    size = new(Math.Abs(wp.Item2.X - wp.Item1.X) - WIDTH_BORDER_OFFSET, Math.Abs(wp.Item2.Y - wp.Item1.Y) - HEIGHT_BORDER_OFFSET);
                    wp.Item1.X += WIDTH_BORDER_OFFSET / 2;
                    wp.Item1.Y += HEIGHT_BORDER_OFFSET - WIDTH_BORDER_OFFSET / 2;
                }
                RelPos pos = new DeckEditorTextRelPos();
                var newPoints = (new Point((int)(pos.X_REL_INIT_POS * size.Width) + wp.Item1.X,
                                           (int)(pos.Y_REL_INIT_POS * size.Height) + wp.Item1.Y),
                                 new Point((int)(pos.X_REL_END_POS * size.Width) + wp.Item1.X,
                                           (int)(pos.Y_REL_END_POS * size.Height) + wp.Item1.Y));
                return newPoints;
            }

            /// <summary>
            /// Returns the coordinates where the currently selected card splash art is located at the screen in "masterduel".
            /// <code></code>The coordinates take into acount the window size, resolution and the window mode.
            /// </summary>
            /// <param name="wp">The window points coordinates. In the format <code>(Point(x1, y1), Point(x2, y2))</code> being (x1, y1) the upper-left corner 
            /// and (x2, y2) the lower-right corner.</param>
            /// <returns>Returns absolute the set  of points coordinates that delimit the rectangle where the name of the currently selected card image is located.</returns>
            public static (Point, Point) GetCardSplashCoords((Point, Point) wp)
            {
                Size size = new(Math.Abs(wp.Item2.X - wp.Item1.X), Math.Abs(wp.Item2.Y - wp.Item1.Y));

                if (IsWindowed(size))
                {
                    size = new(Math.Abs(wp.Item2.X - wp.Item1.X) - WIDTH_BORDER_OFFSET, Math.Abs(wp.Item2.Y - wp.Item1.Y) - HEIGHT_BORDER_OFFSET);
                    wp.Item1.X += WIDTH_BORDER_OFFSET / 2;
                    wp.Item1.Y += HEIGHT_BORDER_OFFSET - WIDTH_BORDER_OFFSET / 2;
                }
                RelPos pos = new SplashRelPos();
                var newPoints = (new Point((int)(pos.X_REL_INIT_POS * size.Width) + wp.Item1.X,
                                           (int)(pos.Y_REL_INIT_POS * size.Height) + wp.Item1.Y),
                                 new Point((int)(pos.X_REL_END_POS * size.Width) + wp.Item1.X,
                                           (int)(pos.Y_REL_END_POS * size.Height) + wp.Item1.Y));
                return newPoints;
            }
        }
    }
}