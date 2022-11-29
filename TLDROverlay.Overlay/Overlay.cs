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
            TransparencyKey = Color.Wheat;
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
            Color backgroundColor = Color.Navy;
            Color foregroundColor = Color.White;

            CreateTooltipIcon(picBox, tooltip, foregroundColor, backgroundColor);
        }

        public void AddIconWithTooltipWithColor(PictureBox picBox, string tooltip, Color? foregroundColor = null, Color? backgroundColor = null)
        {
            if (foregroundColor == null) foregroundColor = Color.White;
            if (backgroundColor == null) backgroundColor = Color.Navy;
            
            CreateTooltipIcon(picBox, tooltip, (Color)foregroundColor, (Color)backgroundColor);
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
        // private methods
        private void CreateTooltipIcon(PictureBox picBox, string tooltip, Color foregroundColor, Color backgroundColor)
        {
            Controls.Add(picBox);
            ToolTip tt = new ToolTip();
            tt.InitialDelay = 0;
            tt.AutoPopDelay = 20000;
            tt.ForeColor = foregroundColor;
            tt.BackColor = backgroundColor;
            tt.ShowAlways = true;
            tt.SetToolTip(picBox, tooltip);
        }

        private void Overlay_Shown(object sender, EventArgs e)
        {
            LoadedSignal.Release();
        }

        private void ToolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
            using (var boldFont = new Font(e.Font, FontStyle.Bold))
            {
                var headerText = "Header: ";
                var valueText = "Value";

                var headerTextSize = TextRenderer.MeasureText(headerText, e.Font);

                TextRenderer.DrawText(e.Graphics, headerText, e.Font, e.Bounds.Location, Color.Black);

                var valueTextPosition = new Point(e.Bounds.X + headerTextSize.Width, e.Bounds.Y);
                TextRenderer.DrawText(e.Graphics, valueText, boldFont, valueTextPosition, Color.Black);
            }
        }
    }
}