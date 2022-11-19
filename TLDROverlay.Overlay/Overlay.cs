using System.Diagnostics;
using System.Runtime.InteropServices;
using static TLDROverlay.Overlay.OverlayManager;

namespace TLDROverlay.Overlay
{
    public partial class Overlay : Form
    {
        public SemaphoreSlim LoadedSignal = new SemaphoreSlim(0, 1);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;


        const int GWL_EXSTYLE = -20;
        const int WS_EX_LAYERED = 0x80000;
        const int WS_EX_TRANSPARENT = 0x20;

        public Overlay()
        {
            InitializeComponent();
        }

        public Overlay(Point startingPoint)
        {
            StartPosition = FormStartPosition.Manual;
            Location = startingPoint;
            InitializeComponent();
        }

        private void Overlay_Load(object sender, EventArgs e)
        {
            StartPosition = FormStartPosition.Manual;
            BackColor = Color.Wheat;
            //TransparencyKey = Color.Wheat;
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            ShowInTaskbar = false;
        }

        public void SetWindowLocation(Point newLocation)
        {
            Location = newLocation;
        }

        public void SetWindowSize(Size newSize)
        {
            Size = newSize;
        }

        public void AddIconWithTooltip(PictureBox picBox, string tooltip)
        {
            Controls.Add(picBox);
            ToolTip tt = new ToolTip();
            tt.InitialDelay = 0;
            tt.AutoPopDelay = 20000;
            tt.ForeColor = Color.White;
            tt.BackColor = Color.Navy;
            tt.ShowAlways = true;
            tt.SetToolTip(picBox, tooltip);
        }
        
        public void EnableClickThrough()
        {
            int initialStyle = GetWindowLong(Handle, GWL_EXSTYLE);
            SetWindowLong(Handle, GWL_EXSTYLE, initialStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }

        public void DisableClickThrough()
        {
            int initialStyle = GetWindowLong(Handle, GWL_EXSTYLE);
            SetWindowLong(Handle, GWL_EXSTYLE, initialStyle & ~(WS_EX_LAYERED | WS_EX_TRANSPARENT));
        }


        public void ShowOverlay()
        {
            Visible = true;
        }
        
        public void HideOverlay()
        {
            Hide();
        }

        
        private void PerformClick()
        {
            uint X = (uint)Cursor.Position.X;
            uint Y = (uint)Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
        }

        private void Overlay_Shown(object sender, EventArgs e)
        {
            LoadedSignal.Release();
        }
    }
}