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
    public class BikemagApro : Helpers.ScraperBase, Helpers.IScraper
    {
        public Scraper Datasheet { get; } = new Scraper()
        {
            Name = "Bikemag"
        };

        public void StartSearch(ScraperSetting scraperSettings)
        {
            if (scraperSettings.IsSearchPatternURL)
            {
                driver.Navigate().GoToUrl(scraperSettings.SearchPattern);
            }
            else
            {
                driver.Navigate().GoToUrl("https://apro.bikemag.hu/");

                var search = driver.FindElement(By.Id("SearchForm_needle"));
                search.Click();
                search.Clear();
                search.SendKeys(scraperSettings.SearchPattern);
                driver.FindElement(By.XPath("//form[@id='clSearch']//button")).Click();
            }
        }

        public IReadOnlyCollection<IWebElement> GetItemsOnPage()
        {
            Waitfor(By.XPath("//h5"));
            return driver.FindElements(By.XPath("//figure"));
        }

        public void GetItem(IWebElement item, MatchData md)
        {
            var link = item.FindElement(By.XPath(".//a"));

            md.Seller = null;
            md.Title = link.GetAttribute("title");
            md.Url = link.GetAttribute("href");
            md.ImageBinary = GetImage(item.FindElement(By.XPath(".//img")).GetAttribute("src"));
            md.Description = null;
            md.ActualPrice = StripToInt(item.FindElement(By.XPath(".//span[@class='d_block']")).Text);
            md.Currency = GetCurrency(item.FindElement(By.XPath(".//span[@class='d_block']")).Text);
            md.IsAuction = false;
            md.Location = item.FindElement(By.XPath(".//span[@class='d_block fw_bold color_light']")).Text;

            md.Expire = NEVEREXPIRE;
        }

        public By NextPageElement { get; } = By.XPath("i[@class='fa fa-angle-right d_inline_m']");

        public void GoToPageMatchDetails(MatchData md)
        {
        }

        public void UpdateMatchDetails(MatchData md)
        {

        }
    }
}
