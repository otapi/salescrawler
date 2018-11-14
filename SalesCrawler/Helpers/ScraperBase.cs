using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesCrawler.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Drawing;

namespace SalesCrawler.Helpers
{
    public class ScraperBase
    {
        public static DateTime NEVEREXPIRE = new DateTime(2100, 1, 1);
        protected IWebDriver driver;
        protected ScraperSetting Setting;
        private Match Match;

        public virtual void ScrapeList()
        {

        }

        public void ScrapeListBase(IWebDriver driver, ScraperSetting scraperSettings)
        {
            this.driver = driver;
            Setting = scraperSettings;
            Match = null;
            ScrapeList();

        }

        public virtual void UpdateMatchDetails(MatchData matchData)
        {

        }

        public void UpdateMatchDetailsBase(IWebDriver driver, Match match)
        {
            this.driver = driver;
            Setting = match.ScraperSetting;
            Match = match;
            UpdateMatchDetails(match.MatchData);
            App.DB.SaveChangesAsync().Wait();

        }

        protected double StripToDouble(string text)
        {
            // TODO: check and locale spec
            var s = System.Text.RegularExpressions.Regex.Replace(text, @"[^\d.,]", "");
            if (s == "" || s == "." || s == ",")
            {
                return 0;
            }
            else
            {
                return double.Parse(s);
            }
        }

        protected int StripToInt(string text)
        {
            var s = System.Text.RegularExpressions.Regex.Replace(text, @"[^\d]", "");
            if (s == "")
            {
                return 0;
            }
            else
            {
                return int.Parse(s);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout">seconds</param>
        /// <returns></returns>
        protected WebDriverWait Wait(int timeout = 10) {
            return new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
        }

        protected void AddMatch(MatchData matchData)
        {
            var m = new Match();
            m.LastScannedDate = DateTime.Now;
            m.MatchData = matchData;
            m.PriceHistories.Add(new PriceHistory()
            {
                Date = m.LastScannedDate,
                PriceHUF = matchData.ActualPriceHUF
            });
            m.ScraperSetting = Setting;
            App.DB.Matches.Add(m);
            App.DB.SaveChangesAsync().Wait();
        }


        protected void PrintNote(string message)
        {
            App.PrintNote($"[{Setting.Name}] ${message}");
        }

        protected void PrintWarning(string message)
        {
            App.PrintWarning($"[{Setting.Name}] ${message}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Y coordinate</returns>
        int ScrollTo(IWebElement element)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript($"window.scroll(0, {element.Location.Y});");
            return element.Location.Y;
        }

        Bitmap TakeScreenshot(IWebDriver driver, IWebElement element)
        {

            driver.SwitchTo();
            driver.Manage().Window.Maximize();


            int yoffset = ScrollTo(element);

            // Todo: cache the screenshot with Y coordinates.
            Byte[] byteArray = ((ITakesScreenshot)driver).GetScreenshot().AsByteArray;
            System.Drawing.Bitmap screenshot = new System.Drawing.Bitmap(new System.IO.MemoryStream(byteArray));

            System.Drawing.Rectangle croppedImage = new System.Drawing.Rectangle(element.Location.X, 0, element.Size.Width, element.Size.Height);
            screenshot = screenshot.Clone(croppedImage, screenshot.PixelFormat);
            return screenshot;
        }
    }
}

