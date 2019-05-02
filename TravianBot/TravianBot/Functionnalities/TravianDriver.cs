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
            data = Utilities.GetDataJson(Constants.dataJson);
        }

        [AssemblyInitialize]
        public static void SetUp(TestContext testContext)
        {
            username = Environment.GetEnvironmentVariable("LOGIN_USERNAME");
            password = Environment.GetEnvironmentVariable("LOGIN_PASSWORD");

            chromeDriver = new ChromeDriver(@"E:\Projects\SeleniumBots\TravianBot\TravianBot\SeleniumDriver\chromedriver_win32");
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

            int waitForAttack = 11854000;
            Debug.WriteLine($"Waiting for attack to land");
            Debug.WriteLine($"{DateTime.Now} - Attack will land in {waitForAttack.ToString()}");
            LoggedWait(waitForAttack);           
            
            Login();
            OpenTab(Tabs.Building);
            RefreshRessources();

            while (true)
            {
                SendAttackToVillages(villagesToAttack, RaidNormalAmount, BuyStuff);
                SendAttackToOasis(oasisToAttack, RaidNormalAmount, BuyStuff);
            }            
        }

        # region Navigation

        public void Login()
        {
            NavigateTo(Constants.travianUrl);
            FillCredentialsAndEnter();
            log.Info("Logged in");
        }

        public void NavigateTo(string url)
        {            
            chromeDriver.Navigate().GoToUrl(url);
            log.Info("Navigate to " + url);
        }

        public void FillCredentialsAndEnter()
        {
            chromeDriver.FindElement(By.Name(Localization.login_username)).SendKeys(username);
            chromeDriver.FindElement(By.Name(Localization.login_password)).SendKeys(password);
            chromeDriver.FindElement(By.Name(Localization.login_password)).SendKeys(Keys.Enter);
            log.Info("Connected as " + username);
        }

        public void OpenTab(Tabs tab)
        {
            string url;
            url = GetTabUrl(tab);

            NavigateTo(url);
            log.Info("Clicked on tab " + tab.ToString());
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

            log.Info($"wood: {wood}");
            log.Info($"clay: {clay}");
            log.Info($"iron: {iron}");
            log.Info($"wheat: {wheat}");
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
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckIfEnoughTroops(int minimumAmount)
        {
            var legionnaireAmountText = "";
            int legionnaireAmount = 0;

            try
            {
                legionnaireAmountText = chromeDriver.FindElement(By.XPath(Localization.XPath_legionnaireAmount)).Text;
            }
            catch (Exception)
            {
                return false;                
            }

            if (legionnaireAmount < minimumAmount)
            {
                return false;
            }
            else
            {
                return true;
            }



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

                while (!CheckIfEnoughTroops(troopAmount))
                {
                    Debug.WriteLine($"No enough troops in the village for attack");
                    if (isBuying)
                    {
                        BuyTroops();
                    }
                }

                if (SendAttack(grid.X, grid.Y, troopAmount))
                {
                    data.Villages[i].IsAttacked = true;
                    log.Info($"Attack in X: {grid.X} - Y: {grid.Y} will land in " + TimeBeforeAttackLands.ToString());

                    Debug.WriteLine($"Attacking X: {grid.X} - Y: {grid.Y}.");
                    Debug.WriteLine($"{DateTime.Now} - Attack will land in {TimeBeforeAttackLands.ToString()}");

                    LoggedWait(TimeBeforeAttackLands);
                    data.Villages[i].IsAttacked = false;
                }
            }
        }

        private void BuyTroops()
        {
            RefreshRessources();

            if (currentWood < 120 || currentWheat < 30 || currentIron < 150 || currentClay < 100)
            {
                Debug.WriteLine($"Not enough ressources for 1 unit :(");

                Random random = new Random();
                var waitTime = random.Next(10000, 20000);
                Debug.WriteLine($"Not enough ressources for 1 unit, waiting for {waitTime/1000} secs.");
                Wait(waitTime);
                return;
            }

            try
            {
                Debug.WriteLine($"Trying to build units");
                var buyAllLegionnaireBtn = chromeDriver.FindElement(By.XPath(Localization.XPath_buyAllLegionnaire));
                buyAllLegionnaireBtn.Click();

                Wait(1000);
                var trainBtn = chromeDriver.FindElement(By.XPath(Localization.XPath_train_troops));
                trainBtn.Click();
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
            }

            if (allTroops)
            {
                var allLegionnaires = chromeDriver.FindElement(By.XPath(Localization.XPath_all_legionnaires));
                allLegionnaires.Click();
            }
            else
            {
                inputLegionnaire.SendKeys(troops.ToString());
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

        public void LoggedWait(int ms)
        {
            int numberOfLogs = 100;
            int partialWait = (int) Math.Ceiling((double) (ms / numberOfLogs));

            for (int i = 0; i < numberOfLogs; i++)
            {
                System.Threading.Thread.Sleep(partialWait);
                ms = ms - partialWait;
                Debug.WriteLine($"{DateTime.Now} - Attack will land in {ms.ToString()}");
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
