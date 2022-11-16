using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TLDROverlay.Overlay
{
    public partial class Overlay : Form
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public Overlay()
        {
            InitializeComponent();
        }

        private void Overlay_Load(object sender, EventArgs e)
        {
            BackColor = Color.Wheat;
            TransparencyKey = Color.Wheat;
            //FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;

            //int initialStyle = GetWindowLong(Handle, -20);
            //SetWindowLong(Handle, -20, initialStyle | 0x80000 | 0x20);
        }

        public void SetWindowLocation(Point newLocation)
        {
            Location = newLocation;
            Show();
        }

        public void SetWindowSize(Size newSize)
        {
            Size = newSize;
            Show();
        }

        // TODO: Draw Icon at position?
    }
}