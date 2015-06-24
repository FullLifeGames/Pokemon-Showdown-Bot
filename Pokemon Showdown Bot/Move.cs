using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokemon_Showdown_Bot
{
    class Move
    {
        public IWebElement moveClick { get; set; }
        public string moveName { get; set; }
        public double maxDamage { get; set; }
        public double minDamage { get; set; }
    }
}
