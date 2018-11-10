using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesCrawler.Models;

namespace SalesCrawler.ViewModels
{
    public class CrawlerBotBase
    {
        // http://scraping.pro/selenium-ide-and-web-scraping/
        // https://saucelabs.com/resources/articles/getting-started-with-webdriver-in-c-using-visual-studio
        // https://javabeginnerstutorial.com/selenium/selenium-tutorial/
        // https://medium.com/the-andela-way/introduction-to-web-scraping-using-selenium-7ec377a8cf72
        // https://github.com/AngleSharp/AngleSharp over htmlagilitypack

        public ScraperSetting Setting { get; set; }
        
        public void Init(ScraperSetting crawlerbotSetting)
        {
            Setting = crawlerbotSetting;
        }

        protected async Task SaveMatch(string seller, string title, string url, string imageUrl, string description,
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
            await App.DB.SaveChangesAsync();
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
