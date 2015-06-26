using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pokemon_Showdown_Bot
{
    interface IFighter
    {
        void setCalculator(Calculator calculator);
        void start();
        void stop();
        void addQueue(string p);
    }
}
