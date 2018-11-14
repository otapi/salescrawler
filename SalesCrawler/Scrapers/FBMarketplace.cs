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
                driver.FindElement(By.XPath("//span[@placeholder='„truck” keresése']//input")).Click();
                driver.FindElement(By.XPath("//span[@placeholder='„truck” keresése']//input")).Clear();
                driver.FindElement(By.XPath("//span[@placeholder='„truck” keresése']//input")).SendKeys(Setting.SearchPattern);
                driver.FindElement(By.XPath("//div[@aria-label='„Keresés” gomb']")).Click();
            }
            else
            {
                driver.Navigate().GoToUrl(Setting.SearchPattern);
            }
            
            foreach (var item in driver.FindElements(By.XPath("//a[@data-testid='marketplace_feed_item']")))
            {
                var md = new MatchData();
                md.Seller = null;
                md.Title = item.GetAttribute("title");
                md.Url = "https://www.facebook.com"+item.GetAttribute("href");
                //md.ImageUrl = null;
                md.Description = null;
                md.ActualPrice = StripToInt(item.FindElement(By.XPath(".//div[@class='_f3l _4x3g']")).Text);
                md.Currency = GetCurrency(item.FindElement(By.XPath(".//div[@class='_f3l _4x3g']")).Text);
                md.IsAuction = false;
                md.Location = item.FindElement(By.XPath(".//span[@location='[object Object']")).Text;
                md.Expire = NEVEREXPIRE;
                AddMatch(md);
            };
        }

        override public void UpdateMatchDetails(MatchData matchData)
        {
            

        }


        Currencies.Currency GetCurrency(string text)
        {
            text = text.Replace("&nbsp;", "").Trim();
            switch (text)
            {
                case "Ft":
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
