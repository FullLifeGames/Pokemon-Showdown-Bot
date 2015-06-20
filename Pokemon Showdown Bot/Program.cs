using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pokemon_Showdown_Bot
{
    class Program
    {

        static void Main(string[] args)
        {

            Config config = Config.loadConfig();
            Fighter fighter = new Fighter(config);
            Calculator calculator = new Calculator(config);
            calculator.init(fighter);
            fighter.setCalculator(calculator);
            fighter.start();
            
        }

    }
}
