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
            Name = "Jofogas.hu"
        };

        public void StartSearch(ScraperSetting scraperSettings)
        {
            if (scraperSettings.IsSearchPatternURL)
            {
                driver.Navigate().GoToUrl(scraperSettings.SearchPattern);
            }
            else
            {
                driver.Navigate().GoToUrl($"https://www.jofogas.hu/magyarorszag?q={System.Web.HttpUtility.UrlEncode(scraperSettings.SearchPattern)}");
            }
        }

        public IReadOnlyCollection<IWebElement> GetItemsOnPage()
        {
            Waitfor(By.XPath("//div[@id='footer_jofogas']"));
            return driver.FindElements(By.XPath("//div//div[@class='contentArea']"));
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

            if (item.FindElements(By.XPath("//div[contains(text(),'Kiszállítás folyamatban')]")).Count > 0)
            {
                md.Status = MatchDataStatus.Sold;
            }
        }

        public By NextPageElement { get; } = By.XPath("//a[@class='ad-list-pager-item ad-list-pager-item-next active-item js_hist_li js_hist jofogasicon-right']");

        public void UpdateMatchDetails(MatchData md)
        {
            Waitfor(By.XPath("//div[@class='description']"));
            md.Description = driver.FindElement(By.XPath("//div[@class='description']")).Text;
            var sell = driver.FindElements(By.XPath("//div[@class='name']"));
            if (sell.Count == 0)
            {
                md.Status = MatchDataStatus.Sold;
                return;
            }
            md.Seller = sell[0].Text;
        }
    }
}
