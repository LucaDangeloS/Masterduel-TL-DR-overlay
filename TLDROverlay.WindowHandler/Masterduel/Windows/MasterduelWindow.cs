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
        private static int WIDTH_BORDER_OFFSET;
        private static int HEIGHT_BORDER_OFFSET;
        private static readonly int[] WIDTHS_GDC = { 16, 16, 1366 };
        private static readonly int[] HEIGHT_GDC = { 9, 10, 768 };
        private static readonly int MAX_WIN_OFFSET = 50;
        
        // Redefinitions
        public new int WindowWidth { get; private set; }
        public new int WindowHeight { get; private set; }
        
        private (Point, Point) _WindowArea;
        public new (Point, Point) WindowArea
        {
            get
            {
                return _WindowArea;
            }
            set
            {
                if (_WindowArea == value) return;
                _WindowArea = value;
                Debug.WriteLine("Impl Changing window points");
                WindowWidth = Math.Abs(_WindowArea.Item2.X - _WindowArea.Item1.X);
                WindowHeight = Math.Abs(_WindowArea.Item2.Y - _WindowArea.Item1.Y);

                // Recalculate coordinates
                CardTitle = GetPosCoords(_WindowArea, new Window.TextRelPos());
                CardDesc = GetPosCoords(_WindowArea, new Window.DescRelPos());
                CardSplash = GetPosCoords(_WindowArea, new Window.SplashRelPos());
                YourLP = GetPosCoords(_WindowArea, new Window.YourLPRelPos());
                EnemyLP = GetPosCoords(_WindowArea, new Window.EnemyLPRelPos());
                CardType = GetPosCoords(_WindowArea, new Window.CardTypeRelPos());
            }
        }

        // private methods
        private (Point, Point) TrimWindowBorders(Point, Point)
        {
            
        }
        
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
        
        public override (Point, Point) GetPosCoords((Point, Point) wp, IRelativePosition pos)
        {
            Size size = new(Math.Abs(wp.Item2.X - wp.Item1.X), Math.Abs(wp.Item2.Y - wp.Item1.Y));

            if (IsWindowed(size))
            {
                size = new(Math.Abs(wp.Item2.X - wp.Item1.X) - WIDTH_BORDER_OFFSET, Math.Abs(wp.Item2.Y - wp.Item1.Y) - HEIGHT_BORDER_OFFSET);
                wp.Item1.X += WIDTH_BORDER_OFFSET / 2;
                wp.Item1.Y += HEIGHT_BORDER_OFFSET - WIDTH_BORDER_OFFSET / 2;
            };

            var newPoints = (new Point((int)(pos.X_REL_INIT_POS * size.Width) + wp.Item1.X,
                                        (int)(pos.Y_REL_INIT_POS * size.Height) + wp.Item1.Y),
                                new Point((int)(pos.X_REL_END_POS * size.Width) + wp.Item1.X,
                                        (int)(pos.Y_REL_END_POS * size.Height) + wp.Item1.Y));
            return newPoints;
        }
    }

}
