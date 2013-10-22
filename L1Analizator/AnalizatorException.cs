using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L1Analizator
{
    class AnalizatorException : Exception
    {
        public int i, j, code;

        public AnalizatorException(string msg, int i, int j, int code) : base(msg)
        {
            this.i = i;
            this.j = j;
            this.code = code;
        }
    }
}
