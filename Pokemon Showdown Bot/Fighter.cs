using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pokemon_Showdown_Bot;
using OpenQA.Selenium.Firefox;
using System.Threading;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using OpenQA.Selenium.Interactions;

namespace Pokemon_Showdown_Bot
{
    class Fighter : IFighter
    {
        #region Variablen
        private Dictionary<string, Dictionary<string, double>> Typechart;
        private bool rocksSet = false;
        private Dictionary<string, int> pokemonToNumber;
        private IWebDriver webDriver;
        private Config config;
        private Calculator calculator;
        private const double MAX_DAMAGE_SWITCH_CONST = 20;
        #endregion

        public Fighter(Config config)
        {
            this.config = config;
        }

        public void start()
        {
            setTypeChart();
            setTeam();
            webDriver = new FirefoxDriver(new FirefoxBinary("C:\\Program Files (x86)\\Mozilla Firefox\\Firefox.exe"), new FirefoxProfile());
            init();
            bool running = true;
            while (running)
            {
                rocksSet = false;
                while (findABattle())
                {
                    Thread.Sleep(50);
                }

                Debug.WriteLine("Battle found!");
                battle();

                exitBattle();
            }
        }

        private void init()
        {

            webDriver.Navigate().GoToUrl("http://play.pokemonshowdown.com");

            IWebElement teamBuilder = webDriver.FindElement(By.CssSelector("div.menugroup:nth-child(2) > p:nth-child(1) > button:nth-child(1)"));
            teamBuilder.Click();

            IWebElement newTeam = webDriver.FindElement(By.Name("new"));
            newTeam.Click();

            IWebElement import = webDriver.FindElement(By.Name("import"));
            import.Click();

            ReadOnlyCollection<IWebElement> teamimport = webDriver.FindElements(By.ClassName("textbox"));
            foreach (IWebElement iwe in teamimport)
            {
                string value = iwe.GetAttribute("value");
                if (value == null || value.Equals(""))
                {
                    iwe.SendKeys(config.team);
                    break;
                }
            }

            IWebElement save = webDriver.FindElement(By.Name("saveImport"));
            save.Click();

            ReadOnlyCollection<IWebElement> selectTier = webDriver.FindElements(By.Name("format"));
            foreach (IWebElement iwe in selectTier)
            {
                if (iwe.Text.Contains("None"))
                {
                    iwe.Click();
                    iwe.SendKeys("o");
                    iwe.Click();
                    break;
                }
            }

            ReadOnlyCollection<IWebElement> home = webDriver.FindElements(By.ClassName("button"));
            foreach (IWebElement iwe in home)
            {
                if (iwe.Text.Contains("Home"))
                {
                    iwe.Click();
                    break;
                }
            }

            IWebElement login = webDriver.FindElement(By.Name("login"));
            login.Click();

            IWebElement username = webDriver.FindElement(By.CssSelector("input.textbox:nth-child(1)"));
            username.SendKeys(config.user);

            IWebElement submit = webDriver.FindElement(By.CssSelector(".buttonbar > button:nth-child(1)"));
            submit.Click();

            Thread.Sleep(1000);
            IWebElement pass = webDriver.FindElement(By.CssSelector("input.textbox:nth-child(1)"));
            pass.SendKeys(config.password);

            login = webDriver.FindElement(By.CssSelector("p.buttonbar:nth-child(5) > button:nth-child(1)"));
            login.Click();

            IJavaScriptExecutor js = webDriver as IJavaScriptExecutor;
            js.ExecuteScript("window.app.tryJoinRoom(\"Pokefans\");");

            Thread.Sleep(200);

            IWebElement musik = webDriver.FindElement(By.CssSelector("button.icon:nth-child(2)"));
            musik.Click();
            webDriver.FindElement(By.CssSelector(".ps-popup > p:nth-child(3) > label:nth-child(1) > input:nth-child(1)")).Click();
            musik = webDriver.FindElement(By.CssSelector("button.icon:nth-child(2)"));
            musik.Click();

        }

