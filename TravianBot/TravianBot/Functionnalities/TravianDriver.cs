using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TravianBot.Ressources;
using TravianBot.Functionnalities.Enums;
using TravianBot.Functionnalities;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace TravianBot
{
    [TestClass]
    public class Functionalities
    {
        static IWebDriver chromeDriver;
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static int currentWood = 0;
        static int currentClay = 0;
        static int currentIron = 0;
        static int currentWheat = 0;

        static string username;
        static string password;

        static readonly int RaidNormalAmount = 10;
        static readonly bool BuyStuff = true;

        static Data data;
        static int TimeBeforeAttackLands;

        public Functionalities()
        {
            data = Utilities.GetDataJson(System.IO.Path.GetFullPath(@"..\..\Ressources\data.json"));
        }

        [AssemblyInitialize]
        public static void SetUp(TestContext testContext)
        {
            username = Environment.GetEnvironmentVariable("LOGIN_USERNAME");
            password = Environment.GetEnvironmentVariable("LOGIN_PASSWORD");

            chromeDriver = new ChromeDriver(System.IO.Path.GetFullPath(@"..\..\SeleniumDriver\chromedriver_win32"));
            chromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            chromeDriver.Manage().Window.Maximize();
        }

        [TestMethod]
        public void MainLoop()
        {
            List<int> villagesToAttack = new List<int>();
            List<int> oasisToAttack = new List<int>();
            
            for (int i = 0; i < data.Villages.Count; i++)
            {
                villagesToAttack.Add(i);
            }

            for (int i = 0; i < data.Oases.Count; i++)
            {
                oasisToAttack.Add(i);
            }

            int waitForAttack = 0;
            Debug.WriteLine($"Waiting for attack to land");
            Debug.WriteLine($"{DateTime.Now} - Attack will land in {waitForAttack.ToString()}");
            LoggedWait(waitForAttack);           
            
            Login();
            OpenTab(Tabs.Building);
            RefreshRessources();

            while (true)
            {
                SendAttackToVillages(villagesToAttack, RaidNormalAmount, false);
                //SendAttackToOasis(oasisToAttack, RaidNormalAmount, BuyStuff);
            }            
        }

        # region Navigation

        public void Login()
        {
            NavigateTo(Constants.travianUrl);
            FillCredentialsAndEnter();
            Debug.WriteLine("Logged in");
        }

        public void NavigateTo(string url)
        {            
            chromeDriver.Navigate().GoToUrl(url);
            Debug.WriteLine("Navigate to " + url);
        }

        public void FillCredentialsAndEnter()
        {
            chromeDriver.FindElement(By.Name(Localization.login_username)).SendKeys(username);
            chromeDriver.FindElement(By.Name(Localization.login_password)).SendKeys(password);
            chromeDriver.FindElement(By.Name(Localization.login_password)).SendKeys(Keys.Enter);
            Debug.WriteLine("Connected as " + username);
        }

        public void OpenTab(Tabs tab)
        {
            string url;
            url = GetTabUrl(tab);

            NavigateTo(url);
            Debug.WriteLine("Clicked on tab " + tab.ToString());
        }

        public string GetTabUrl(Tabs tab)
        {
            switch (tab)
            {
                case Tabs.Ressources:
                    return Constants.travianUrl + Localization.url_ressources;
                case Tabs.Building:
                    return Constants.travianUrl + Localization.url_building;
                case Tabs.Map:
                    return Constants.travianUrl + Localization.url_map;
                default:
                    return "Error";
            }
        }

        #endregion

        #region CurrentStats

        public void RefreshRessources()
        {
            var wood = "";
            var clay = "";
            var iron = "";
            var wheat = "";

            wood = chromeDriver.FindElement(By.XPath(Localization.id_wood)).Text;
            clay = chromeDriver.FindElement(By.XPath(Localization.id_clay)).Text;
            iron = chromeDriver.FindElement(By.XPath(Localization.id_iron)).Text;
            wheat = chromeDriver.FindElement(By.XPath(Localization.id_wheat)).Text;            

            currentWood = Int32.Parse(wood.Replace(",", ""));
            currentClay = Int32.Parse(clay.Replace(",", ""));
            currentIron = Int32.Parse(iron.Replace(",", ""));
            currentWheat = Int32.Parse(wheat.Replace(",", ""));

            Debug.WriteLine($"wood: {wood}");
            Debug.WriteLine($"clay: {clay}");
            Debug.WriteLine($"iron: {iron}");
            Debug.WriteLine($"wheat: {wheat}");
        }

        private bool CheckForTroopsInOasis(int x, int y)
        {
            var scoutUrl = Constants.travianUrl + Localization.url_gridCell;
            scoutUrl = $"{scoutUrl}x={x}&y={y}";
            NavigateTo(scoutUrl);

            Random random = new Random();
            Wait(random.Next(1500, 2500));

            var troops = chromeDriver.FindElement(By.XPath(Localization.XPath_oasis_troops));

            if (troops.Text == "none")
            {
                Debug.WriteLine("Oasis empty");
                return true;
            }
            else
            {
                Debug.WriteLine("Oasis has some troops");
                return false;
            }
        }

        public bool CheckIfEnoughTroops(int minimumAmount = 5)
        {
            Random random = new Random();
            LoggedWait(random.Next(10000, 60000), "Waiting before checking for troops");

            var legionnaireAmountText = "";

            OpenTab(Tabs.Ressources);

            try
            {
                legionnaireAmountText = chromeDriver.FindElement(By.XPath(Localization.XPath_troops_row_one)).Text;
                var splitted_text = legionnaireAmountText.Split(' ');

                bool isLegionnaire = splitted_text[1] == "Legionnaires";
                
                bool isHigherThanMinimumAmount = int.Parse(splitted_text[0]) >= minimumAmount;
                Debug.WriteLine($"We have this in first row {legionnaireAmountText}");
                if (!isLegionnaire)
                {
                    return false;
                }

                if (isHigherThanMinimumAmount)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;                
            }

            return false;
        }

        #endregion

        #region Attacks

        public void SendAttackToOasis(List<int> oasisToAttack, int troopAmount, bool isBuying, bool mustBeEmpty = true)
        {
            if (mustBeEmpty)
            {
                for (int i = 0; i < oasisToAttack.Count; i++)
                {
                    var grid = data.Oases[i];

                    Random random = new Random();
                    var randomWait = random.Next(2500, 20000);

                    Debug.WriteLine($"Random wait to not get banned");
                    LoggedWait(randomWait);

                    while (!CheckIfEnoughTroops(troopAmount))
                    {
                        Debug.WriteLine($"No enough troops in the village for attack");
                        if (isBuying)
                        {
                            BuyTroops();
                        }
                    }

                    if (CheckForTroopsInOasis(grid.X, grid.Y))
                    {
                        if (SendAttack(grid.X, grid.Y, 5))
                        {
                            data.Oases[i].IsAttacked = true;

                            log.Info("Attack will land in " + TimeBeforeAttackLands.ToString());

                            Debug.WriteLine($"Attacking X: {grid.X} - Y: {grid.Y}.");
                            Debug.WriteLine($"{DateTime.Now} - Attack will land in {TimeBeforeAttackLands.ToString()}");

                            data.Oases[i].IsAttacked = false;
                        }
                    }
                }
            }
        }

        public void SendAttackToVillages(List<int> number, int troopAmount, bool isBuying)
        {
            for (int i = 0; i < number.Count; i++)
            {
                var grid = data.Villages[number[i]];

                Debug.WriteLine($"----- Attacking village # {i.ToString()} -----");
                Debug.WriteLine($"----- X: {grid.X.ToString()} -----");
                Debug.WriteLine($"----- Y: {grid.Y.ToString()} -----");

                Random random = new Random();
                var randomWait = random.Next(2500, 10000);

                Debug.WriteLine($"Random wait to not get banned");
                LoggedWait(randomWait);

                while (!CheckIfEnoughTroops(troopAmount))
                {
                    Debug.WriteLine($"Not enough troops in the village for attack");
                    if (isBuying)
                    {
                        BuyTroops();
                    }
                    else
                    {
                        Debug.Write("Not buying troops");
                    }
                }

                if (SendAttack(grid.X, grid.Y, troopAmount))
                {
                    data.Villages[i].IsAttacked = true;
                    log.Info($"Attack in X: {grid.X} - Y: {grid.Y} will land in " + TimeBeforeAttackLands.ToString());

                    Debug.WriteLine($"Attacking X: {grid.X} - Y: {grid.Y}.");
                    Debug.WriteLine($"{DateTime.Now} - Attack will land in {TimeBeforeAttackLands.ToString()}");
                    //LoggedWait(TimeBeforeAttackLands);

                    data.Villages[i].IsAttacked = false;
                }
            }
        }

        private void BuyTroops(int amount = 1)
        {
            RefreshRessources();

            if (currentWood < 120 * amount || 
                currentWheat < 30 * amount || 
                currentIron < 150 * amount || 
                currentClay < 100 * amount)
            {
                Random random = new Random();
                var waitTime = random.Next(10000, 20000);
                Debug.WriteLine($"Not enough ressources for {amount} unit, waiting for {waitTime/1000} secs.");
                Wait(waitTime);
                return;
            }

            try
            {
                Debug.WriteLine($"Trying to build units");

                Random random = new Random();
                var waitTime = random.Next(1000, 2500);

                var url = Constants.travianUrl + Localization.url_barracks;
                NavigateTo(url);
                Wait(waitTime);

                var buyXLegionnaireInput = chromeDriver.FindElement(By.XPath(Localization.XPath_buyXLegionnaire));
                buyXLegionnaireInput.SendKeys(amount.ToString());
                Wait(waitTime);

                var trainBtn = chromeDriver.FindElement(By.XPath(Localization.XPath_train_troops));
                trainBtn.Click();

                Debug.WriteLine($"Buying {buyXLegionnaireInput.ToString()}");
            }
            catch (Exception)
            {
                Debug.WriteLine($"Not enough ressources for 1 unit :(");
            }
        }

        public bool SendAttack(int x, int y, int troops, bool sendHero = false, bool allTroops = false)
        {
            var rallyPointUrl = Constants.travianUrl + Localization.url_rallyPoint_sendTroops;
            NavigateTo(rallyPointUrl);

            var inputXCoord = chromeDriver.FindElement(By.XPath(Localization.XPath_x_coord));
            var inputYCoord = chromeDriver.FindElement(By.XPath(Localization.XPath_y_coord));
            var rboRaid = chromeDriver.FindElement(By.XPath(Localization.XPath_raid));
            var buttonSend = chromeDriver.FindElement(By.XPath(Localization.XPath_OkBtn));

            //TODO: add troop class to choose soldiers
            var inputLegionnaire = chromeDriver.FindElement(By.XPath(Localization.XPath_legionnaire));

            if (sendHero)
            {
                var inputHero = chromeDriver.FindElement(By.XPath(Localization.XPath_hero));
                inputHero.SendKeys(1.ToString());
                Debug.WriteLine("Hero was sent");
            }

            if (allTroops)
            {
                var allLegionnaires = chromeDriver.FindElement(By.XPath(Localization.XPath_all_legionnaires));
                allLegionnaires.Click();
                Debug.WriteLine("All legionnaires were send");
            }
            else
            {
                inputLegionnaire.SendKeys(troops.ToString());
                Debug.WriteLine($"Sent {troops.ToString()} legionnaires");
            }

            inputXCoord.SendKeys(x.ToString());
            inputYCoord.SendKeys(y.ToString());
            rboRaid.Click();

            buttonSend.Click();

            Wait(1000);

            try
            {
                var errorMessage = chromeDriver.FindElement(By.XPath(Localization.XPath_error_sending_troops));

                if (errorMessage.Text == "No troops have been selected.")
                {
                    return false;
                }
            }
            catch (Exception)
            {
                var buttonConfirm = chromeDriver.FindElement(By.XPath(Localization.XPath_ConfirmBtn));
                var arrivalTime = chromeDriver.FindElement(By.XPath(Localization.XPath_arrival_time));

                var time = arrivalTime.Text;
                TimeBeforeAttackLands = TimeToMs(time) * 2;

                buttonConfirm.Click();

                return true;
            }

            return false;
        }

        #endregion

        #region Utilities

        public void Wait(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }

        public void LoggedWait(int ms, string reason = "Attack will land in")
        {
            int numberOfLogs = 10;
            int partialWait = (int) Math.Ceiling((double) (ms / numberOfLogs));

            for (int i = 0; i < numberOfLogs; i++)
            {
                System.Threading.Thread.Sleep(partialWait);
                ms = ms - partialWait;
                Debug.WriteLine($"{DateTime.Now} - {reason} {ms.ToString()}");
            }
        }

        public int TimeToMs(string time)
        {            
            string[] times = time.Replace("in ", "").Replace(" hrs.", "").Split(':');

            return (Int32.Parse(times[0]) * 60 * 60  + Int32.Parse(times[1]) * 60 + Int32.Parse(times[2])) * 1000;
        }

        #endregion


    }
}
