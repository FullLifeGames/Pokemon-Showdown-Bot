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
            IFighter fighter;
            if (config.botMethod == null || config.botMethod == "Fighter")
            {
                fighter = new Fighter(config);
                Calculator calculator = new Calculator(config);
                calculator.init(fighter);
                fighter.setCalculator(calculator);
            }
            else
            {
                fighter = new FighterwithoutCalculator(config);
            }
            Thread runThread = new Thread(fighter.start);

            runThread.Start();

            Debug.WriteLine("Commands are: \n\"stop\": for stopping the bot");

            string command = Console.ReadLine();

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

            runThread.Join();

            Debug.WriteLine("Bot stopped!");

        }

    }
}
