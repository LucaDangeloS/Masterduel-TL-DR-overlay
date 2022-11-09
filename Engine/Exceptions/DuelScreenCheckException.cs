using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masterduel_TLDR_overlay.Exceptions
{
    class DuelScreenCheckException : Exception
    {
        private static string DEFAULT_MESSAGE = "No duel screen has been detected.";
        public DuelScreenCheckException(string message) : base(message) { }
        public DuelScreenCheckException() : base(DEFAULT_MESSAGE) { }
    }
}
