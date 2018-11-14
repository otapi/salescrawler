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
    public class Jofogas : Helpers.ScraperBase, Helpers.IScraper
    {
        public Scraper Datasheet { get; } = new Scraper()
        {
            ScraperIdentifier = 1,
            Name = "Jofogas.hu"
        };

        
        override public void ScrapeList()
        {
            if (!Setting.IsSearchPatternURL)
            {
                driver.Navigate().GoToUrl("https://www.jofogas.hu/");
                //Wait().Until(ExpectedConditions.ElementToBeClickable(By.Id("CybotCookiebotDialogBodyButtonAccept")));
                if (driver.FindElements(By.Id("CybotCookiebotDialogBodyButtonAccept")).Count > 0)
                {
                    driver.FindElement(By.Id("CybotCookiebotDialogBodyButtonAccept")).Click();
                }

                //Wait().Until(c => c.FindElement(By.Id("index-search")));
                //Wait().Until(ExpectedConditions.ElementToBeClickable(By.Id("index-search")));
                var search = driver.FindElement(By.Id("index-search"));
                search.Click();
                search.Clear();
                search.SendKeys(Setting.SearchPattern);
                driver.FindElement(By.XPath("//button[@type='submit']//i[@class='mdi mdi-magnify']")).Click();
            }
            else
            {
                driver.Navigate().GoToUrl(Setting.SearchPattern);
                //Wait().Until(ExpectedConditions.ElementToBeClickable(By.Id("CybotCookiebotDialogBodyButtonAccept")));
                if (driver.FindElements(By.Id("CybotCookiebotDialogBodyButtonAccept")).Count > 0)
                {
                    driver.FindElement(By.Id("CybotCookiebotDialogBodyButtonAccept")).Click();
                }

            }
            //Wait().Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='ad-list-pager-item ad-list-pager-item-next active-item js_hist_li js_hist jofogasicon-right']")));

            Wait().Until(c => c.FindElement(By.XPath("//a[@class='ad-list-pager-item ad-list-pager-item-next active-item js_hist_li js_hist jofogasicon-right']")));
            foreach (var item in driver.FindElements(By.XPath("//div//div[@class='contentArea']")))
            {
                var md = new MatchData();
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
                AddMatch(md);
            };
        }

        override public void UpdateMatchDetails(MatchData matchData)
        {
            driver.Navigate().GoToUrl(matchData.Url);

            if (driver.FindElements(By.Id("CybotCookiebotDialogBodyButtonAccept")).Count > 0)
            {
                driver.FindElement(By.Id("CybotCookiebotDialogBodyButtonAccept")).Click();
            }

            Wait().Until(c => c.FindElement(By.XPath("//div[@class='description']")));
            matchData.Description = driver.FindElement(By.XPath("//div[@class='description']")).Text;
            matchData.Seller = driver.FindElement(By.XPath("//div[@class='name']")).Text;

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
