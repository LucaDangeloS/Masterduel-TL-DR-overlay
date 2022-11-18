using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TLDROverlay.Overlay.Icons;
using Icon = TLDROverlay.Overlay.Icons.Icon;

namespace TLDROverlay.Overlay
{
    public class OverlayManager
    {
        private Point _iconDistribution;
        public Point IconDistribution {
            get { return _iconDistribution; }
            set
            {
                _iconDistribution = value;
                MaxIconCount = _iconDistribution.X * _iconDistribution.Y;
            }
        }
        
        private IconScheme _iconScheme;
        public IconScheme IconScheme {
            get { return _iconScheme; }
            set
            {
                ClearIcons();
                _iconScheme = value;
            }
        }
        
        private Point _startingPoint;
        public Point StartingPoint
        {
            get {return _startingPoint; }
            set
            {
                _startingPoint = value;

                if (_isOverlayRunning)
                {
                    UpdateOverlay(_overlayWindow.SetWindowLocation, _startingPoint);
                }
            }
            
        }

        private bool _isOverlayRunning = false;
        private Overlay _overlayWindow;
        private int IconCount = 0;
        private int MaxIconCount;
        private List<PictureBox> ActiveIconsList;
        private Point LastIconLocation = new Point(0, 0);
        private int MaxYIconSize = 0;

        public OverlayManager((int, int) iconDistribution, IconScheme iconScheme, Point startingPoint)
        {
            ActiveIconsList = new List<PictureBox>();
            _overlayWindow = new Overlay(startingPoint);

            IconDistribution = new Point(iconDistribution.Item1, iconDistribution.Item2);
            IconScheme = iconScheme;
            StartingPoint = startingPoint;
        }

        // public methods
        public void Initialize()
        {
            _isOverlayRunning = true;
            _overlayWindow.ShowDialog();
        }
        public void HideOverlay()
        {
            _overlayWindow.Hide();
        }
        
        public void ShowOverlay()
        {
            _overlayWindow.Show();
        }

        public void AppendIcon(int key, string tooltip)
        {
            if (IconCount + 1 > MaxIconCount)
            {
                throw new Exception("Icon limit reached");
            }
            Icon icon = IconScheme.GetIcon(key);
            Point iconPos = CalculateNextIconPosition(icon.Size);
            PictureBox picBox = CreatePictureBox(icon, iconPos, icon.Size);
            ActiveIconsList.Add(picBox);
            IconCount++;
            UpdateOverlay(_overlayWindow.Controls.Add, picBox);
        }

        public void ClearIcons()
        {
            foreach(PictureBox picBox in ActiveIconsList)
            {
                UpdateOverlay(_overlayWindow.Controls.Remove, picBox);
            }
            ActiveIconsList.Clear();
            IconCount = 0;
            LastIconLocation = new Point(0, 0);
            MaxYIconSize = 0;
        }
        
        // private methods
        private void UpdateOverlay(Delegate func, params object[] parameters)
        {
            try
            {
                _overlayWindow.Invoke((MethodInvoker)delegate
                {
                    func.DynamicInvoke(parameters);
                });
            } catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private PictureBox CreatePictureBox(Icon icon, Point location, Size size)
        {
            PictureBox picBox = new PictureBox();
            picBox.Location = location;
            picBox.Size = size;
            picBox.Visible = true;
            picBox.Image = icon.ToBitmap();
            ActiveIconsList.Add(picBox);
            
            return picBox;
        }

        private Point CalculateNextIconPosition(Size size)
        {
            Point iconPos = LastIconLocation;
            var mod = IconCount % IconDistribution.Y;

            if (mod == 0)
            {
                iconPos.X = 0;
                iconPos.Y += MaxYIconSize;
            }

            LastIconLocation = new Point(iconPos.X + size.Width, iconPos.Y);
            MaxYIconSize = int.Max(size.Height, MaxYIconSize);
            return iconPos;
        }
    }
}
