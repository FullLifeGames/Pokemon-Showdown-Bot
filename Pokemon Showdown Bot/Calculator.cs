using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private List<string> scarfers;

        public Calculator(Config config)
        {
            this.config = config;
        }

        public void init(IFighter fighter)
        {
            initMagicBouncePokemon();

            calculator = new FirefoxDriver();
            calculator.Navigate().GoToUrl("http://fsibapt.github.io/");
            
            IWebElement teamimport = calculator.FindElement(By.CssSelector(".import-team-text"));
            teamimport.SendKeys(config.team);

            IWebElement teamsave = calculator.FindElement(By.CssSelector(".bs-btn"));
            teamsave.Click();

            IAlert alert = calculator.SwitchTo().Alert();

            alert.Accept(); //for two buttons, choose the affirmative one

            setScarfers();

            this.fighter = fighter;
        }

        private void setScarfers()
        {
            scarfers = new List<string>();
            foreach (string line in config.team.Split('\n'))
            {
                if (line.Contains("Choice Scarf"))
                {
                    if (line.Contains("(M") || line.Contains("(F"))
                    {
                        scarfers.Add(changeName(line.Substring(0, line.IndexOf("(")).Trim()));
                    }
                    else if (line.Contains("@"))
                    {
                        scarfers.Add(changeName(line.Substring(0, line.IndexOf("@")).Trim()));
                    }
                }
            }
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
            string move1Text = move1.Text;
            if (move1Text != "(No Move)")
            {
                mydamage.Add(move1Text, damage1.Text.Replace(".", ","));
            }
            string move2Text = move2.Text;
            if (move2Text != "(No Move)")
            {
                mydamage.Add(move2Text, damage2.Text.Replace(".", ","));
            }
            string move3Text = move3.Text;
            if (move3Text != "(No Move)")
            {
                mydamage.Add(move3Text, damage3.Text.Replace(".", ","));
            }
            string move4Text = move4.Text;
            if (move4Text != "(No Move)")
            {
                mydamage.Add(move4Text, damage4.Text.Replace(".", ","));
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
            string oppmove1Text = oppmove1.Text;
            if (oppmove1Text != "(No Move)")
            {
                oppdamage.Add(oppmove1Text, oppdamage1.Text.Replace(".", ","));
            }
            string oppmove2Text = oppmove2.Text;
            if (oppmove2Text != "(No Move)")
            {
                oppdamage.Add(oppmove2Text, oppdamage2.Text.Replace(".", ","));
            }
            string oppmove3Text = oppmove3.Text;
            if (oppmove3Text != "(No Move)")
            {
                oppdamage.Add(oppmove3Text, oppdamage3.Text.Replace(".", ","));
            }
            string oppmove4Text = oppmove4.Text;
            if (oppmove4Text != "(No Move)")
            {
                oppdamage.Add(oppmove4Text, oppdamage4.Text.Replace(".", ","));
            }

            excludeIntimidate();

            return new Dictionary<string,string>[] { mydamage, oppdamage };
        }

        private void excludeIntimidate()
        {
            SelectElement select = new SelectElement(calculator.FindElement(By.CssSelector("#p2 > div:nth-child(6) > div:nth-child(2) > select:nth-child(2)")));
            string ability = getSelectedOption(select).Text;
            if (ability == "Intimidate")
            {
                select.SelectByValue("");
            }
        }

        private static IWebElement getSelectedOption(SelectElement select)
        {
            IWebElement selectedoption = null;
            Parallel.ForEach(select.Options, (option, state) =>
            {
                if (option.Selected)
                {
                    selectedoption = option;
                    state.Break();
                }
            });
            return selectedoption;
        }

        private void setPokemon(string myPokemonRaw, string opponentsPokemonRaw, string oppitem = null)
        {

            string myPokemon = myPokemonRaw.Replace("-Resolute", "").Trim();
            string opponentsPokemon = opponentsPokemonRaw.Replace("-Resolute", "").Trim();

            myPokemon = changeName(myPokemon);
            opponentsPokemon = changeName(opponentsPokemon);

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
                opponentText.SendKeys(opponentsPokemon);

                IWebElement opponentResults = calculator.FindElement(By.CssSelector("#select2-drop > ul:nth-child(2)"));
                ReadOnlyCollection<IWebElement> oppresults = opponentResults.FindElements(By.TagName("li"));
                bool hasOuSet = false;
                IWebElement tempresult = null;
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
                lastLoadedOpp = opponentsPokemon;

                excludeIntimidate();

                if (oppitem != null)
                {
                    SelectElement select = new SelectElement(calculator.FindElement(By.CssSelector("#p2 > div:nth-child(6) > div:nth-child(3) > select:nth-child(2)")));
                    select.SelectByValue(oppitem.Trim());
                }
            }
        }

        private string changeName(string poke)
        {
            switch (poke)
            {
                case "Houndoom-Mega":
                    poke = "Mega Houndoom";
                    break;
                case "Venusaur-Mega":
                    poke = "Mega Venusaur";
                    break;
                case "Blastoise-Mega":
                    poke = "Mega Blastoise";
                    break;
                case "Alakazam-Mega":
                    poke = "Mega Alakazam";
                    break;
                case "Gengar-Mega":
                    poke = "Mega Gengar";
                    break;
                case "Kangaskhan-Mega":
                    poke = "Mega Kangaskhan";
                    break;
                case "Pinsir-Mega":
                    poke = "Mega Pinsir";
                    break;
                case "Gyarados-Mega":
                    poke = "Mega Gyarados";
                    break;
                case "Aerodactyl-Mega":
                    poke = "Mega Aerodactyl";
                    break;
                case "Ampharos-Mega":
                    poke = "Mega Ampharos";
                    break;
                case "Scizor-Mega":
                    poke = "Mega Scizor";
                    break;
                case "Heracross-Mega":
                    poke = "Mega Heracross";
                    break;
                case "Tyranitar-Mega":
                    poke = "Mega Tyranitar";
                    break;
                case "Blaziken-Mega":
                    poke = "Mega Blaziken";
                    break;
                case "Gardevoir-Mega":
                    poke = "Mega Gardevoir";
                    break;
                case "Mawile-Mega":
                    poke = "Mega Mawile";
                    break;
                case "Aggron-Mega":
                    poke = "Mega Aggron";
                    break;
                case "Medicham-Mega":
                    poke = "Mega Medicham";
                    break;
                case "Manectric-Mega":
                    poke = "Mega Manectric";
                    break;
                case "Banette-Mega":
                    poke = "Mega Banette";
                    break;
                case "Absol-Mega":
                    poke = "Mega Absol";
                    break;
                case "Garchomp-Mega":
                    poke = "Mega Garchomp";
                    break;
                case "Lucario-Mega":
                    poke = "Mega Lucario";
                    break;
                case "Beedrill-Mega":
                    poke = "Mega Beedrill";
                    break;
                case "Pidgeot-Mega":
                    poke = "Mega Pidgeot";
                    break;
                case "Slowbro-Mega":
                    poke = "Mega Slowbro";
                    break;
                case "Steelix-Mega":
                    poke = "Mega Steelix";
                    break;
                case "Sceptile-Mega":
                    poke = "Mega Sceptile";
                    break;
                case "Swampert-Mega":
                    poke = "Mega Swampert";
                    break;
                case "Sableye-Mega":
                    poke = "Mega Sableye";
                    break;
                case "Sharpedo-Mega":
                    poke = "Mega Sharpedo";
                    break;
                case "Camerupt-Mega":
                    poke = "Mega Camerupt";
                    break;
                case "Altaria-Mega":
                    poke = "Mega Altaria";
                    break;
                case "Salamence-Mega":
                    poke = "Mega Salamence";
                    break;
                case "Metagross-Mega":
                    poke = "Mega Metagross";
                    break;
                case "Latias-Mega":
                    poke = "Mega Latias";
                    break;
                case "Latios-Mega":
                    poke = "Mega Latios";
                    break;
                case "Rayquaza-Mega":
                    poke = "Mega Rayquaza";
                    break;
                case "Lopunny-Mega":
                    poke = "Mega Lopunny";
                    break;
                case "Gallade-Mega":
                    poke = "Mega Gallade";
                    break;
                case "Audino-Mega":
                    poke = "Mega Audino";
                    break;
                case "Diancie-Mega":
                    poke = "Mega Diancie";
                    break;
                case "Charizard-Mega-X":
                    poke = "Mega Charizard X";
                    break;
                case "Charizard-Mega-Y":
                    poke = "Mega Charizard Y";
                    break;
                case "Mewtwo-Mega-X":
                    poke = "Mega Mewtwo X";
                    break;
                case "Mewtwo-Mega-Y":
                    poke = "Mega Mewtwo Y";
                    break;
                case "Groudon-Primal":
                    poke = "Primal Groudon";
                    break;
                case "Kyogre-Primal":
                    poke = "Primal Kyogre";
                    break;
                case "Rotom-Fan":
                    poke = "Rotom-S";
                    break;
                case "Rotom-Mow":
                    poke = "Rotom-C";
                    break;
                case "Rotom-Frost":
                    poke = "Rotom-F";
                    break;
                case "Rotom-Wash":
                    poke = "Rotom-W";
                    break;
                case "Rotom-Heat":
                    poke = "Rotom-H";
                    break;
                case "Meowstic-F":
                    poke = "Meowstic";
                    break;
                case "Kyurem-Black":
                    poke = "Kyurem-B";
                    break;
                case "Kyurem-White":
                    poke = "Kyurem-W";
                    break;
                case "Landorus-Therian":
                    poke = "Landorus-T";
                    break;
                case "Tornadus-Therian":
                    poke = "Tornadus-T";
                    break;
                case "Thundurus-Therian":
                    poke = "Thundurus-T";
                    break;
                case "Giratina-Origin":
                    poke = "Giratina-O";
                    break;
                case "Gourgeist":
                    poke = "Gourgeist-Average";
                    break;
                case "Shaymin-Sky":
                    poke = "Shaymin-S";
                    break;
                case "Wormadam-Sandy":
                    poke = "Wormadam-G";
                    break;
                case "Wormadam-Trash":
                    poke = "Wormadam-S";
                    break;
                case "Deoxys-Attack":
                    poke = "Deoxys-A";
                    break;
                case "Deoxys-Defense":
                    poke = "Deoxys-D";
                    break;
                case "Deoxys-Speed":
                    poke = "Deoxys-S";
                    break;
                case "Aegislash":
                    poke = "Aegislash-Blade";
                    break;
                case "Pikachu-Belle":
                case "Pikachu-Cosplay":
                case "Pikachu-Libre":
                case "Pikachu-PhD":
                case "Pikachu-Pop-Star":
                case "Pikachu-Rock-Star":
                    poke = "Pikachu";
                    break;
            }
            return poke;
        }        

        public bool canStealthRock(string me, string opp)
        {
            setPokemon(me, opp);

            SelectElement select = new SelectElement(calculator.FindElement(By.CssSelector("#p1 > div:nth-child(6) > div:nth-child(2) > select:nth-child(2)")));
            string ability = getSelectedOption(select).Text;
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

            string type1 = getSelectedOption(new SelectElement(calculator.FindElement(By.CssSelector("#p1 > div:nth-child(8) > select:nth-child(4)")))).Text;
            string type2 = getSelectedOption(new SelectElement(calculator.FindElement(By.CssSelector("#p1 > div:nth-child(9) > select:nth-child(4)")))).Text;
            string type3 = getSelectedOption(new SelectElement(calculator.FindElement(By.CssSelector("#p1 > div:nth-child(10) > select:nth-child(4)")))).Text;
            string type4 = getSelectedOption(new SelectElement(calculator.FindElement(By.CssSelector("#p1 > div:nth-child(11) > select:nth-child(4)")))).Text;

            string opptype1 = getSelectedOption(new SelectElement(calculator.FindElement(By.CssSelector("#p2 > div:nth-child(4) > div:nth-child(1) > select:nth-child(2)")))).Text;
            string opptype2 = getSelectedOption(new SelectElement(calculator.FindElement(By.CssSelector("#p2 > div:nth-child(4) > div:nth-child(1) > select:nth-child(3)")))).Text;

            return new string[] { type1, type2, type3, type4, opptype1, opptype2 };
        }

        public int[] getSpeedStats(string me, string opp)
        {
            setPokemon(me, opp);

            int mySpeed = int.Parse(calculator.FindElement(By.CssSelector("#p1 > div:nth-child(5) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(8) > td:nth-child(6) > span:nth-child(1)")).Text);
            int oppSpeed = int.Parse(calculator.FindElement(By.CssSelector("#p2 > div:nth-child(5) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(8) > td:nth-child(6) > span:nth-child(1)")).Text);

            if (scarfers.Contains(changeName(me)))
            {
                mySpeed = (int)(mySpeed * 1.5);
            }

            return new int[] { mySpeed, oppSpeed };
        }

        public void exit()
        {
            calculator.Close();
        }
    }
}