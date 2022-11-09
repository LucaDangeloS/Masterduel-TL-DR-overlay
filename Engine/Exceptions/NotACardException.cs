using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masterduel_TLDR_overlay.Exceptions
{
    class NotACardException : Exception
    {
        private static readonly string DEFAULT_MESSAGE = "The zone checked does not correspond to a valid card.";
        public NotACardException() : base(DEFAULT_MESSAGE)  { }
    }
}
