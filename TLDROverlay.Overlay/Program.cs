using TLDROverlay.Overlay.Icons;

namespace TLDROverlay.Overlay
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            IconScheme scheme = new IconScheme();
            var distr = (1, 3);
            Point start = new Point(0, 0);

            //ApplicationConfiguration.Initialize();
            var overlay = new OverlayManager(distr, scheme, start);
            overlay.Initialize();
        }
    }
}