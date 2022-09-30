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
        
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int VirtualKeyPressed);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, EntryPoint = "GetCurrentObject", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr IntGetCurrentObject(HandleRef hDC, int uObjectType);

        private IntPtr WinHandle;

        // Public methods
        public Handler(string windowName)
        {
            WinHandle = GetWinHandle(windowName);
        }
        public bool GetLeftMousePressed()
        {
            if (GetAsyncKeyState(0x01) == 0)
                return false;
            else
                return true;
        }
        public bool IsWindowCurrentlySelected()
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            static extern IntPtr GetForegroundWindow();

            return GetForegroundWindow() == WinHandle;
        }
        public (Point, Point) GetWindowPoints(string windowName)
        {
            Boundaries b = new Boundaries();
            IntPtr hWnd = GetWinHandle(windowName);
            var res = GetWindowBoundaries(hWnd, ref b);
            if (!res) throw new NoDimensionsFoundException();

            return (new Point(b.Left, b.Top), new Point(b.Right, b.Bottom));
        }

        public (Point, Point) GetWindowPoints(IntPtr hWnd)
        {
            Boundaries b = new Boundaries();
            var res = GetWindowBoundaries(hWnd, ref b);
            if (!res) throw new NoDimensionsFoundException();

            return (new Point(b.Left, b.Top), new Point(b.Right, b.Bottom));
        }
        
        // Private methods
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
            if (hWnd == IntPtr.Zero) throw new NoWindowFoundException("No window with name " + windowName + " found.");
            WinHandle = hWnd;
            return hWnd;
        }

        private static bool GetWindowBoundaries(IntPtr hWnd, ref Boundaries b)
        {
            return GetWindowRect(hWnd, ref b);
        }

    }
}
