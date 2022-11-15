using System.Diagnostics;
using System.Drawing;

namespace TLDROverlay.WindowHandler.Masterduel;

public abstract class AbstractMasterduelWindow
{
    public readonly string WindowName = "masterduel";
    public int WindowWidth { get; private set; }
    public int WindowHeight { get; private set; }
    private int prevWindowDimensions;
    private (Point, Point) _WindowArea;
    public virtual (Point, Point) WindowArea
    {
        get
        {
            return _WindowArea;
        }
        set
        {
            if (_WindowArea == value) return;
            Debug.WriteLine("Abstract Changing window points");
            _WindowArea = value;
            prevWindowDimensions = WindowWidth + WindowHeight;
            WindowWidth = Math.Abs(_WindowArea.Item2.X - _WindowArea.Item1.X);
            WindowHeight = Math.Abs(_WindowArea.Item2.Y - _WindowArea.Item1.Y);

            // Recalculate coordinates
            CardTitleCoordinates = GetPosCoords(_WindowArea, new Window.TextRelPos());
            CardDescCoordinates = GetPosCoords(_WindowArea, new Window.DescRelPos());
            CardSplashCoordinates = GetPosCoords(_WindowArea, new Window.SplashRelPos());
            YourLPCoordinates = GetPosCoords(_WindowArea, new Window.YourLPRelPos());
            EnemyLPCoordinates = GetPosCoords(_WindowArea, new Window.EnemyLPRelPos());
            CardTypeCoordinates = GetPosCoords(_WindowArea, new Window.CardTypeRelPos());
        }
    }

    // Coords
    public (Point, Point) CardTitleCoordinates;
    public (Point, Point) CardDescCoordinates;
    public (Point, Point) CardSplashCoordinates;
    public (Point, Point) YourLPCoordinates;
    public (Point, Point) EnemyLPCoordinates;
    public (Point, Point) CardTypeCoordinates;


    // public methods
    public virtual bool DidResolutionChange()
    {
        return prevWindowDimensions != WindowWidth + WindowHeight;
    }


    public virtual(Point, Point) GetPosCoords((Point, Point) wp, IRelativePosition pos)
    {
        Size size = new(Math.Abs(wp.Item2.X - wp.Item1.X), Math.Abs(wp.Item2.Y - wp.Item1.Y));

        var newPoints = (new Point((int)(pos.X_REL_INIT_POS * size.Width) + wp.Item1.X,
                                    (int)(pos.Y_REL_INIT_POS * size.Height) + wp.Item1.Y),
                            new Point((int)(pos.X_REL_END_POS * size.Width) + wp.Item1.X,
                                    (int)(pos.Y_REL_END_POS * size.Height) + wp.Item1.Y));
        return newPoints;
    }

    // protected methods
    protected static class Window
    {
        public class TextRelPos : IRelativePosition
        {
            public float X_REL_INIT_POS => 0.0191f;
            public float Y_REL_INIT_POS => 0.14f;
            public float X_REL_END_POS => 0.19f;
            public float Y_REL_END_POS => 0.18f;
        };
        public class CardTypeRelPos : IRelativePosition
        {
            public float X_REL_INIT_POS => 0.0175f;
            public float Y_REL_INIT_POS => 0.4288f;
            public float X_REL_END_POS => 0.2156f;
            public float Y_REL_END_POS => 0.460f;
        };
        public class DescRelPos : IRelativePosition
        {
            public float X_REL_INIT_POS => 0.01625f;
            public float Y_REL_INIT_POS => 0.4655f;
            public float X_REL_END_POS => 0.20f;
            public float Y_REL_END_POS => 0.58f;
        }
        public class DeckEditorTextRelPos : IRelativePosition
        {
            public float X_REL_INIT_POS => 0.03f;
            public float Y_REL_INIT_POS => 0.11f;
            public float X_REL_END_POS => 0.211f;
            public float Y_REL_END_POS => 0.160f;
        };
        public class SplashRelPos : IRelativePosition
        {
            public float X_REL_INIT_POS => 0.030f;
            public float Y_REL_INIT_POS => 0.245f;
            public float X_REL_END_POS => 0.0922f;
            public float Y_REL_END_POS => 0.3546f;
        };
        public class EnemyLPRelPos : IRelativePosition
        {
            public float X_REL_INIT_POS => 0.7993f;
            public float Y_REL_INIT_POS => 0.0733f;
            public float X_REL_END_POS => 0.8262f;
            public float Y_REL_END_POS => 0.1033f;
        };
        public class YourLPRelPos : IRelativePosition
        {
            public float X_REL_INIT_POS => 0.0818f;
            public float Y_REL_INIT_POS => 0.9422f;
            public float X_REL_END_POS => 0.11f;
            public float Y_REL_END_POS => 0.9744f;
        };
    }
}
