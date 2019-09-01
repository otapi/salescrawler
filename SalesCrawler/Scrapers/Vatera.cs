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
using OpenQA.Selenium.Interactions;

namespace SalesCrawler.Scrapers
{
    public class Vatera : Helpers.ScraperBase, Helpers.IScraper
    {
        public Scraper Datasheet { get; } = new Scraper()
        {
            Name = "Vatera"
        };

        public void StartSearch(ScraperSetting scraperSettings)
        {
            if (scraperSettings.IsSearchPatternURL)
            {
                driver.Navigate().GoToUrl(scraperSettings.SearchPattern);
            }
            else
            {
                // https://www.vatera.hu/listings/index.php?
                /*
                ob =16
                    obd=2
                    c=
                    q=bicikli
                    eo=
                    at%5B%5D=2
                    tr8=
                    p1=2000
                    p2=1000
                    ee=
                    es=
                    pci=0
                    tmpsb=Pontos+keres%C3%A9s
                    */
                Uri url = new Uri($"https://www.vatera.hu/listings/index.php?ob=16&obd=2&c=&eo=&at%5B%5D=2&tr8=&ee=&es=&pci=0&tmpsb=Pontos+keres%C3%A9s");
                AddQuery(ref url, "p2", scraperSettings.MaxPrice);
                AddQuery(ref url, "p1", scraperSettings.MinPrice);
                AddQuery(ref url, "q", scraperSettings.SearchPattern);

                driver.Navigate().GoToUrl(url);
            }
        }

        public IReadOnlyCollection<IWebElement> GetItemsOnPage()
        {
            Waitfor(By.XPath("//p[@id='footer-copyright']"));
            var ret = driver.FindElements(By.XPath("//tr[@data-gtm-name]"));
            return ret;
        }

        public void GetItem(IWebElement item, MatchData md)
        {
            Actions actions = new Actions(driver);
            actions.MoveToElement(item);
            actions.Perform();

            md.Seller = null;
            md.Title = item.GetAttribute("data-gtm-name");
            md.Url = item.FindElement(By.XPath(".//a")).GetAttribute("href");
            var imageUrl = item.FindElement(By.XPath(".//td[@class='listing-item-picture']//img")).GetAttribute("src");
            md.ImageBinary = GetImage(imageUrl);
            md.Description = null;
            md.ActualPrice = StripToInt(item.GetAttribute("data-gtm-price"));
            md.Currency = Currencies.Currency.HUF;
            md.IsAuction = false;
            if (item.FindElements(By.XPath(".//div[@class='item-location']")).Count != 0)
            {
                md.Location = item.FindElement(By.XPath(".//div[@class='item-location']")).Text.Replace("Termék helye:&nbsp;", "").Trim();
            }
            md.Expire = NEVEREXPIRE; // TODO: enddate
        }

        public By NextPageElement { get; } = By.XPath("//a[img[@src='https://img-ssl.vatera.hu/images/search/arw_frw.gif']]");

        public void UpdateMatchDetails(MatchData md)
        {
            
        }
    }
}
