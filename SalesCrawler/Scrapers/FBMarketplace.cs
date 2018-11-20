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
            Name = "FB Marketplace"
        };

        ScraperSetting ScraperSettings;
        public void StartSearch(ScraperSetting scraperSettings)
        {
            ScraperSettings = scraperSettings;
            if (scraperSettings.IsSearchPatternURL)
            {
                driver.Navigate().GoToUrl(scraperSettings.SearchPattern);
            }
            else
            {
                driver.Navigate().GoToUrl($"https://www.facebook.com/marketplace/budapest/search?query={System.Web.HttpUtility.UrlEncode(scraperSettings.SearchPattern)}");

            }

            
            
        }

        public IReadOnlyCollection<IWebElement> GetItemsOnPage()
        {
            driver.Manage().Window.Maximize();
            IReadOnlyCollection<IWebElement> ret = null;
            for (int i = 0; i < ScraperSettings.PagesToScrape; i++)
            {
                ret = WaitForItemsloaded();
                if (ret.Count == 0)
                {
                    break;
                } else
                {
                    ret.ElementAt(0).SendKeys(Keys.PageDown);
                    ret.ElementAt(0).SendKeys(Keys.PageDown);
                    ret.ElementAt(0).SendKeys(Keys.PageDown);
                    ret.ElementAt(0).SendKeys(Keys.PageDown);
                    ret.ElementAt(0).SendKeys(Keys.PageDown);
                }
            }
            driver.Manage().Window.Minimize();
            return ret;
        }

        private IReadOnlyCollection<IWebElement> WaitForItemsloaded()
        {
            
            var list = driver.FindElements(By.XPath("//a[@data-testid='marketplace_feed_item']"));
            int prevcount = list.Count;
            int actcount = -1;
            Wait();
            for (;prevcount!=actcount;)
            {
                prevcount = list.Count;
                list = driver.FindElements(By.XPath("//a[@data-testid='marketplace_feed_item']"));
                actcount = list.Count;
                Wait();
            }
            return list;
        }

        public void GetItem(IWebElement item, MatchData md)
        {
            md.Seller = null;
            md.Title = item.GetAttribute("title");
            md.Url = item.GetAttribute("href");
            var imageUrl = item.FindElement(By.XPath(".//img")).GetAttribute("src");
            md.ImageBinary = GetImage(imageUrl);
            md.Description = null;
            var price = item.FindElement(By.XPath(".//div[@class='_f3l _4x3g']")).Text;
            md.ActualPrice = StripToInt(price);
            md.Currency = GetCurrency(price);
            md.IsAuction = false;
            md.Location = item.FindElement(By.XPath(".//span[@location]")).Text;
            md.Expire = NEVEREXPIRE;
        }

        public By NextPageElement { get; } = null;

        public void UpdateMatchDetails(MatchData md)
        {
            Waitfor(By.XPath("//p[@class='_4etw']"), 30);
            if (driver.FindElements(By.XPath("//a[contains(@title,'Továbbiak')]")).Count == 0)
            {
                driver.FindElement(By.XPath("//a[contains(@title,'Továbbiak')]")).Click();
            }
            md.Description = driver.FindElement(By.XPath("//p[@class='_4etw']/span")).Text;

        }
    }
}
