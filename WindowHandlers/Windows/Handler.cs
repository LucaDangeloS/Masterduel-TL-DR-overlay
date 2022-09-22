using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Masterduel_TLDR_overlay.WindowHandlers;
using static Masterduel_TLDR_overlay.WindowHandlers.WindowHandlerInterface;
using System.Web;

namespace Masterduel_TLDR_overlay.Windows
{
    internal class Handler : WindowHandlerInterface
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static private extern bool GetWindowRect(IntPtr hWnd, ref Boundaries lpRect);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, EntryPoint = "GetCurrentObject", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr IntGetCurrentObject(HandleRef hDC, int uObjectType);

        private IntPtr GetWinHandle(string windowName)
        {
            IntPtr hWnd = IntPtr.Zero;
            foreach (Process pList in Process.GetProcesses())
            {
                // Debug.WriteLine("[DEBUG] - " + pList.MainWindowTitle);
                if (pList.MainWindowTitle.Contains(windowName))
                {
                    hWnd = pList.MainWindowHandle;
                    break;
                }
            }
            if (hWnd == IntPtr.Zero) throw new NoWindowFound("No window with name " + windowName + " found.");
            return hWnd;
        }

        private bool GetWindowBoundaries(IntPtr hWnd, ref Boundaries b)
        {
            return GetWindowRect(hWnd, ref b);
        }

        public (Point, Point) GetWindowPoints(string windowName)
        {
            Boundaries b = new Boundaries();
            IntPtr hWnd = GetWinHandle(windowName);
            var res = GetWindowBoundaries(hWnd, ref b);
            if (!res) throw new NoDimensionsFound();

            return (new Point(b.Left, b.Top), new Point(b.Right, b.Bottom));
        }
    }
}
