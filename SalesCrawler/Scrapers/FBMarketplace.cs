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

        public void StartSearch(ScraperSetting scraperSettings)
        {
            if (scraperSettings.IsSearchPatternURL)
            {
                driver.Navigate().GoToUrl(scraperSettings.SearchPattern);
            }
            else
            {
                driver.Navigate().GoToUrl("https://www.facebook.com/marketplace/budapest?ref=bookmark");
                var truck = driver.FindElement(By.XPath("//div[div[@aria-label='„Keresés” gomb']]/span//input"));
                truck.Click();
                truck.Clear();
                truck.SendKeys(scraperSettings.SearchPattern);
                driver.FindElement(By.XPath("//div[@aria-label='„Keresés” gomb']")).Click();
            }

            for (int i = 0; i < scraperSettings.PagesToScrape; i++)
            {
                ScrollToBottom();
                Wait();
            }
        }

        public IReadOnlyCollection<IWebElement> GetItemsOnPage()
        {
            Waitfor(By.XPath("//div[@id='footer_jofogas']"));
            return driver.FindElements(By.XPath("//a[@data-testid='marketplace_feed_item']"));
        }

        public void GetItem(IWebElement item, MatchData md)
        {
            md.Seller = null;
            var link = item.FindElement(By.XPath(".//h3[@class='item-title']/a"));
            md.Title = link.Text;
            md.Url = link.GetAttribute("href");
            var imageUrl = item.FindElement(By.XPath(".//img")).GetAttribute("style").Replace("background-image: url(\"", "").Replace("\");", "");
            md.ImageBinary = GetImage(imageUrl);
            //md.ImageUrl = item.FindElement(By.XPath(".//img")).GetAttribute("style").Replace("background-image: url(\"", "").Replace("\");", "");
            md.Description = null;
            md.ActualPrice = StripToInt(item.FindElement(By.XPath(".//h3[@class='item-price']")).Text);
            md.Currency = GetCurrency(item.FindElement(By.XPath(".//span[@class='currency']")).Text);
            md.IsAuction = false;
            md.Location = item.FindElement(By.XPath(".//section[@class='reLiSection cityname']")).Text;
            md.Expire = NEVEREXPIRE;
        }

        public By NextPageElement { get; } = null;

        public void GoToPageMatchDetails(MatchData md)
        {
            driver.Navigate().GoToUrl(md.Url);
        }

        public void UpdateMatchDetails(MatchData md)
        {
        
            if (driver.FindElements(By.Id("CybotCookiebotDialogBodyButtonAccept")).Count > 0)
            {
                driver.FindElement(By.Id("CybotCookiebotDialogBodyButtonAccept")).Click();
            }

            Waitfor(By.XPath("//div[@class='description']"));
            md.Description = driver.FindElement(By.XPath("//div[@class='description']")).Text;
            md.Seller = driver.FindElement(By.XPath("//div[@class='name']")).Text;
        }
    }
}
