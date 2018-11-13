using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesCrawler.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;


namespace SalesCrawler.ViewModels
{
    public class ScraperBase
    {
        protected IWebDriver driver;

        public ScraperSetting Setting { get; set; }
        
        public void Init(ScraperSetting crawlerbotSetting, IWebDriver webDriver)
        {
            driver = webDriver;
            Setting = crawlerbotSetting;
        }

        public virtual void Start()
        {

        }

        public void StartBase()
        {
            Start();
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
        protected WebDriverWait Wait(int timeout=10) {
            return new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
        }

        protected void SaveMatch(string seller, string title, string url, string imageUrl, string description,
                double actualPrice, Default.Currency currency)
        {
        
        var m = new Match()
            {
                Seller = seller,
                Title = title,
                Url = url,
                ImageUrl = imageUrl,
                Description = description,
                ActualPrice = actualPrice,
                Currency = currency,
                CrawlerbotSetting = Setting,
                Alive = true,
                LastScannedDate = DateTime.Now,
                ActualPriceHUF = ConvertPriceToHUF(currency, actualPrice),
                Visible = true,

            };
            App.DB.Matches.Add(m);
            App.DB.SaveChangesAsync().Wait();
        }

        public static int ConvertPriceToHUF(Default.Currency currency, double price)
        {
            switch (currency) {
                case Default.Currency.EUR:
                    return (int) price * 325;
                case Default.Currency.HUF:
                    return (int) price;
                case Default.Currency.USD:
                    return (int) price * 180;
                default:
                    App.PrintWarning($"Unkown currency: [{currency.ToString()}] with price ${price.ToString()}");
                    return 0;
            }
        }
        protected void PrintNote(string message)
        {
            App.PrintNote($"[{Setting.Name}] ${message}");
        }

        protected void PrintWarning(string message)
        {
            App.PrintWarning($"[{Setting.Name}] ${message}");
        }
    }
}
