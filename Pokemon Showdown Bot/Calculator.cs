using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokemon_Showdown_Bot
{
    class Calculator
    {

        private IWebDriver calculator;
        private IFighter fighter;
        private Config config;
        private List<string> magicBouncePokemon;
        private string lastLoadedMe;
        private string lastLoadedOpp;

        public Calculator(Config config)
        {
            this.config = config;
        }

        public void init(IFighter fighter)
        {
            initMagicBouncePokemon();

            calculator = new FirefoxDriver(new FirefoxBinary("C:\\Program Files (x86)\\Mozilla Firefox\\Firefox.exe"), new FirefoxProfile());
            calculator.Navigate().GoToUrl("http://fsibapt.github.io/");
            
            IWebElement teamimport = calculator.FindElement(By.CssSelector(".import-team-text"));
            teamimport.SendKeys(config.team);

            IWebElement teamsave = calculator.FindElement(By.CssSelector(".bs-btn"));
            teamsave.Click();

            IAlert alert = calculator.SwitchTo().Alert();

            alert.Accept(); //for two buttons, choose the affirmative one

            this.fighter = fighter;
        }

        private void initMagicBouncePokemon()
        {
            magicBouncePokemon = new List<string>();
            magicBouncePokemon.Add("Natu");
            magicBouncePokemon.Add("Xatu");
            magicBouncePokemon.Add("Espeon");
            magicBouncePokemon.Add("Sableye");
            magicBouncePokemon.Add("Absol");
            magicBouncePokemon.Add("Diancie");
        }

        public Dictionary<string,string>[] calculate(string myPokemon, string opponentsPokemon, string oppitem = null)
        {

            setPokemon(myPokemon, opponentsPokemon, oppitem);

            IWebElement move1 = calculator.FindElement(By.CssSelector("div.move-result-subgroup:nth-child(1) > div:nth-child(2) > label:nth-child(2)"));
            IWebElement damage1 = calculator.FindElement(By.Id("resultDamageL1"));

            IWebElement move2 = calculator.FindElement(By.CssSelector("div.move-result-subgroup:nth-child(1) > div:nth-child(3) > label:nth-child(2)"));
            IWebElement damage2 = calculator.FindElement(By.Id("resultDamageL2"));

            IWebElement move3 = calculator.FindElement(By.CssSelector("div.move-result-subgroup:nth-child(1) > div:nth-child(4) > label:nth-child(2)"));
            IWebElement damage3 = calculator.FindElement(By.Id("resultDamageL3"));

            IWebElement move4 = calculator.FindElement(By.CssSelector("div.move-result-subgroup:nth-child(1) > div:nth-child(5) > label:nth-child(2)"));
            IWebElement damage4 = calculator.FindElement(By.Id("resultDamageL4"));

            Dictionary<string, string> mydamage = new Dictionary<string, string>();
            if (move1.Text != "(No Move)")
            {
                mydamage.Add(move1.Text, damage1.Text.Replace(".", ","));
            }
            if (move2.Text != "(No Move)")
            {
                mydamage.Add(move2.Text, damage2.Text.Replace(".", ","));
            }
            if (move3.Text != "(No Move)")
            {
                mydamage.Add(move3.Text, damage3.Text.Replace(".", ","));
            }
            if (move4.Text != "(No Move)")
            {
                mydamage.Add(move4.Text, damage4.Text.Replace(".", ","));
            }

            IWebElement oppmove1 = calculator.FindElement(By.CssSelector("div.move-result-subgroup:nth-child(2) > div:nth-child(2) > label:nth-child(2)"));
            IWebElement oppdamage1 = calculator.FindElement(By.Id("resultDamageR1"));

            IWebElement oppmove2 = calculator.FindElement(By.CssSelector("div.move-result-subgroup:nth-child(2) > div:nth-child(3) > label:nth-child(2)"));
            IWebElement oppdamage2 = calculator.FindElement(By.Id("resultDamageR2"));

            IWebElement oppmove3 = calculator.FindElement(By.CssSelector("div.move-result-subgroup:nth-child(2) > div:nth-child(4) > label:nth-child(2)"));
            IWebElement oppdamage3 = calculator.FindElement(By.Id("resultDamageR3"));

            IWebElement oppmove4 = calculator.FindElement(By.CssSelector("div.move-result-subgroup:nth-child(2) > div:nth-child(5) > label:nth-child(2)"));
            IWebElement oppdamage4 = calculator.FindElement(By.Id("resultDamageR4"));

            Dictionary<string, string> oppdamage = new Dictionary<string, string>();
            if (oppmove1.Text != "(No Move)")
            {
                oppdamage.Add(oppmove1.Text, oppdamage1.Text.Replace(".",","));
            }
            if (oppmove2.Text != "(No Move)")
            {
                oppdamage.Add(oppmove2.Text, oppdamage2.Text.Replace(".", ","));
            }
            if (oppmove3.Text != "(No Move)")
            {
                oppdamage.Add(oppmove3.Text, oppdamage3.Text.Replace(".", ","));
            }
            if (oppmove4.Text != "(No Move)")
            {
                oppdamage.Add(oppmove4.Text, oppdamage4.Text.Replace(".", ","));
            }

            excludeIntimidate();

            return new Dictionary<string,string>[] { mydamage, oppdamage };
        }

        private void excludeIntimidate()
        {
            SelectElement select = new SelectElement(calculator.FindElement(By.CssSelector("#p2 > div:nth-child(6) > div:nth-child(2) > select:nth-child(2)")));
            string ability = select.SelectedOption.Text;
            if (ability == "Intimidate")
            {
                select.SelectByValue("");
            }
        }

        private void setPokemon(string myPokemonRaw, string opponentsPokemonRaw, string oppitem = null)
        {

            string myPokemon = myPokemonRaw.Replace("-Mega", "").Replace("-Resolute", "").Trim();
            string opponentsPokemon = opponentsPokemonRaw.Replace("-Mega", "").Replace("-Resolute", "").Trim();

            if (myPokemon.Contains("-"))
            {
                myPokemon = myPokemon.Substring(0, myPokemon.IndexOf("-") + 2);
            }
            if (opponentsPokemon.Contains("-"))
            {
                opponentsPokemon = opponentsPokemon.Substring(0, opponentsPokemon.IndexOf("-") + 2);
            }

            if (lastLoadedMe != myPokemon)
            {
                IWebElement myControl = calculator.FindElement(By.CssSelector("#s2id_autogen1 > a:nth-child(1)"));
                myControl.Click();

                IWebElement myText = calculator.FindElement(By.CssSelector("#select2-drop > div:nth-child(1) > input:nth-child(1)"));
                myText.SendKeys(myPokemon);

                IWebElement myResults = calculator.FindElement(By.CssSelector("#select2-drop > ul:nth-child(2)"));
                ReadOnlyCollection<IWebElement> results = myResults.FindElements(By.TagName("li"));
                foreach (IWebElement result in results)
                {
                    if (result.GetAttribute("innerHTML").Contains("Custom Set"))
                    {
                        result.Click();
                        break;                    
                    }
                }
                lastLoadedMe = myPokemon;
            }

            if (lastLoadedOpp != opponentsPokemon)
            {
                IWebElement opponentControl = calculator.FindElement(By.CssSelector("#s2id_autogen3 > a:nth-child(1)"));
                opponentControl.Click();

                IWebElement opponentText = calculator.FindElement(By.CssSelector("#select2-drop > div:nth-child(1) > input:nth-child(1)"));
                opponentText.SendKeys("Mega " + opponentsPokemon.Replace("-", " "));
                bool hasMega = false;
                IWebElement opponentResults = calculator.FindElement(By.CssSelector("#select2-drop > ul:nth-child(2)"));
                ReadOnlyCollection<IWebElement> oppresults = opponentResults.FindElements(By.TagName("li"));
                IWebElement tempresult = null;
                bool hasOuSet = false;
                foreach (IWebElement result in oppresults)
                {
                    if (result.Text.Contains("OU"))
                    {
                        tempresult = result;
                        hasOuSet = true;
                        break;
                    }
                    else
                    {
                        if (!hasOuSet && tempresult == null)
                        {
                            tempresult = result;
                        }
                    }
                }
                if (opponentsPokemonRaw.Contains("-Mega") && tempresult != null)
                {
                    hasMega = true;
                    tempresult.Click();
                }
                if (!hasMega)
                {
                    opponentControl = calculator.FindElement(By.CssSelector("#s2id_autogen3 > a:nth-child(1)"));
                    opponentControl.Click();
                    opponentControl.Click();
                    opponentText.SendKeys(opponentsPokemon);

                    opponentResults = calculator.FindElement(By.CssSelector("#select2-drop > ul:nth-child(2)"));
                    oppresults = opponentResults.FindElements(By.TagName("li"));
                    hasOuSet = false;
                    tempresult = null;
                    bool skipfirst = true;
                    foreach (IWebElement result in oppresults)
                    {
                        if (skipfirst)
                        {
                            skipfirst = false;
                            continue;
                        }
                        if (result.Text.Contains("OU"))
                        {
                            tempresult = result;
                            hasOuSet = true;
                            break;
                        }
                        else
                        {
                            if (!hasOuSet && tempresult == null)
                            {
                                tempresult = result;
                            }
                        }
                    }
                    if (tempresult != null)
                    {
                        tempresult.Click();
                    }
                }
                lastLoadedOpp = opponentsPokemon;

                if (oppitem != null)
                {
                    SelectElement select = new SelectElement(calculator.FindElement(By.CssSelector("#p2 > div:nth-child(6) > div:nth-child(3) > select:nth-child(2)")));
                    select.SelectByValue(oppitem.Trim());
                }
            }
        }        

        public bool canStealthRock(string me, string opp)
        {
            setPokemon(me, opp);

            SelectElement select = new SelectElement(calculator.FindElement(By.CssSelector("#p1 > div:nth-child(6) > div:nth-child(2) > select:nth-child(2)")));
            string ability = select.SelectedOption.Text;
            bool oppHasMagicBounce = magicBouncePokemon.Contains(opp);
            if (oppHasMagicBounce && !ability.Contains("Mold Breaker"))
            {
                return false;
            }
            return true;
        }

        public string[] getMoveTypeAndOpponentsType(string me, string opp)
        {
            setPokemon(me, opp);

            string type1 = (new SelectElement(calculator.FindElement(By.CssSelector("#p1 > div:nth-child(8) > select:nth-child(4)")))).SelectedOption.Text;
            string type2 = (new SelectElement(calculator.FindElement(By.CssSelector("#p1 > div:nth-child(9) > select:nth-child(4)")))).SelectedOption.Text;
            string type3 = (new SelectElement(calculator.FindElement(By.CssSelector("#p1 > div:nth-child(10) > select:nth-child(4)")))).SelectedOption.Text;
            string type4 = (new SelectElement(calculator.FindElement(By.CssSelector("#p1 > div:nth-child(11) > select:nth-child(4)")))).SelectedOption.Text;

            string opptype1 = (new SelectElement(calculator.FindElement(By.CssSelector("#p2 > div:nth-child(4) > div:nth-child(1) > select:nth-child(2)")))).SelectedOption.Text;
            string opptype2 = (new SelectElement(calculator.FindElement(By.CssSelector("#p2 > div:nth-child(4) > div:nth-child(1) > select:nth-child(3)")))).SelectedOption.Text;

            return new string[] { type1, type2, type3, type4, opptype1, opptype2 };
        }

        public int[] getSpeedStats(string me, string opp)
        {
            setPokemon(me, opp);

            int mySpeed = int.Parse(calculator.FindElement(By.CssSelector("#p1 > div:nth-child(5) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(8) > td:nth-child(6) > span:nth-child(1)")).Text);
            int oppSpeed = int.Parse(calculator.FindElement(By.CssSelector("#p2 > div:nth-child(5) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(8) > td:nth-child(6) > span:nth-child(1)")).Text);

            if ((new SelectElement(calculator.FindElement(By.CssSelector("#p1 > div:nth-child(6) > div:nth-child(3) > select:nth-child(2)")))).SelectedOption.Text == "Choice Scarf")
            {
                mySpeed = (int)(mySpeed * 1.5);
            }
            if ((new SelectElement(calculator.FindElement(By.CssSelector("#p2 > div:nth-child(6) > div:nth-child(3) > select:nth-child(2)")))).SelectedOption.Text == "Choice Scarf")
            {
                oppSpeed = (int)(oppSpeed * 1.5);
            }

            return new int[] { mySpeed, oppSpeed };
        }

        public void exit()
        {
            calculator.Close();
        }
    }
}