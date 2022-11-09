using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masterduel_TLDR_overlay.Exceptions
{
    public class CardNameIsChangedException : Exception
    {
        private static readonly string DEFAULT_MESSAGE = "Card name is changed";
        public CardNameIsChangedException(string message) : base(message) { }
        public CardNameIsChangedException() : base(DEFAULT_MESSAGE) { }
    }
}
