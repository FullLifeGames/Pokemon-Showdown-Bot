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
    // TODO Deprecated
    class FighterwithoutCalculator : IFighter
    {
        #region Variablen
        private Dictionary<string, Dictionary<string, double>> Typechart;
        private bool rocksSet = false;
        private Dictionary<string, List<string>> dictTypes;
        private Dictionary<string, int> pokemonToNumber;
        private IWebDriver webDriver;
        private Config config;
        private Calculator calculator;
        #endregion

        public FighterwithoutCalculator(Config config)
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

            Debug.WriteLine("Wait For Opponent");
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
                Debug.WriteLine("Wait For Opponent");
                while (waitingForOpponent() || skippingTurnWaiting())
                {
                    Thread.Sleep(50);
                }
                meltPokemonWithNumbers();
                pickPokemonifDefeated();
                Debug.WriteLine("Wait For Opponent");
                while (waitingForOpponent() || skippingTurnWaiting())
                {
                    Thread.Sleep(50);
                }
                pickPokemonifDefeated();
                Debug.WriteLine("Wait For Opponent");
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
            dictTypes = new Dictionary<string, List<string>>();
            string pokemon = "";
            string[] typelines = config.teamtypes.Split('\n');
            int countline = 0;
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
                else if (line.Trim().Equals(""))
                {
                    dictTypes.Add(pokemon, new List<string>());
                    while (!typelines[countline].Trim().Equals(""))
                    {
                        dictTypes[pokemon].Add(typelines[countline].Trim());
                        countline++;
                    }
                    countline++;
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

        private void pickPokemonifDefeated()
        {
            try
            {
                IWebElement whatDo = webDriver.FindElement(By.CssSelector(".whatdo"));
                if (whatDo.Text.Contains("Switch"))
                {
                    Random r = new Random();
                    bool exception = true;
                    List<int> bestPokemon = getBestPossiblePokemon();
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

        private List<int> getBestPossiblePokemon()
        {
            List<string> types = getOpponentsType();
            types = getOpponentsType();
            if (types != null)
            {
                Dictionary<string, double> strength = new Dictionary<string, double>();
                foreach (KeyValuePair<string, List<string>> kv in dictTypes)
                {
                    strength.Add(kv.Key, calcMaxStrength(kv.Value, types));
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
            else
            {
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
        }

        private double calcMaxStrength(List<string> p, List<string> types)
        {
            double maxStrength = 0;
            foreach (string type in p)
            {
                double value = 1.0;
                foreach (string opptype in types)
                {
                    try
                    {
                        Dictionary<string, double> Charttypes = Typechart[opptype.ToLower()];

                        if (Charttypes.ContainsKey(type.ToLower()))
                        {
                            value = value * Charttypes[type.ToLower()];
                        }
                    }
                    catch (Exception)
                    {

                    }
                    if (opptype.ToLower().Contains("levitate") && type.ToLower().Equals("ground"))
                    {
                        value = 0;
                    }
                    if (opptype.ToLower().Contains("dry skin") && type.ToLower().Equals("water"))
                    {
                        value = 0;
                    }
                    if (opptype.ToLower().Contains("flash fire") && type.ToLower().Equals("fire"))
                    {
                        value = 0;
                    }
                    if (opptype.ToLower().Contains("water absorb") && type.ToLower().Equals("water"))
                    {
                        value = 0;
                    }
                    if (opptype.ToLower().Contains("volt absorb") && type.ToLower().Equals("electric"))
                    {
                        value = 0;
                    }
                    if (opptype.ToLower().Contains("lightning rod") && type.ToLower().Equals("electric"))
                    {
                        value = 0;
                    }
                }
                if (maxStrength < value)
                {
                    maxStrength = value;
                }
            }
            return maxStrength;
        }

        private void makeMove()
        {
            try
            {
                List<string> types = getOpponentsType();
                List<string> mytypes = getMyType();

                IWebElement allMoves = webDriver.FindElement(By.CssSelector(".movemenu"));
                ReadOnlyCollection<IWebElement> moves = allMoves.FindElements(By.Name("chooseMove"));

                IWebElement selectedMove = null;
                List<IWebElement> possibleMoves = new List<IWebElement>();
                double superValue = -1;
                foreach (IWebElement move in moves)
                {
                    if (!rocksSet && move.GetAttribute("innerHTML").Contains("Stealth Rock"))
                    {
                        superValue = 1;
                        selectedMove = move;
                        rocksSet = true;
                        break;
                    }
                    else if (move.GetAttribute("innerHTML").Contains("Stealth Rock"))
                    {
                        continue;
                    }

                    string type = move.GetAttribute("class").Substring(5);
                    double value = calcMove(type, types, mytypes);
                    if (value == superValue)
                    {
                        possibleMoves.Add(move);
                    }
                    if (value > superValue)
                    {
                        selectedMove = move;
                        superValue = value;
                        possibleMoves.Clear();
                        possibleMoves.Add(move);
                    }

                }
                if (possibleMoves.Count > 1)
                {
                    selectedMove = possibleMoves[new Random().Next(possibleMoves.Count)];
                }
                if (superValue < 1)
                {
                    if (!tryToSwitch())
                    {
                        selectedMove.Click();
                    }
                }
                else
                {
                    selectedMove.Click();
                }
            }
            catch (Exception)
            {

            }
        }

        private List<string> getMyType()
        {
            IWebElement tooltip = webDriver.FindElement(By.CssSelector(".tooltip"));
            string searchtext = tooltip.GetAttribute("innerHTML");
            string type1 = searchtext.Substring(searchtext.IndexOf("alt") + 5, searchtext.IndexOf("height") - 2 - (searchtext.IndexOf("alt") + 5));
            string type2 = null;
            if (searchtext.Contains("class=\"b\""))
            {
                searchtext = searchtext.Substring(searchtext.IndexOf("<img") + 4);
                searchtext = searchtext.Substring(searchtext.IndexOf("<img") + 4);
                type2 = searchtext.Substring(searchtext.IndexOf("alt") + 5, searchtext.IndexOf("class") - 2 - (searchtext.IndexOf("alt") + 5));
            }
            List<string> types = new List<string>();
            types.Add(type1);
            if (type2 != null)
            {
                types.Add(type2);
            }
            return types;
        }

        private bool tryToSwitch()
        {
            try
            {
                Random r = new Random();
                bool exception = true;
                int count = 0;
                const int maxtrys = 100;
                List<int> bestPokemon = getBestPossiblePokemon();
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

        private double calcMove(string type, List<string> types, List<string> mytypes)
        {
            Debug.WriteLine("Eigener Typ: " + type.ToLower());

            double value = 1.0;
            string gegtypen = "Gegnerische Typen: ";
            foreach (string opptype in types)
            {
                try
                {
                    Dictionary<string, double> Charttypes = Typechart[opptype.ToLower()];
                    gegtypen += opptype.ToLower();
                    // Stab Prio
                    if (mytypes.Contains(type))
                    {
                        value = value * 1.1;
                    }
                    if (Charttypes.ContainsKey(type.ToLower()))
                    {
                        value = value * Charttypes[type.ToLower()];
                    }
                }
                catch (Exception)
                {

                }
                if (opptype.ToLower().Contains("levitate") && type.ToLower().Equals("ground"))
                {
                    value = 0;
                }
                if (opptype.ToLower().Contains("dry skin") && type.ToLower().Equals("water"))
                {
                    value = 0;
                }
                if (opptype.ToLower().Contains("flash fire") && type.ToLower().Equals("fire"))
                {
                    value = 0;
                }
                if (opptype.ToLower().Contains("water absorb") && type.ToLower().Equals("water"))
                {
                    value = 0;
                }
                if (opptype.ToLower().Contains("volt absorb") && type.ToLower().Equals("electric"))
                {
                    value = 0;
                }
                if (opptype.ToLower().Contains("lightning rod") && type.ToLower().Equals("electric"))
                {
                    value = 0;
                }
            }
            Debug.WriteLine(gegtypen);
            return value;
        }

        private List<string> getOpponentsType()
        {
            try
            {
                IWebElement opponent;
                try
                {
                    opponent= webDriver.FindElement(By.CssSelector("div.ps-room:nth-child(47) > div:nth-child(2) > div:nth-child(1)"));
                } 
                catch(Exception)
                {
                    opponent = webDriver.FindElement(By.CssSelector(".foehint > div:nth-child(1)"));
                }
                Actions action = new Actions(webDriver);
                action.MoveToElement(opponent).Perform();
                IWebElement tooltip = webDriver.FindElement(By.CssSelector(".tooltip"));

                string searchtext = tooltip.GetAttribute("innerHTML");
                searchtext = searchtext.Substring(searchtext.IndexOf("<img") + 4);
                string type1 = searchtext.Substring(searchtext.IndexOf("alt") + 5, searchtext.IndexOf("height") - 2 - (searchtext.IndexOf("alt") + 5));
                string type2 = null;

                if (searchtext.Contains("class=\"b\""))
                {
                    searchtext = searchtext.Substring(searchtext.IndexOf("<img") + 4);
                    type2 = searchtext.Substring(searchtext.IndexOf("alt") + 5, searchtext.IndexOf("class") - 2 - (searchtext.IndexOf("alt") + 5));
                }
                string ability = "";
                if (searchtext.Contains("Ability:"))
                {
                    ability = searchtext.Substring(searchtext.IndexOf("Ability:"));
                    ability = ability.Substring(ability.IndexOf(":") + 1, ability.IndexOf("</") - (ability.IndexOf(":") + 1)).Trim();
                }
                else
                {
                    ability = searchtext.Substring(searchtext.IndexOf("abilities:"));
                    ability = ability.Substring(ability.IndexOf(":") + 1, ability.IndexOf("</") - (ability.IndexOf(":") + 1)).Trim();
                }

                List<string> types = new List<string>();
                types.Add(type1);
                if (type2 != null)
                {
                    types.Add(type2);
                }
                if (ability.ToLower().Contains("levitate"))
                {
                    types.Add("levitate");
                }
                if (ability.ToLower().Contains("dry skin"))
                {
                    types.Add("dry skin");
                }
                if (ability.ToLower().Contains("flash fire"))
                {
                    types.Add("flash fire");
                }
                if (ability.ToLower().Contains("water absorb"))
                {
                    types.Add("water absorb");
                }
                if (ability.ToLower().Contains("volt absorb"))
                {
                    types.Add("volt absorb");
                }
                if (ability.ToLower().Contains("lightning rod"))
                {
                    types.Add("lightning rod");
                }
                try
                {
                    IWebElement me = webDriver.FindElement(By.CssSelector("div.ps-room:nth-child(47) > div:nth-child(2) > div:nth-child(2)"));
                    action.MoveToElement(me).Perform();
                }
                catch (Exception)
                { }
                return types;
            }
            catch (Exception)
            {
                return null;
            }
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

    }
}