        private bool findABattle()
        {
            try
            {
                IWebElement format = webDriver.FindElement(By.CssSelector(".formatselect"));
            }
            catch (Exception)
            {
                return false;
            }

            try
            {
                IWebElement lookForABattle = webDriver.FindElement(By.CssSelector(".big"));
                IWebElement format = webDriver.FindElement(By.CssSelector(".formatselect"));
                format.Click();
                IWebElement ou = webDriver.FindElement(By.CssSelector("ul.popupmenu:nth-child(1) > li:nth-child(4) > button:nth-child(1)"));
                ou.Click();
                lookForABattle.Click();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void battle()
        {
            Debug.WriteLine("Picking Start Pokemon");
            pickStartPokemon();

            while (waitingForOpponent() || skippingTurnWaiting())
            {
                Thread.Sleep(50);
            }

            Debug.WriteLine("Game Start");
            while (checkifGameOver())
            {
                meltPokemonWithNumbers();
                Debug.WriteLine("Check Mega Evolve Possibility");
                megaEvolveIfPossible();
                Debug.WriteLine("Make A Move");
                makeMove();
                while (waitingForOpponent() || skippingTurnWaiting())
                {
                    Thread.Sleep(50);
                }
                meltPokemonWithNumbers();
                pickPokemonifDefeated();
                while (waitingForOpponent() || skippingTurnWaiting())
                {
                    Thread.Sleep(50);
                }
                pickPokemonifDefeated();
                while (waitingForOpponent() || skippingTurnWaiting())
                {
                    Thread.Sleep(50);
                }
            }

        }

        private void exitBattle()
        {
            webDriver.FindElement(By.CssSelector(".battle-log-add > form:nth-child(1) > textarea:nth-child(3)")).SendKeys("gg, thanks for playing!" + (char)13);
            try
            {
                webDriver.FindElement(By.CssSelector(".tabbar > div:nth-child(1) > ul:nth-child(2) > li:nth-child(1) > a:nth-child(2) > i:nth-child(1)")).Click();
            }
            catch (Exception)
            {
                Thread.Sleep(100);
                webDriver.FindElement(By.CssSelector(".tabbar > div:nth-child(1) > ul:nth-child(2) > li:nth-child(1) > a:nth-child(2) > i:nth-child(1)")).Click();
            }
        }

        #region Help Functions
        private void setTeam()
        {
            string pokemon = "";
            foreach (string line in config.team.Split('\n'))
            {
                if (line.Contains("(M") || line.Contains("(F"))
                {
                    pokemon = line.Substring(0, line.IndexOf("(")).Trim();
                }
                else if (line.Contains("@"))
                {
                    pokemon = line.Substring(0, line.IndexOf("@")).Trim();
                }
            }
        }

        private void setTypeChart()
        {
            Typechart = new Dictionary<string, Dictionary<string, double>>();
            Typechart.Add("normal", new Dictionary<string, double>());
            Typechart["normal"].Add("ghost", 0);
            Typechart["normal"].Add("fighting", 2);
            Typechart.Add("fighting", new Dictionary<string, double>());
            Typechart["fighting"].Add("flying", 2);
            Typechart["fighting"].Add("rock", 0.5);
            Typechart["fighting"].Add("bug", 0.5);
            Typechart["fighting"].Add("psychic", 2);
            Typechart["fighting"].Add("dark", 0.5);
            Typechart["fighting"].Add("fairy", 2);
            Typechart.Add("flying", new Dictionary<string, double>());
            Typechart["flying"].Add("fighting", 0.5);
            Typechart["flying"].Add("ground", 0);
            Typechart["flying"].Add("rock", 2);
            Typechart["flying"].Add("bug", 0.5);
            Typechart["flying"].Add("grass", 0.5);
            Typechart["flying"].Add("electric", 2);
            Typechart["flying"].Add("ice", 2);
            Typechart.Add("poison", new Dictionary<string, double>());
            Typechart["poison"].Add("fighting", 0.5);
            Typechart["poison"].Add("poison", 0.5);
            Typechart["poison"].Add("ground", 2);
            Typechart["poison"].Add("bug", 0.5);
            Typechart["poison"].Add("grass", 0.5);
            Typechart["poison"].Add("psychic", 2);
            Typechart["poison"].Add("fairy", 0.5);
            Typechart.Add("ground", new Dictionary<string, double>());
            Typechart["ground"].Add("poison", 0.5);
            Typechart["ground"].Add("rock", 0.5);
            Typechart["ground"].Add("water", 2);
            Typechart["ground"].Add("grass", 2);
            Typechart["ground"].Add("electric", 0);
            Typechart["ground"].Add("ice", 2);
            Typechart.Add("rock", new Dictionary<string, double>());
            Typechart["rock"].Add("normal", 0.5);
            Typechart["rock"].Add("fighting", 2);
            Typechart["rock"].Add("flying", 0.5);
            Typechart["rock"].Add("poison", 0.5);
            Typechart["rock"].Add("ground", 2);
            Typechart["rock"].Add("steel", 2);
            Typechart["rock"].Add("fire", 0.5);
            Typechart["rock"].Add("water", 2);
            Typechart["rock"].Add("grass", 2);
            Typechart.Add("bug", new Dictionary<string, double>());
            Typechart["bug"].Add("fighting", 0.5);
            Typechart["bug"].Add("flying", 2);
            Typechart["bug"].Add("ground", 0.5);
            Typechart["bug"].Add("rock", 2);
            Typechart["bug"].Add("fire", 2);
            Typechart["bug"].Add("grass", 0.5);
            Typechart.Add("ghost", new Dictionary<string, double>());
            Typechart["ghost"].Add("normal", 0);
            Typechart["ghost"].Add("fighting", 0);
            Typechart["ghost"].Add("posion", 0.5);
            Typechart["ghost"].Add("bug", 0.5);
            Typechart["ghost"].Add("ghost", 2);
            Typechart["ghost"].Add("dark", 2);
            Typechart.Add("steel", new Dictionary<string, double>());
            Typechart["steel"].Add("normal", 0.5);
            Typechart["steel"].Add("fighting", 2);
            Typechart["steel"].Add("flying", 0.5);
            Typechart["steel"].Add("poison", 0);
            Typechart["steel"].Add("ground", 2);
            Typechart["steel"].Add("rock", 0.5);
            Typechart["steel"].Add("bug", 0.5);
            Typechart["steel"].Add("steel", 0.5);
            Typechart["steel"].Add("fire", 2);
            Typechart["steel"].Add("grass", 0.5);
            Typechart["steel"].Add("psychic", 0.5);
            Typechart["steel"].Add("ice", 0.5);
            Typechart["steel"].Add("dragon", 0.5);
            Typechart["steel"].Add("fairy", 0.5);
            Typechart.Add("fire", new Dictionary<string, double>());
            Typechart["fire"].Add("ground", 2);
            Typechart["fire"].Add("rock", 2);
            Typechart["fire"].Add("bug", 0.5);
            Typechart["fire"].Add("steel", 0.5);
            Typechart["fire"].Add("fire", 0.5);
            Typechart["fire"].Add("water", 2);
            Typechart["fire"].Add("grass", 0.5);
            Typechart["fire"].Add("ice", 0.5);
            Typechart["fire"].Add("fairy", 0.5);
            Typechart.Add("water", new Dictionary<string, double>());
            Typechart["water"].Add("steel", 0.5);
            Typechart["water"].Add("fire", 0.5);
            Typechart["water"].Add("water", 0.5);
            Typechart["water"].Add("grass", 2);
            Typechart["water"].Add("electric", 2);
            Typechart["water"].Add("ice", 0.5);
            Typechart.Add("grass", new Dictionary<string, double>());
            Typechart["grass"].Add("flying", 2);
            Typechart["grass"].Add("poison", 2);
            Typechart["grass"].Add("ground", 0.5);
            Typechart["grass"].Add("bug", 2);
            Typechart["grass"].Add("fire", 2);
            Typechart["grass"].Add("water", 0.5);
            Typechart["grass"].Add("grass", 0.5);
            Typechart["grass"].Add("electric", 0.5);
            Typechart["grass"].Add("ice", 2);
            Typechart.Add("electric", new Dictionary<string, double>());
            Typechart["electric"].Add("flying", 0.5);
            Typechart["electric"].Add("ground", 2);
            Typechart["electric"].Add("steel", 0.5);
            Typechart["electric"].Add("electric", 0.5);
            Typechart.Add("psychic", new Dictionary<string, double>());
            Typechart["psychic"].Add("fighting", 0.5);
            Typechart["psychic"].Add("bug", 2);
            Typechart["psychic"].Add("ghost", 2);
            Typechart["psychic"].Add("psychic", 0.5);
            Typechart["psychic"].Add("dark", 2);
            Typechart.Add("ice", new Dictionary<string, double>());
            Typechart["ice"].Add("fighting", 2);
            Typechart["ice"].Add("rock", 2);
            Typechart["ice"].Add("steel", 2);
            Typechart["ice"].Add("fire", 2);
            Typechart["ice"].Add("ice", 0.5);
            Typechart.Add("dragon", new Dictionary<string, double>());
            Typechart["dragon"].Add("fire", 0.5);
            Typechart["dragon"].Add("water", 0.5);
            Typechart["dragon"].Add("grass", 0.5);
            Typechart["dragon"].Add("electric", 0.5);
            Typechart["dragon"].Add("ice", 2);
            Typechart["dragon"].Add("dragon", 2);
            Typechart["dragon"].Add("fairy", 2);
            Typechart.Add("dark", new Dictionary<string, double>());
            Typechart["dark"].Add("fighting", 2);
            Typechart["dark"].Add("bug", 2);
            Typechart["dark"].Add("ghost", 0.5);
            Typechart["dark"].Add("psychic", 0);
            Typechart["dark"].Add("dark", 0 - 5);
            Typechart["dark"].Add("fairy", 2);
            Typechart.Add("fairy", new Dictionary<string, double>());
            Typechart["fairy"].Add("fighting", 0.5);
            Typechart["fairy"].Add("poison", 2);
            Typechart["fairy"].Add("bug", 0.5);
            Typechart["fairy"].Add("steel", 2);
            Typechart["fairy"].Add("dragon", 0);
            Typechart["fairy"].Add("dark", 0.5);
        }

        private bool skippingTurnWaiting()
        {
            Debug.WriteLine("skippingTurnWaiting");
            try
            {
                IWebElement element = webDriver.FindElement(By.CssSelector("div.ps-room:nth-child(47) > div:nth-child(5) > p:nth-child(1) > button:nth-child(1)"));
                if (element.Text.Contains("turn"))
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        private void megaEvolveIfPossible()
        {
            try
            {
                webDriver.FindElement(By.CssSelector(".movemenu > label:nth-child(6) > input:nth-child(1)")).Click();
            }
            catch (Exception)
            { }
        }

        //TODO Überarbeiten mit Calculator Pick
        private void pickPokemonifDefeated()
        {
            try
            {
                IWebElement whatDo = webDriver.FindElement(By.CssSelector(".whatdo"));
                if (whatDo.Text.Contains("Switch"))
                {
                    Random r = new Random();
                    bool exception = true;
                    List<int> bestPokemon = getBestPossiblePokemonForSwitch();
                    while (exception)
                    {
                        try
                        {
                            int child;
                            if (bestPokemon.Count != 0)
                            {
                                child = bestPokemon[0];
                                bestPokemon.RemoveAt(0);
                            }
                            else
                            {
                                child = r.Next(6) + 1;
                            }
                            IWebElement pokemon = webDriver.FindElement(By.CssSelector(".switchmenu > button:nth-child(" + child + ")"));
                            if (pokemon.GetAttribute("class").Equals("disabled"))
                            {
                                exception = true;
                            }
                            else
                            {
                                exception = false;
                                pokemon.Click();
                            }

                        }
                        catch (Exception)
                        {
                            exception = true;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        //TODO Überarbeiten mit Calculator Pick, Pokemon was schneller ist und Pokemon was am meisten Schaden macht
        private List<int> getBestPossiblePokemonForSwitch()
        {
            string oppsearchtext = null;
            string opp = null;
            double opphealth = -1;
            string oppitem = null;
            try
            {
                oppsearchtext = getOpponentsSearchText();
                opp = getName(oppsearchtext);
                opphealth = getHealth(oppsearchtext);
                oppitem = getItem(oppsearchtext);
            }
            catch (Exception)
            {
            }
            Dictionary<string, double> mypokemon = getMyPokemon();
            // Double Down
            if (opp == null)
            {
                // TODO Bisher mit random implementiert, aber erweiterbar mit der selben Methode, die auch im Start implementiert werden soll
                List<int> numbers = new List<int>();
                for (int i = 1; i <= 6; i++)
                {
                    numbers.Add(i);
                }
                List<int> ret = new List<int>();
                Random r = new Random();
                for (int i = 0; i < 6; i++)
                {
                    int next = r.Next(numbers.Count);
                    ret.Add(numbers[next]);
                    numbers.RemoveAt(next);
                }
                return ret;
            }
            else
            {
                Dictionary<string, double> strength = new Dictionary<string, double>();
                foreach (string pokemon in pokemonToNumber.Keys)
                {
                    Dictionary<string, string>[] damages = calculator.calculate(pokemon, opp, oppitem);
                    Dictionary<string, string> mydamage = damages[0];
                    Dictionary<string, string> oppdamage = damages[1];
                    int[] speedStats = calculator.getSpeedStats(pokemon, opp);

                    string bestmove = "";
                    double maxdamage = -1;
                    double mindamage = -1;
                    foreach (KeyValuePair<string, string> damage in mydamage)
                    {
                        double tempdamage = double.Parse(damage.Value.Substring(damage.Value.IndexOf("-") + 1, damage.Value.IndexOf("%") - (damage.Value.IndexOf("-") + 1)));
                        if (tempdamage > maxdamage)
                        {
                            maxdamage = tempdamage;
                            mindamage = double.Parse(damage.Value.Substring(0, damage.Value.IndexOf("-")));
                            bestmove = damage.Key;
                        }
                    }

                    string oppbestmove = "";
                    double oppmaxdamage = -1;
                    double oppmindamage = -1;
                    foreach (KeyValuePair<string, string> damage in oppdamage)
                    {
                        double tempdamage = double.Parse(damage.Value.Substring(damage.Value.IndexOf("-") + 1, damage.Value.IndexOf("%") - (damage.Value.IndexOf("-") + 1)));
                        if (tempdamage > maxdamage)
                        {
                            oppmaxdamage = tempdamage;
                            oppmindamage = double.Parse(damage.Value.Substring(0, damage.Value.IndexOf("-")));
                            oppbestmove = damage.Key;
                        }
                    }
                    try
                    {
                        if (speedStats[(int)Position.ME] > speedStats[(int)Position.OPPONENT] && (mypokemon[pokemon] - mittelwert(oppmaxdamage, oppmindamage)) > 0 && (opphealth - mittelwert(maxdamage, mindamage)) < 0)
                        {
                            strength.Add(pokemon, 7);
                        }
                        else if (speedStats[(int)Position.ME] > speedStats[(int)Position.OPPONENT] && (mypokemon[pokemon] - mittelwert(oppmaxdamage, oppmindamage)) <= 0 && (opphealth - mittelwert(maxdamage, mindamage)) < 0)
                        {
                            strength.Add(pokemon, 6);
                        }
                        else if (speedStats[(int)Position.ME] <= speedStats[(int)Position.OPPONENT] && (mypokemon[pokemon] - mittelwert(oppmaxdamage, oppmindamage)) > 0 && (opphealth - mittelwert(maxdamage, mindamage)) < 0)
                        {
                            strength.Add(pokemon, 5);
                        }
                        else if (speedStats[(int)Position.ME] > speedStats[(int)Position.OPPONENT] && myPokemonWins(mypokemon[pokemon], maxdamage, mindamage, opphealth, oppmaxdamage, oppmindamage, speedStats[(int)Position.ME] > speedStats[(int)Position.OPPONENT]))
                        {
                            strength.Add(pokemon, 4);
                        }
                        else if (speedStats[(int)Position.ME] <= speedStats[(int)Position.OPPONENT] && myPokemonWins(mypokemon[pokemon], maxdamage, mindamage, opphealth, oppmaxdamage, oppmindamage, speedStats[(int)Position.ME] <= speedStats[(int)Position.OPPONENT]))
                        {
                            strength.Add(pokemon, 3);
                        }
                        else if (speedStats[(int)Position.ME] > speedStats[(int)Position.OPPONENT] && !myPokemonWins(mypokemon[pokemon], maxdamage, mindamage, opphealth, oppmaxdamage, oppmindamage, speedStats[(int)Position.ME] > speedStats[(int)Position.OPPONENT]))
                        {
                            strength.Add(pokemon, 2);
                        }
                        else if (speedStats[(int)Position.ME] <= speedStats[(int)Position.OPPONENT] && !myPokemonWins(mypokemon[pokemon], maxdamage, mindamage, opphealth, oppmaxdamage, oppmindamage, speedStats[(int)Position.ME] <= speedStats[(int)Position.OPPONENT]))
                        {
                            strength.Add(pokemon, 1);
                        }
                        else
                        {
                            strength.Add(pokemon, 0);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                List<KeyValuePair<string, double>> myList = strength.ToList();

                myList.Sort((firstPair, nextPair) =>
                {
                    return nextPair.Value.CompareTo(firstPair.Value);
                });
                List<int> ret = new List<int>();
                for (int i = 0; i < myList.Count; i++)
                {
                    ret.Add(pokemonToNumber[myList[i].Key]);
                }

                return ret;
            }
            
        }

        private bool myPokemonWins(double health, double maxdamage, double mindamage, double opphealth, double oppmaxdamage, double oppmindamage, bool faster)
        {
            double damage = mittelwert(maxdamage, mindamage);
            double oppdamage = mittelwert(oppmaxdamage, oppmindamage);
            double temphealth = health;
            double tempopphealth = opphealth;
            while (temphealth > 0 && tempopphealth > 0)
            {
                temphealth -= oppdamage;
                tempopphealth -= damage;
            }
            if (temphealth <= 0 && tempopphealth <= 0)
            {
                return faster;
            }
            else if (temphealth <= 0)
            {
                return false;
            }
            return true;
        }

        private Dictionary<string, double> getMyPokemon()
        {
            Dictionary<string, double> ret = new Dictionary<string, double>();
            Actions action = new Actions(webDriver);
            for (int i = 1; i <= 6; i++)
            {
                IWebElement pokemon = webDriver.FindElement(By.CssSelector(".switchmenu > button:nth-child(" + i + ")"));
                if (pokemon.GetAttribute("class") != "disabled")
                {
                    action.MoveToElement(pokemon).Perform();

                    IWebElement tooltip = webDriver.FindElement(By.CssSelector(".tooltip"));
                    string searchtext = tooltip.GetAttribute("innerHTML");
                    string name = getName(searchtext);
                    if (name.Contains("-Mega"))
                    {
                        name = name.Substring(0, name.IndexOf("-Mega"));
                    }
                    double health = getHealth(searchtext);
                    ret.Add(name.Trim(), health);
                }
            }
            return ret;
        }

        private void makeMove()
        {
            try
            {
                string oppsearchtext = getOpponentsSearchText();
                string opp = getName(oppsearchtext);
                double opphealth = getHealth(oppsearchtext);
                string oppitem = getItem(oppsearchtext);

                string mysearchtext = getMySearchText();
                string me = getName(mysearchtext);

                Move move = calcBestMove(me, opp, oppitem);                     // Wenn der Mittelwert von min und max damage nicht killt => wechsel
                if (move.maxDamage < MAX_DAMAGE_SWITCH_CONST && ((opphealth - mittelwert(move.maxDamage, move.minDamage)) > 0))
                {
                    if (!tryToSwitch())
                    {
                        move.moveClick.Click();
                    }
                }
                else
                {
                    move.moveClick.Click();
                }
            }
            catch (Exception)
            {

            }
        }

        private Move calcBestMove(string me, string opp, string oppitem)
        {
            Dictionary<string, string>[] damages = calculator.calculate(me, opp, oppitem);
            Dictionary<string, string> mydamage = damages[0];
            Dictionary<string, string> oppdamage = damages[1];
            String[] types = calculator.getMoveTypeAndOpponentsType(me, opp);
            IWebElement allMoves = webDriver.FindElement(By.CssSelector(".movemenu"));
            ReadOnlyCollection<IWebElement> moves = allMoves.FindElements(By.Name("chooseMove"));
            bool canStealthRock = calculator.canStealthRock(me, opp);
            
            List<Move> movesList = new List<Move>();
            foreach (KeyValuePair<string, string> damage in mydamage)
            {
                double tempdamage = double.Parse(damage.Value.Substring(damage.Value.IndexOf("-") + 1, damage.Value.IndexOf("%") - (damage.Value.IndexOf("-") + 1)));
                Move m = new Move();
                m.maxDamage = tempdamage;
                m.minDamage = double.Parse(damage.Value.Substring(0, damage.Value.IndexOf("-")));
                m.moveName = damage.Key;
                movesList.Add(m);
            }
            movesList.OrderBy(move => move.maxDamage);
            double maxdamage = -1;
            double mindamage = -1;
            IWebElement selectedMove = null;
            foreach (IWebElement move in moves)
            {
                if (!rocksSet && canStealthRock && move.GetAttribute("innerHTML").Contains("Stealth Rock"))
                {
                    maxdamage = 21;
                    selectedMove = move;
                    rocksSet = true;
                    break;
                }
                else if (move.GetAttribute("innerHTML").Contains("Stealth Rock"))
                {
                    continue;
                }                
                foreach (Move m in movesList)
                {
                    if (move.GetAttribute("innerHTML").Contains(m.moveName))
                    {
                        if (maxdamage < m.maxDamage)
                        {
                            selectedMove = move;
                            maxdamage = m.maxDamage;
                            mindamage = m.minDamage;
                        }
                    }
                }
            }
            Move ret = new Move();
            ret.moveClick = selectedMove;
            ret.maxDamage = maxdamage;
            ret.minDamage = mindamage;
            return ret;
        }

        private bool tryToSwitch()
        {
            try
            {
                Random r = new Random();
                bool exception = true;
                int count = 0;
                const int maxtrys = 100;
                List<int> bestPokemon = getBestPossiblePokemonForSwitch();
                while (exception)
                {
                    if (count > maxtrys)
                    {
                        return false;
                    }
                    else
                    {
                        try
                        {
                            int child;
                            if (bestPokemon.Count != 0)
                            {
                                child = bestPokemon[0];
                                bestPokemon.RemoveAt(0);
                            }
                            else
                            {
                                child = r.Next(6) + 1;
                            }
                            IWebElement pokemon = webDriver.FindElement(By.CssSelector(".switchmenu > button:nth-child(" + child + ")"));
                            if (pokemon.GetAttribute("class").Equals("disabled"))
                            {
                                exception = true;
                            }
                            else
                            {
                                exception = false;
                                pokemon.Click();
                            }

                        }
                        catch (Exception)
                        {
                            exception = true;
                        }
                    }
                    count++;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private string getMySearchText()
        {
            Actions action = new Actions(webDriver);
            IWebElement me = webDriver.FindElement(By.CssSelector("div.ps-room:nth-child(47) > div:nth-child(2) > div:nth-child(2)"));
            action.MoveToElement(me).Perform();

            IWebElement tooltip = webDriver.FindElement(By.CssSelector(".tooltip"));
            string searchtext = tooltip.GetAttribute("innerHTML");
            return searchtext;
        }
        
        private string getOpponentsSearchText()
        {
            IWebElement opponent;
            try
            {
                opponent = webDriver.FindElement(By.CssSelector("div.ps-room:nth-child(47) > div:nth-child(2) > div:nth-child(1)"));
            }
            catch (Exception)
            {
                opponent = webDriver.FindElement(By.CssSelector(".foehint > div:nth-child(1)"));
            }
            Actions action = new Actions(webDriver);
            action.MoveToElement(opponent).Perform();
            IWebElement tooltip = webDriver.FindElement(By.CssSelector(".tooltip"));

            string searchtext = tooltip.GetAttribute("innerHTML");
            return searchtext;
        }

        private string getItem(string searchtext)
        {
            if (searchtext.Contains("Item:"))
            {
                searchtext = searchtext.Substring(searchtext.IndexOf("Item:") + 5);
                return searchtext.Substring(0, searchtext.IndexOf("</p"));
            }
            else
            {
                return null;
            }
        }

        private double getHealth(string searchtext)
        {
            searchtext = searchtext.Substring(searchtext.IndexOf("HP:") + 3);
            return double.Parse((searchtext.Substring(0, searchtext.IndexOf("%"))).Replace(".",","));
        }

        private string getName(string searchtext)
        {
            string name;
            try
            {
                name = searchtext.Substring(searchtext.IndexOf("<h2>") + 4, searchtext.IndexOf("<small style") - (searchtext.IndexOf("<h2>") + 4));
            }
            catch (Exception)
            {
                name = searchtext.Substring(searchtext.IndexOf("<h2>") + 4, searchtext.IndexOf("<br") - (searchtext.IndexOf("<h2>") + 4));
            }
            if (name.Contains("("))
            {
                name = name.Substring(name.IndexOf("(") + 1, name.IndexOf(")") - (name.IndexOf("(") + 1));
            }
            return name;
        }

        private bool waitingForOpponent()
        {
            Debug.WriteLine("waitingForOpponent");
            try
            {
                IWebElement waiting = webDriver.FindElement(By.CssSelector(".controls > p:nth-child(1) > em:nth-child(1)"));
                if (waiting.GetAttribute("innerHTML").Contains("Waiting"))
                {
                    IWebElement timer = webDriver.FindElement(By.CssSelector(".timer > button:nth-child(1)"));
                    if (timer.GetAttribute("innerHTML").Contains("Start timer"))
                    {
                        timer.Click();
                    }
                    return true;
                }
            }
            catch (Exception)
            { }
            return false;
        }

        private void pickStartPokemon()
        {
            bool exception = true;
            while (exception)
            {
                Thread.Sleep(1000);
                try
                {
                    meltPokemonWithNumbers();

                    int child = pickLeadPokemon();

                    webDriver.FindElement(By.CssSelector(".switchmenu > button:nth-child(" + child + ")")).Click();
                    exception = false;

                    webDriver.FindElement(By.CssSelector(".battle-log-add > form:nth-child(1) > textarea:nth-child(3)")).SendKeys("Gl Hf!" + (char)13);
                }
                catch (Exception)
                {
                    exception = true;
                }
            }
        }

        // TODO Intelligenter Lead
        private int pickLeadPokemon()
        {
            if (!pokemonToNumber.ContainsKey(config.leadPokemon))
            {
                Random r = new Random();
                return r.Next(6) + 1;
            }
            else
            {
                return pokemonToNumber[config.leadPokemon];
            }
        }

        private void meltPokemonWithNumbers()
        {
            pokemonToNumber = new Dictionary<string, int>();
            for (int i = 1; i <= 6; i++)
            {
                try
                {
                    IWebElement pokemon = webDriver.FindElement(By.CssSelector(".switchmenu > button:nth-child(" + i + ")"));
                    pokemonToNumber.Add(pokemon.Text, i);
                }
                catch (Exception)
                {

                }
            }
        }

        private bool checkifGameOver()
        {
            Debug.WriteLine("checkifGameOver");
            try
            {
                IWebElement messageBar = webDriver.FindElement(By.CssSelector(".messagebar"));
                if (messageBar.Text.Contains("won the battle!") || messageBar.Text.Contains("lost the battle!"))
                {
                    return false;
                }
            }
            catch (Exception)
            { }
            return true;
        }

        #endregion
        public void setCalculator(Calculator calculator)
        {
            this.calculator = calculator;
        }

        private double mittelwert(double a, double b)
        {
            return (a + b) / 2;
        }

    }
}
