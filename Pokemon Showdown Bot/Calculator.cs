using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokemon_Showdown_Bot
{
    class Calculator
    {

        private IWebDriver calculator;
        private Fighter fighter;
        private Config config;

        public Calculator(Config config)
        {
            this.config = config;
        }

        public void init(Fighter fighter)
        {
            calculator = new FirefoxDriver();
            calculator.Navigate().GoToUrl("http://fsibapt.github.io/");

            IWebElement teamimport = calculator.FindElement(By.CssSelector(".import-team-text"));
            teamimport.SendKeys(config.team);

            IWebElement teamsave = calculator.FindElement(By.CssSelector(".bs-btn"));
            teamsave.Click();

            IAlert alert = calculator.SwitchTo().Alert();

            alert.Accept(); //for two buttons, choose the affirmative one

            this.fighter = fighter;
        }

    }
}