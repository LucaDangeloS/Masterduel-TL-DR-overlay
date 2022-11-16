using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLDROverlay.Overlay.Icons
{
    public class Icon
    {
        public Size IconSize { get; }
        public Bitmap Image { get; }
        public string Path { get; }

        public Icon(string path, Size resolution)
        {
            var tmpBm = new Bitmap(path);
            Path = path;
            Image = new Bitmap(tmpBm, resolution);
            tmpBm.Dispose();
            IconSize = resolution;
        }
    }
}
