using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Masterduel_TLDR_overlay.Masterduel.MasterduelWindow.Window;

namespace Masterduel_TLDR_overlay.Overlay
{
    internal class Icons
    {
        protected class OverlayIconStart : RelPos
        {  // 28 x 28
            public float X_REL_INIT_POS => 0.11375f; //182
            public float Y_REL_INIT_POS => 0.3644f; //358
            public float X_REL_END_POS => 0.1312f; // + 28
            public float Y_REL_END_POS => 0.3955f; // + 28
        };
    }
}
