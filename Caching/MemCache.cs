using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Masterduel_TLDR_overlay.Masterduel;

namespace Masterduel_TLDR_overlay.Caching
{
    internal class MemCache
    {
        public CardInfo LastLookup { get; }
        public bool CheckInCache(Bitmap bm, float precision)
        {
            return false;
        }
    }
}
