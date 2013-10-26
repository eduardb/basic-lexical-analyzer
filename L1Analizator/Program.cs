using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L1Analizator
{
    class Program : System.Object
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                System.Console.WriteLine("Dati numele fisierului de intrare!");
                Environment.ExitCode = -1;
                return;
            }


            if (!File.Exists(args[0]))
            {
                System.Console.WriteLine("Fisierul nu exista!");
                Environment.ExitCode = -2;
                return;
            }

            Analizator analizator = new Analizator(args[0]);
            analizator.init();
            analizator.run();

#if DEBUG
            System.Console.ReadKey();
#endif
        }
    }
}
