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

        public static string CHROMEDRIVER_PATH = @"C:\Users\Benedikt\Documents\GitHub\Pokemon-Showdown-Bot";

        static void Main(string[] args)
        {

            Config config = Config.loadConfig();            
            IFighter fighter;
            if (config.botMethod == null || config.botMethod == "Fighter")
            {
                fighter = new Fighter(config);
                Calculator calculator = new Calculator(config);
                calculator.init(fighter);
                fighter.setCalculator(calculator);
            }
            else if(config.botMethod == null || config.botMethod == "Staller")
            {
                fighter = new Staller(config);
                Calculator calculator = new Calculator(config);
                calculator.init(fighter);
                fighter.setCalculator(calculator);
            }
            else
            {
                fighter = new FighterwithoutCalculator(config);
            }

            Thread controlThread = new Thread(delegate() { control(fighter); });

            controlThread.Start();

            fighter.start();

            Debug.WriteLine("Bot stopped!");

        }

        private static void control(IFighter fighter)
        {
            Debug.WriteLine("Commands are: \n\"stop\": for stopping the bot");

            string command = "";

            while (command != "stop")
            {
                command = Console.ReadLine();
                if (command.Contains("write:"))
                {
                    fighter.addQueue(command.Substring(6).Trim());
                }
                else if (command == "forfeit")
                {
                    fighter.addQueue("/forfeit");
                }
                else if (command == "stop")
                {
                    Debug.WriteLine("Stop will be initiated!");
                }
                else
                {
                    Console.WriteLine("Command not recognized!");
                }
            }

            fighter.stop();
        }

    }
}
