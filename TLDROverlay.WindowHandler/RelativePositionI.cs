using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLDROverlay.WindowHandler
{
    public interface IRelativePosition
    {
        public float X_REL_INIT_POS { get; }
        public float Y_REL_INIT_POS { get; }
        public float X_REL_END_POS { get; }
        public float Y_REL_END_POS { get; }
    }
}
