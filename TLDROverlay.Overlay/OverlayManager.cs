using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using TLDROverlay.Overlay.Icons;
using Icon = System.Drawing.Icon;

namespace TLDROverlay.Overlay
{
    public class OverlayManager
    {
        private (int, int) _iconDistribution;
        public (int, int) IconDistribution {
            get { return _iconDistribution; }
            set
            {
                _iconDistribution = value;
                _maxIconCount = _iconDistribution.Item1 * _iconDistribution.Item2;
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
        private readonly List<Icon> _icons;
        private int IconCount = 0;
        private int _maxIconCount;

        public OverlayManager((int, int) iconDistribution, IconScheme iconScheme, Point startingPoint)
        {
            _overlayWindow = new Overlay();

            IconDistribution = iconDistribution;
            IconScheme = iconScheme;
            StartingPoint = startingPoint;

            _icons = new List<Icon>();
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
            if (IconCount + 1 > _maxIconCount)
            {
                throw new Exception("Icon limit reached");
            }
            var icon = IconScheme.GetIcon(key);
        }

        public List<Icon> GetIcons()
        {
            return _icons;
        }

        public void ClearIcons()
        {
            
        }
        
        // private methods
        private void UpdateOverlay(Delegate func, params object[] parameters)
        {
            _overlayWindow.Invoke((MethodInvoker)delegate
            {
                func.DynamicInvoke(parameters);
            });
        }
    }
}
