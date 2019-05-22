using System;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using TravianBot.Ressources;
using TravianBot.Functionnalities.Data;
using TravianBot.Functionnalities.Enums;
using TravianBot.Functionnalities.Utilities;

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

        static int NumberOfTries = 0;
        static readonly int MaxNumberOfTries = 3;

        static string username;
        static string password;

        static bool CanBuyTroops = false;

        static AttackTargets targets;
        static int TimeBeforeAttackLands;

        static TroopsInfo TroopsToSave = new TroopsInfo();

        public Functionalities()
        {
            targets = Utilities.GetDataJson(System.IO.Path.GetFullPath(@"..\..\Ressources\data.json"));
            TroopsToSave.Praetorian.Amount = 0;
            TroopsToSave.Settlers.Amount = 3;
            TroopsToSave.Imperian.Amount = 100;
            TroopsToSave.EquitesImperatoris.Amount = 100;
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

            CanBuyTroops = false;

            TroopsInfo troopsToBuyPraven = new TroopsInfo();
            troopsToBuyPraven.EquitesImperatoris.Amount = 5;

            TroopsInfo troopsToBuySuno = new TroopsInfo();
            troopsToBuySuno.Imperian.Amount = 10;

            TroopsInfo troopsToSend = new TroopsInfo();
            troopsToSend.Legionnaire.Amount = 0;
            troopsToSend.EquitesImperatoris.Amount = 2;
            troopsToSend.Imperian.Amount = 5;
            

            for (int i = 0; i < targets.Villages.Count; i++)
            {
                villagesToAttack.Add(i);
            }

            for (int i = 0; i < targets.Oases.Count; i++)
            {
                oasisToAttack.Add(i);
            }

            int waitForAttack = 0;
            Debug.WriteLine($"Waiting for attack to land");
            Debug.WriteLine($"{DateTime.Now} - Attack will land in {waitForAttack.ToString()}");
            LoggedWait(waitForAttack, "Waiting for last attack to land");           
            
            Login();
            OpenTab(Tabs.Ressources);
            RefreshRessources();

            while (true)
            {                
                SendAttackToVillages(villagesToAttack, troopsToSend, troopsToBuyPraven, troopsToBuySuno);
            }            
        }

        public void SendAttackToVillages(List<int> number, TroopsInfo attackInfo, TroopsInfo buyInfoPraven, TroopsInfo buyInfoSuno)
        {
            for (int i = 0; i < number.Count; i++)
            {
                AvoidAttack(TroopsToSave);

                try
                {
                    var grid = targets.Villages[number[i]];

                    if (!grid.CanRaid)
                    {
                        Debug.WriteLine($"----- Village # {i.ToString()} cannot be attacked -----");
                        Debug.WriteLine($"----- X: {grid.X.ToString()} -----");
                        Debug.WriteLine($"----- Y: {grid.Y.ToString()} -----");
                    }
                    else
                    {
                        Debug.WriteLine($"----- Attacking village # {i.ToString()} -----");
                        Debug.WriteLine($"----- X: {grid.X.ToString()} -----");
                        Debug.WriteLine($"----- Y: {grid.Y.ToString()} -----");

                        Random random = new Random();
                        var randomWait = random.Next(2500, 10000);
                        LoggedWait(randomWait, "Waiting before attack to not get banned for");

                        //Start with first village
                        Cities city = Cities.Praven;
                        OpenTab(Tabs.Ressources, city);

                        while (CheckIfEnoughTroops(attackInfo, city) == Messages.None)
                        {
                            Debug.WriteLine($"Not enough troops in the village for attack");

                            if (CanBuyTroops)
                            {
                                Debug.WriteLine($"Buying Troops");
                                if (city == Cities.Praven)
                                {
                                    BuyTroops(buyInfoPraven, city);
                                }
                                else if (city == Cities.Suno)
                                {
                                    BuyTroops(buyInfoSuno, city);
                                }
                            }

                            AvoidAttack(TroopsToSave);

                            if (city == Cities.Praven)
                            {
                                city = Cities.Suno;
                            }
                            else if (city == Cities.Suno)
                            {
                                city = Cities.Praven;
                            }
                            Debug.WriteLine($"Changing village to {city.ToString()}" );
                        }

                        var result = CheckIfEnoughTroops(attackInfo, city);

                        if (result == Messages.EquitesImperatoris)
                        {
                            SendAttack(grid.X, grid.Y, attackInfo.EquitesImperatoris);

                            targets.Villages[i].IsAttacked = true;

                            Debug.WriteLine($"{DateTime.Now} - Attacking X: {grid.X} - Y: {grid.Y}.");
                            Debug.WriteLine($"{DateTime.Now} - Attack will land in {TimeBeforeAttackLands.ToString()}");
                            Debug.WriteLine($"{DateTime.Now} - Attacking with {attackInfo.EquitesImperatoris.Amount} {attackInfo.EquitesImperatoris.Name}");


                            targets.Villages[i].IsAttacked = false;
                        }
                        else if (result == Messages.Legionnaire)
                        {
                            SendAttack(grid.X, grid.Y, attackInfo.Legionnaire);

                            targets.Villages[i].IsAttacked = true;

                            Debug.WriteLine($"{DateTime.Now} - Attacking X: {grid.X} - Y: {grid.Y}.");
                            Debug.WriteLine($"{DateTime.Now} - Attack will land in {TimeBeforeAttackLands.ToString()}");
                            Debug.WriteLine($"{DateTime.Now} - Attacking with {attackInfo.Legionnaire.Amount} {attackInfo.Legionnaire.Name}");
                            
                            targets.Villages[i].IsAttacked = false;
                        }
                        else if (result == Messages.Imperian)
                        {
                            SendAttack(grid.X, grid.Y, attackInfo.Imperian);

                            targets.Villages[i].IsAttacked = true;

                            Debug.WriteLine($"{DateTime.Now} - Attacking X: {grid.X} - Y: {grid.Y}.");
                            Debug.WriteLine($"{DateTime.Now} - Attack will land in {TimeBeforeAttackLands.ToString()}");
                            Debug.WriteLine($"{DateTime.Now} - Attacking with {attackInfo.Imperian.Amount} {attackInfo.Imperian.Name}");

                            targets.Villages[i].IsAttacked = false;
                        }
                        else if (result == Messages.All)
                        {
                            if (attackInfo.Legionnaire.Amount != 0)
                            {
                                SendAttack(grid.X, grid.Y, attackInfo.Legionnaire);
                            }
                            else if (attackInfo.EquitesImperatoris.Amount != 0)
                            {
                                SendAttack(grid.X, grid.Y, attackInfo.EquitesImperatoris);
                            }
                            else if (attackInfo.Imperian.Amount != 0)
                            {
                                SendAttack(grid.X, grid.Y, attackInfo.Imperian);
                            }
                            else
                            {
                                Debug.WriteLine($"No troop was sent cause amounts were all at 0");
                                i--;
                            }

                            targets.Villages[i].IsAttacked = true;

                            Debug.WriteLine($"{DateTime.Now} - Attacking X: {grid.X} - Y: {grid.Y}.");
                            Debug.WriteLine($"{DateTime.Now} - Attack will land in {TimeBeforeAttackLands.ToString()}");
                            Debug.WriteLine($"{DateTime.Now} - Attacking with ---");

                            targets.Villages[i].IsAttacked = false;
                        }
                        else if (result == Messages.None)
                        {
                            Debug.WriteLine("No troop was sent");
                        }
                        else
                        {
                            Debug.WriteLine("Bug, it should not get here");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Exception, {e.Message}");

                    if (NumberOfTries <= MaxNumberOfTries)
                    {
                        i--;
                        NumberOfTries++;
                    }
                    else
                    {
                        NumberOfTries = 0;
                        SelectMainVillage();
                        OpenTab(Tabs.Building);
                    }
                }                
            }
        }

        public void SelectMainVillage()
        {
            chromeDriver.FindElement(By.XPath(Localization.XPath_MainVillage)).Click();
        }

        public void EnsureWeAreInMainVillage()
        {
            var villages = chromeDriver.FindElement(By.XPath(Localization.XPath_MainVillage));
            
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

        public void OpenTab(Tabs tab, Cities city = Cities.Praven)
        {
            string url;
            url = GetTabUrl(tab, city);

            Random random = new Random();
            if (random.Next(1, 10) == 2)
            {
                NavigateTo(url);
                return;
            }

            if (url != chromeDriver.Url)
            {
                NavigateTo(url);
            }

            Debug.WriteLine("Clicked on tab " + tab.ToString());
        }

        public string GetTabUrl(Tabs tab, Cities cities = Cities.Praven)
        {
            switch (tab)
            {
                case Tabs.Ressources:
                    if (cities == Cities.Suno)
                    {
                        return Constants.travianUrl + Localization.url_ressources_Suno;
                    }
                    return Constants.travianUrl + Localization.url_ressources_Praven;
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
        
        public Messages CheckIfEnoughTroops(TroopsInfo attackTroops, Cities city, bool requiresHero = false)
        {
            Random random = new Random();
            LoggedWait(random.Next(2000, 4000), "Waiting before checking for troops");

            OpenTab(Tabs.Ressources, city);
            var TroopRows = chromeDriver.FindElements(By.XPath(Localization.XPath_troops_rows));

            LoggedWait(random.Next(500, 2000), "Waiting for page to avoid stale element exception");

            bool enoughLegionnaire = false;
            bool enoughEquitesImperatoris = false;
            bool enoughImperian = false;
            bool enoughHero = false;

            foreach (var row in TroopRows)
            {
                var textSplitted = row.Text.Split(' ');
                var amount = int.Parse(textSplitted[0]);

                if (textSplitted[1] == "Equites")
                    textSplitted[1] = $"{textSplitted[1]} {textSplitted[2]}";

                // If legionnaire
                if (textSplitted[1].Contains(attackTroops.Legionnaire.Name))
                {
                    
                    if (amount >= attackTroops.Legionnaire.Amount)
                    {
                        enoughLegionnaire = true;
                    }
                }

                //If Hero
                if (textSplitted[1].Contains(attackTroops.Hero.Name))
                {
                    if (amount >= 1)
                    {
                        enoughHero = true;
                    }
                }

                //If Equites Imperatoris
                if (textSplitted[1].Contains(attackTroops.EquitesImperatoris.Name))
                {
                    if (amount >= attackTroops.EquitesImperatoris.Amount)
                    {
                        enoughEquitesImperatoris = true;
                    }
                }

                //If Imperian
                if (textSplitted[1].Contains(attackTroops.Imperian.Name))
                {
                    if (amount >= attackTroops.Imperian.Amount)
                    {
                        enoughImperian = true;
                    }
                }

                Debug.WriteLine($"{amount} {textSplitted[1]}");
            }

            if (attackTroops.Legionnaire.Amount == 0)
            {
                enoughLegionnaire = false;
            }

            if (attackTroops.EquitesImperatoris.Amount == 0)
            {
                enoughEquitesImperatoris = false;
            }

            if (attackTroops.Imperian.Amount == 0)
            {
                enoughImperian = false;
            }

            if (!requiresHero)
            {
                if (enoughEquitesImperatoris && enoughLegionnaire)
                {
                    return Messages.All;
                }
                else if (enoughEquitesImperatoris)
                {
                    return Messages.EquitesImperatoris;
                }
                else if (enoughLegionnaire)
                {
                    return Messages.Legionnaire;
                }
                else if (enoughImperian)
                {
                    return Messages.Imperian;
                }
                else
                {
                    return Messages.None;
                }
            }
            else
            {
                if (enoughEquitesImperatoris && enoughLegionnaire && enoughHero)
                {
                    return Messages.All;
                }
                else if (enoughEquitesImperatoris && enoughHero)
                {
                    return Messages.EquitesImperatorisWithHero;
                }
                else if (enoughLegionnaire && enoughHero)
                {
                    return Messages.LegionnaireWithHero;
                }
                else if (enoughImperian && enoughHero)
                {
                    return Messages.ImperianWithHero;
                }
                else
                {
                    return Messages.None;
                }
            }            
        }

        #endregion

        #region Attacks

        /*public void SendAttackToOasis(List<int> oasisToAttack, int troopAmount, bool isBuying, bool mustBeEmpty = true)
        {
            if (mustBeEmpty)
            {
                for (int i = 0; i < oasisToAttack.Count; i++)
                {
                    var grid = targets.Oases[i];

                    Random random = new Random();
                    var randomWait = random.Next(2500, 20000);

                    Debug.WriteLine($"Random wait to not get banned");
                    LoggedWait(randomWait);

                    while (!CheckIfEnoughTroops(troopAmount))
                    {
                        Debug.WriteLine($"No enough troops in the village for attack");
                        if (isBuying)
                        {
                            BuyTroops(BuyingLevel);
                        }
                    }

                    if (CheckForTroopsInOasis(grid.X, grid.Y))
                    {
                        if (SendAttack(grid.X, grid.Y, 5))
                        {
                            targets.Oases[i].IsAttacked = true;

                            log.Info("Attack will land in " + TimeBeforeAttackLands.ToString());

                            Debug.WriteLine($"Attacking X: {grid.X} - Y: {grid.Y}.");
                            Debug.WriteLine($"{DateTime.Now} - Attack will land in {TimeBeforeAttackLands.ToString()}");

                            targets.Oases[i].IsAttacked = false;
                        }
                    }
                }
            }
        }*/

        private void BuyTroops(TroopsInfo buyInfo, Cities city)
        {
            OpenTab(Tabs.Ressources, city);
            RefreshRessources();

            bool CanBuyEquitesImperatoris = false;
            bool CanBuyLegionnaire = false;
            bool CanBuyImperian = false;

            if (EnoughRessources(buyInfo.Legionnaire) && buyInfo.Legionnaire.Amount > 0)
            {
                CanBuyLegionnaire = true;
                Debug.WriteLine($"Enough ressources for {buyInfo.Legionnaire.Amount} {buyInfo.Legionnaire.Name}");
            }

            if (EnoughRessources(buyInfo.EquitesImperatoris) && buyInfo.EquitesImperatoris.Amount > 0)
            {
                CanBuyEquitesImperatoris = true;
                Debug.WriteLine($"Enough ressources for {buyInfo.EquitesImperatoris.Amount} {buyInfo.EquitesImperatoris.Name}");
            }

            if (EnoughRessources(buyInfo.Imperian) && buyInfo.Imperian.Amount > 0)
            {
                CanBuyImperian = true;
                Debug.WriteLine($"Enough ressources for {buyInfo.Imperian.Amount} {buyInfo.Imperian.Name}");
            }

            if (CanBuyEquitesImperatoris)
            {
                Debug.WriteLine($"Buying {buyInfo.EquitesImperatoris.Amount} {buyInfo.EquitesImperatoris.Name}");

                Random random = new Random();
                var waitTime = random.Next(1000, 2500);

                var url = Constants.travianUrl + Localization.url_stables;
                NavigateTo(url);
                Wait(waitTime);

                var buyXInput = chromeDriver.FindElement(By.XPath(Localization.XPath_buyXEquitesImperatoris));
                buyXInput.SendKeys(buyInfo.EquitesImperatoris.Amount.ToString());

                var trainBtn = chromeDriver.FindElement(By.XPath(Localization.XPath_train_troops));
                trainBtn.Click();

                Debug.WriteLine($"Bought {buyInfo.EquitesImperatoris.Amount} {buyInfo.EquitesImperatoris.Name}");
                Wait(waitTime);

                return;
            }
            else if (CanBuyLegionnaire)
            {
                Debug.WriteLine($"Buying {buyInfo.Legionnaire.Amount} {buyInfo.Legionnaire.Name}");

                Random random = new Random();
                var waitTime = random.Next(1000, 2500);

                var url = Constants.travianUrl + Localization.url_barracks;
                NavigateTo(url);
                Wait(waitTime);

                var buyXInput = chromeDriver.FindElement(By.XPath(Localization.XPath_buyXLegionnaire));
                buyXInput.SendKeys(buyInfo.Legionnaire.Amount.ToString());

                var trainBtn = chromeDriver.FindElement(By.XPath(Localization.XPath_train_troops));
                trainBtn.Click();

                Debug.WriteLine($"Bought {buyInfo.Legionnaire.Amount} {buyInfo.Legionnaire.Name}");
                Wait(waitTime);

                return;
            }
            else if (CanBuyImperian)
            {
                Debug.WriteLine($"Buying {buyInfo.Imperian.Amount} {buyInfo.Imperian.Name}");

                Random random = new Random();
                var waitTime = random.Next(1000, 2500);

                var url = Constants.travianUrl + Localization.url_barracks;
                NavigateTo(url);
                Wait(waitTime);

                var buyXInput = chromeDriver.FindElement(By.XPath(Localization.XPath_buyXImperian));
                buyXInput.SendKeys(buyInfo.Imperian.Amount.ToString());

                var trainBtn = chromeDriver.FindElement(By.XPath(Localization.XPath_train_troops));
                trainBtn.Click();

                Debug.WriteLine($"Bought {buyInfo.Imperian.Amount} {buyInfo.Imperian.Name}");
                Wait(waitTime);

                return;
            }
            else
            {
                Random random = new Random();
                var waitTime = random.Next(2500, 15000);
                Debug.WriteLine($"waiting for {waitTime / 1000} secs.");
                Wait(waitTime);
                return;
            }      
        }

        public void SendAttack(int x, int y, Unit troops, bool sendHero = false)
        {
            var rallyPointUrl = Constants.travianUrl + Localization.url_rallyPoint_sendTroops;
            NavigateTo(rallyPointUrl);

            Random random = new Random();
            LoggedWait(random.Next(1000, 2500), "Waiting For Barracks for no ban");

            var inputXCoord = chromeDriver.FindElement(By.XPath(Localization.XPath_x_coord));
            var inputYCoord = chromeDriver.FindElement(By.XPath(Localization.XPath_y_coord));
            var rboRaid = chromeDriver.FindElement(By.XPath(Localization.XPath_raid));
            var buttonSend = chromeDriver.FindElement(By.XPath(Localization.XPath_OkBtn));

            var inputLegionnaire = chromeDriver.FindElement(By.XPath(Localization.XPath_legionnaire));
            var inputEquitesImperatoris = chromeDriver.FindElement(By.XPath(Localization.XPath_EquitesImperatoris));
            var inputImperian = chromeDriver.FindElement(By.XPath(Localization.XPath_Imperian));

            if (sendHero)
            {
                var inputHero = chromeDriver.FindElement(By.XPath(Localization.XPath_hero));
                inputHero.SendKeys(1.ToString());
                Debug.WriteLine("Hero was sent");
            }

            TroopsInfo troopsInfo = new TroopsInfo();

            if (troops.Name == troopsInfo.Legionnaire.Name)
            {
                inputLegionnaire.SendKeys(troops.Amount.ToString());
            }
            else if (troops.Name == troopsInfo.EquitesImperatoris.Name)
            {
                inputEquitesImperatoris.SendKeys(troops.Amount.ToString());
            }
            else if (troops.Name == troopsInfo.Imperian.Name)
            {
                inputImperian.SendKeys(troops.Amount.ToString());
            }

            Debug.WriteLine($"{troops.Amount} {troops.Name} were sent.");

            LoggedWait(random.Next(500, 1200), "Activating Raid");

            inputXCoord.SendKeys(x.ToString());
            inputYCoord.SendKeys(y.ToString());
            rboRaid.Click();

            LoggedWait(random.Next(500, 1200), "Clicking attack");

            buttonSend.Click();

            Wait(1000);

            try
            {
                var noVillageMsg = chromeDriver.FindElement(By.XPath(Localization.XPath_noVillage_msg)).Text;

                if (noVillageMsg.Contains(Constants.noVillageMsg))
                {
                    Debug.WriteLine("There is no village at these coordinates");
                    return;
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Confirming attack");
            }

            var buttonConfirm = chromeDriver.FindElement(By.XPath(Localization.XPath_ConfirmBtn));
            var arrivalTime = chromeDriver.FindElement(By.XPath(Localization.XPath_arrival_time));

            var time = arrivalTime.Text;
            TimeBeforeAttackLands = Utilities.TimeToMs(time) * 2;

            buttonConfirm.Click();
        }

        public void AvoidAttack(TroopsInfo troops)
        {
            OpenTab(Tabs.Ressources);      

            Debug.WriteLine($"Check if we are attacked");
            if (CheckIfAttacked())
            {
                Debug.WriteLine($"We are being attacked !");
                var timeBeforeAttack = chromeDriver.FindElement(By.XPath(Localization.Xpath_IncomingTroops_time));
                var timeInMs = Utilities.TimeToMs(timeBeforeAttack.Text);

                Debug.WriteLine($"Attack will land in {timeInMs.ToString()}.");

                if (timeInMs < 60000)
                {
                    SendReinforcement(troops);
                }
                else
                {
                    Debug.WriteLine($"No action required for the moment");
                }
            }
        }
        
        public bool CheckIfAttacked()
        {
            try
            {
                var typeOfTroops = chromeDriver.FindElement(By.XPath(Localization.XPath_IncomingTroops_section));
                var incommingTroopsAttack = chromeDriver.FindElement(By.XPath(Localization.XPath_IncomingTroops_span));
                
                if (typeOfTroops.Text.Contains("Incoming"))
                {
                    if (incommingTroopsAttack.Text.Contains("Attack"))
                    {
                        return true;
                    }
                }                

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SendReinforcement(TroopsInfo troops)
        {
            var rallyPointUrl = Constants.travianUrl + Localization.url_rallyPoint_sendTroops;
            NavigateTo(rallyPointUrl);

            Random random = new Random();
            LoggedWait(random.Next(1000, 2500), "Waiting For Barracks for no ban");

            var inputXCoord = chromeDriver.FindElement(By.XPath(Localization.XPath_x_coord));
            var inputYCoord = chromeDriver.FindElement(By.XPath(Localization.XPath_y_coord));
            var buttonSend = chromeDriver.FindElement(By.XPath(Localization.XPath_OkBtn));

            var inputLegionnaire = chromeDriver.FindElement(By.XPath(Localization.XPath_legionnaire));
            var inputEquitesImperatoris = chromeDriver.FindElement(By.XPath(Localization.XPath_EquitesImperatoris));
            var inputPraetorian = chromeDriver.FindElement(By.XPath(Localization.XPath_Praetorian));
            var inputSettler = chromeDriver.FindElement(By.XPath(Localization.XPath_Settlers));

            TroopsInfo troopsInfo = new TroopsInfo();

            inputXCoord.SendKeys("-43");
            inputYCoord.SendKeys("-42");

            inputLegionnaire.SendKeys(troops.Legionnaire.Amount.ToString());
            inputEquitesImperatoris.SendKeys(troops.EquitesImperatoris.Amount.ToString());
            inputPraetorian.SendKeys(troops.Praetorian.Amount.ToString());
            inputSettler.SendKeys(troops.Settlers.Amount.ToString());

            Debug.WriteLine($"Following troops were sent:");
            Debug.WriteLine($"{troops.Legionnaire.Amount.ToString()} {troops.Legionnaire.Name} were sent.");
            Debug.WriteLine($"{troops.EquitesImperatoris.Amount.ToString()} {troops.EquitesImperatoris.Name} were sent.");
            Debug.WriteLine($"{troops.Praetorian.Amount.ToString()} {troops.Praetorian.Name} were sent.");
            Debug.WriteLine($"{troops.Settlers.Amount.ToString()} {troops.Settlers.Name} were sent.");

            LoggedWait(random.Next(500, 1200), "Reinforcing -43|-42");

            buttonSend.Click();
            Wait(1000);

            var buttonConfirm = chromeDriver.FindElement(By.XPath(Localization.XPath_ConfirmBtn));
            var arrivalTime = chromeDriver.FindElement(By.XPath(Localization.XPath_arrival_time));

            var time = arrivalTime.Text;
            TimeBeforeAttackLands = Utilities.TimeToMs(time) * 2;

            buttonConfirm.Click();
        }

        #endregion

        #region Utilities

        public bool EnoughRessources(Unit unit)
        {
            return (currentWood > unit.Amount * unit.Cost.Wood &&
                    currentWheat > unit.Amount * unit.Cost.Wheat &&
                    currentIron > unit.Amount * unit.Cost.Iron &&
                    currentClay > unit.Amount * unit.Cost.Clay);
        }

        public void Wait(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }

        public void LoggedWait(int ms, string reason = "Attack will land in")
        {
            int numberOfLogs = 3;
            int partialWait = (int) Math.Ceiling((double) (ms / numberOfLogs));

            for (int i = 0; i < numberOfLogs; i++)
            {
                System.Threading.Thread.Sleep(partialWait);
                ms = ms - partialWait;
                Debug.WriteLine($"{DateTime.Now} - {reason} {ms.ToString()}");
            }
        }     

        #endregion
    }
}
