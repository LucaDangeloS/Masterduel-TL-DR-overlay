using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLDROverlay.Overlay.Icons
{
    // TODO: The implementation of the Icon class may not be needed as there is already one at System.Drawing.Icon
    public class Icon
    {
        public Size Size { get; }
        public string Path { get; }
        private Bitmap Image { get; }

        public Icon(string path, Size resolution)
        {
            var tmpBm = new Bitmap(path);
            Path = path;
            Image = new Bitmap(tmpBm, resolution);
            tmpBm.Dispose();
            Size = resolution;
        }

        public Icon(Icon icon, Size resolution)
        {
            var tmpBm = new Bitmap(icon.Image);
            Path = icon.Path;
            Image = new Bitmap(tmpBm, resolution);
            tmpBm.Dispose();
            Size = resolution;
        }

        public Bitmap ToBitmap()
        {
            return Image;
        }
    }
}
