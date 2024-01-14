using System.Diagnostics;
using System.Drawing;
using TLDROverlay.Overlay.Icons;
using Icon = TLDROverlay.Overlay.Icons.Icon;

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
            // Make icon resolution scale with the Y axis space available.
            scheme.AddIcon(1, new Icon("C:\\Users\\Lucad\\Desktop\\Cosas\\TLDR Masterduel Overlay\\TLDROverlay.Overlay\\Resources\\placeholder.png", new Size(32, 32)));
            var distr = (2, 3);
            Point startingPoint = new Point(176, 352);

            //ApplicationConfiguration.Initialize();
            var overlay = new OverlayManager(distr, scheme, startingPoint);
            overlay.ShowOverlay();
            overlay.AppendIcon(1, "test tooltip");
            overlay.AppendIcon(1, "test tooltip 2");
            overlay.AppendIcon(1, "test tooltip 3");
            overlay.AppendIcon(1, "test tooltip 2");
            overlay.AppendIcon(1, "test tooltip 3");
            Thread.Sleep(2000);
            overlay.HideOverlay();
            Thread.Sleep(1000);
            overlay.ShowOverlay();
            Thread.Sleep(5000);
            overlay.StopOverlay();
        }
    }
}