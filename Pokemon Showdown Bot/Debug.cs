using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokemon_Showdown_Bot
{
    class Debug
    {

        public const bool DEBUG = true;
        private static string lastPrint = "";

        public static void WriteLine(string s)
        {
            if (DEBUG && !s.Equals(lastPrint))
            {
                lastPrint = s;
                Console.WriteLine("Debug: " + s);
            }
        }

    }
}
