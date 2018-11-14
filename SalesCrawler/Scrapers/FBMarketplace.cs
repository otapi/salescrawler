using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SalesCrawler.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SalesCrawler.Scrapers
{
    public class FBMarketplace : Helpers.ScraperBase, Helpers.IScraper
    {
        public Scraper Datasheet { get; } = new Scraper()
        {
            ScraperIdentifier = 2,
            Name = "FB Marketplace"
        };

        override public void ScrapeList()
        {
            if (!Setting.IsSearchPatternURL)
            {
                driver.Navigate().GoToUrl("https://www.facebook.com/marketplace/budapest?ref=bookmark");
                var truck = driver.FindElement(By.XPath("//div[div[@aria-label='„Keresés” gomb']]/span//input"));
                truck.Click();
                truck.Clear();
                truck.SendKeys(Setting.SearchPattern);
                driver.FindElement(By.XPath("//div[@aria-label='„Keresés” gomb']")).Click();

            }
            else
            {
                driver.Navigate().GoToUrl(Setting.SearchPattern);
            }

            Wait();
            for (int i=0;i<Setting.PagesToScrape;i++)
            {
                ScrollToBottom();
                Wait();
            }
            var items = driver.FindElements(By.XPath("//a[@data-testid='marketplace_feed_item']"));
            foreach (var item in items)
            {
                var md = new MatchData();
                md.Seller = null;
                md.Title = item.GetAttribute("title");
                md.Url = "https://www.facebook.com"+item.GetAttribute("href");
                md.ImageBinary = TakeScreenshot(item);
                md.Description = null;
                md.ActualPrice = StripToInt(item.FindElement(By.XPath(".//div[@class='_f3l _4x3g']")).Text);
                md.Currency = GetCurrency(item.FindElement(By.XPath(".//div[@class='_f3l _4x3g']")).Text);
                md.IsAuction = false;
                md.Location = item.FindElement(By.XPath(".//span[@location]")).Text;
                md.Expire = NEVEREXPIRE;
                AddMatch(md);
            };
        }

        override public void UpdateMatchDetails(MatchData matchData)
        {
            

        }


        Currencies.Currency GetCurrency(string text)
        {
            
            switch (StripToLetters(text))
            {
                case "Ft":
                case "INGYENES":
                case "":
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
