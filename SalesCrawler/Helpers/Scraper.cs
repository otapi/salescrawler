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
using System.Net;
using System.IO;
using SalesCrawler.ViewModels;

namespace SalesCrawler.Helpers
{
    public class Scraper : BaseVM
    {
        private static bool SAVE_DB_ONLYONCE = true;
        public static DateTime NEVEREXPIRE = new DateTime(2100, 1, 1);
        protected IWebDriver driver;
        protected ScraperSetting Setting;
        private Match Match;
        private List<Match> AddedMatches = new List<Match>();
        string _Message;
        public string Message
        {
            get { return _Message; }
            set { SetProperty(ref _Message, value); }
        }

        public virtual void ScrapeList()
        {

        }

        public void ScrapeListBase(IWebDriver driver, ScraperSetting scraperSettings)
        {
            this.driver = driver;
            Setting = scraperSettings;
            Match = null;
            AddedMatches = new List<Match>();
            Message = "Start Scrape List";
            ScrapeList();

            // If details need to be updated at new Matches...
            for (int i=0; i<AddedMatches.Count;i++)
            {
                Message = $"Scrape Details: {i}/{AddedMatches.Count}";
                UpdateMatchDetailsBase(driver, AddedMatches[i]);
            }
            Message = $"ScrapeList finished, found {AddedMatches.Count} items.";

            if (SAVE_DB_ONLYONCE)
            {
                App.DB.SaveChangesAsync().Wait();
            }
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
            if (!SAVE_DB_ONLYONCE)
            {
                App.DB.SaveChangesAsync().Wait();
            }

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

        protected string StripToLetters(string text)
        {
            var s = System.Text.RegularExpressions.Regex.Replace(text, @"[\d-]", string.Empty);
            s = s.Replace("&nbsp;", " ");
            return s.Trim();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout">seconds</param>
        /// <returns></returns>
        protected WebDriverWait Waitfor(int timeout = 10) {
            return new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
        }

        protected void Wait(int timeout = 1)
        {
            System.Threading.Thread.Sleep(timeout * 1000);
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
            if (!SAVE_DB_ONLYONCE)
            {
                App.DB.SaveChangesAsync().Wait();
            }
            AddedMatches.Add(m);
            
        }


        protected void PrintNote(string message)
        {
            App.PrintNote($"[{Setting.Name}] ${message}");
            Message = message;
        }

        protected void PrintWarning(string message)
        {
            App.PrintWarning($"[{Setting.Name}] ${message}");
            Message = message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Y coordinate</returns>
        protected int ScrollTo(IWebElement element, int offsetY = 0)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript($"window.scroll(0, {element.Location.Y+offsetY});");
            return element.Location.Y;
        }

        protected void ScrollToBottom()
        {
            ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTop = window.scrollHeight;");
        }

        protected byte[] GetImage(string url)
        {
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
            WebResponse myResp = myReq.GetResponse();

            byte[] b = null;
            using (Stream stream = myResp.GetResponseStream())
            using (MemoryStream ms = new MemoryStream())
            {
                int count = 0;
                do
                {
                    byte[] buf = new byte[1024];
                    count = stream.Read(buf, 0, 1024);
                    ms.Write(buf, 0, count);
                } while (stream.CanRead && count > 0);
                b = ms.ToArray();
            }
            return b;
        }
        protected byte[] TakeScreenshot(IWebElement element)
        {

            driver.SwitchTo();
            driver.Manage().Window.Maximize();

            int offs = 100;
            int yoffset = ScrollTo(element, -offs);

            // Todo: cache the screenshot with Y coordinates.
            Byte[] byteArray = ((ITakesScreenshot)driver).GetScreenshot().AsByteArray;
            System.Drawing.Bitmap screenshot = new System.Drawing.Bitmap(new System.IO.MemoryStream(byteArray));

            System.Drawing.Rectangle croppedImage = new System.Drawing.Rectangle(element.Location.X, offs, element.Size.Width, element.Size.Height+offs);
            screenshot = screenshot.Clone(croppedImage, screenshot.PixelFormat);

            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(screenshot, typeof(byte[]));
        }

        protected Currencies.Currency GetCurrency(string text)
        {

            switch (StripToLetters(text))
            {
                case "Ft":
                case "":
                case "INGYENES":
                    return Currencies.Currency.HUF;
                default:
                    if (text.StartsWith("Ft"))
                    {
                        return Currencies.Currency.HUF;
                    }
                    throw new Exception("Unkown currency: " + text);
            }
        }
    }
}

