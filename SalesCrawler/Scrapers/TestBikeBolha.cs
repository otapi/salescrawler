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
    public class TestBikeBolha : Helpers.ScraperBase, Helpers.IScraper
    {
        // Same as BikeMag!
        public Scraper Datasheet { get; } = new Scraper()
        {
            Name = "TestBike"
        };

        public void StartSearch(ScraperSetting scraperSettings)
        {
            if (scraperSettings.IsSearchPatternURL)
            {
                driver.Navigate().GoToUrl(scraperSettings.SearchPattern);
            }
            else
            {
                driver.Navigate().GoToUrl($"https://bolha.testbike.hu/aprok?q={System.Web.HttpUtility.UrlEncode(scraperSettings.SearchPattern)}");
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


        public void UpdateMatchDetails(MatchData md)
        {
            Waitfor(By.XPath("//div[@itemprop='description']"));
            md.Description = driver.FindElement(By.XPath("//div[@itemprop='description']")).Text;
            var sell = driver.FindElements(By.XPath("//a[@title='profiloldal']"));
            if (sell.Count == 0)
            {
                md.Status = MatchDataStatus.Sold;
                return;
            }
            md.Seller = sell[0].Text;
        }
    }
}
