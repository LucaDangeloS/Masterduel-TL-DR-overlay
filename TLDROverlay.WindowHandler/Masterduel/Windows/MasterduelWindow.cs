using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLDROverlay.WindowHandler.Masterduel.Windows
{
    public class MasterduelWindow : AbstractMasterduelWindow
    {
        private static readonly float DEFAULT_ASPECT_RATIO = 0.5625f;
        private static int WIDTH_BORDER_OFFSET = 0;
        private static int HEIGHT_BORDER_OFFSET = 0;
        private static readonly int[] WIDTHS_GDC = { 160, 160, 1366 };
        private static readonly int[] HEIGHT_GDC = { 90, 100, 768 };
        private static readonly int MAX_WIN_OFFSET = 50;
        private int prevWindowDimensions;
        
        // Redefinitions
        public new int WindowWidth { get; private set; }
        public new int WindowHeight { get; private set; }
        
        private (Point, Point) _WindowArea;
        public override (Point, Point) WindowArea
        {
            get
            {
                return _WindowArea;
            }
            set
            {
                if (_WindowArea == value) return;
                _WindowArea = value;

                // Recalculate coordinates
                CardTitleCoordinates = GetPosCoords(_WindowArea, new Window.TextRelPos());
                CardDescCoordinates = GetPosCoords(_WindowArea, new Window.DescRelPos());
                CardSplashCoordinates = GetPosCoords(_WindowArea, new Window.SplashRelPos());
                YourLPCoordinates = GetPosCoords(_WindowArea, new Window.YourLPRelPos());
                EnemyLPCoordinates = GetPosCoords(_WindowArea, new Window.EnemyLPRelPos());
                CardTypeCoordinates = GetPosCoords(_WindowArea, new Window.CardTypeRelPos());
                WindowWidth = Math.Abs(_WindowArea.Item2.X - _WindowArea.Item1.X) - WIDTH_BORDER_OFFSET;
                WindowHeight = Math.Abs(_WindowArea.Item2.Y - _WindowArea.Item1.Y) - HEIGHT_BORDER_OFFSET;
                prevWindowDimensions = WindowWidth + WindowHeight;
            }
        }
        
        // public methods
        public override bool DidResolutionChange()
        {
            return prevWindowDimensions != WindowWidth + WindowHeight;
        }

        public override (Point, Point) GetPosCoords((Point, Point) wp, IRelativePosition pos)
        {
            Size size = new(Math.Abs(wp.Item2.X - wp.Item1.X), Math.Abs(wp.Item2.Y - wp.Item1.Y));

            if (IsWindowed(size))
            {
                size = new(size.Width - WIDTH_BORDER_OFFSET, size.Height - HEIGHT_BORDER_OFFSET);
                wp.Item1.X += WIDTH_BORDER_OFFSET / 2;
                wp.Item1.Y += HEIGHT_BORDER_OFFSET - WIDTH_BORDER_OFFSET / 2;
            };

            var newPoints = (new Point((int)(pos.X_REL_INIT_POS * size.Width) + wp.Item1.X,
                                        (int)(pos.Y_REL_INIT_POS * size.Height) + wp.Item1.Y),
                                new Point((int)(pos.X_REL_END_POS * size.Width) + wp.Item1.X,
                                        (int)(pos.Y_REL_END_POS * size.Height) + wp.Item1.Y));
            return newPoints;
        }

        // private methods
        private bool IsWindowed(Size size)
        {
            var aspectRatio = (float)size.Height / size.Width;
            bool ratioed = !(aspectRatio == DEFAULT_ASPECT_RATIO);

            if (ratioed)
            {
                for (int i = 0; i < WIDTHS_GDC.Length; i++)
                {
                    WIDTH_BORDER_OFFSET = size.Width % WIDTHS_GDC[i];
                    HEIGHT_BORDER_OFFSET = size.Height % HEIGHT_GDC[i];

                    if (WIDTH_BORDER_OFFSET < MAX_WIN_OFFSET
                        && HEIGHT_BORDER_OFFSET < MAX_WIN_OFFSET) break;

                }
            }
            return ratioed;
        }
    }

}
