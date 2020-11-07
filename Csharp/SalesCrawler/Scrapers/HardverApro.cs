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
    public class HardverApro : Helpers.ScraperBase, Helpers.IScraper
    {
        public Scraper Datasheet { get; } = new Scraper()
        {
            Name = "HardverApro"
        };

        public void StartSearch(ScraperSetting scraperSettings)
        {
            if (scraperSettings.IsSearchPatternURL)
            {
                driver.Navigate().GoToUrl(scraperSettings.SearchPattern);
            }
            else
            {
                // https://hardverapro.hu/aprok/keres.php?stext=bicikli&county=&stcid=&settlement=&stmid=&minprice=1000&maxprice=2000&company=&cmpid=&user=&usrid=&selling=1&stext_none=
                Uri url = new Uri($"https://hardverapro.hu/aprok/keres.php");
                AddQuery(ref url, "maxprice", scraperSettings.MaxPrice);
                AddQuery(ref url, "minprice", scraperSettings.MinPrice);
                AddQuery(ref url, "selling", "1");
                AddQuery(ref url, "stext", scraperSettings.SearchPattern);

                driver.Navigate().GoToUrl(url);
            }
        }

        public IReadOnlyCollection<IWebElement> GetItemsOnPage()
        {
            Waitfor(By.XPath("//div[@id='forum-nav-btm']//a[@class='btn']"));
            var ret = driver.FindElements(By.XPath("//li[@class='media']"));
            return ret;
        }

        public void GetItem(IWebElement item, MatchData md)
        {
            md.Seller = null;
            var link = item.FindElement(By.XPath(".//div[@class='media-body']//a"));
            md.Title = link.Text;
            md.Url = link.GetAttribute("href");
            var imageUrl = item.FindElement(By.XPath(".//img")).GetAttribute("src");
            md.ImageBinary = GetImage(imageUrl);
            //md.ImageUrl = item.FindElement(By.XPath(".//img")).GetAttribute("style").Replace("background-image: url(\"", "").Replace("\");", "");
            md.Description = null;
            md.ActualPrice = StripToInt(item.FindElement(By.XPath(".//div[@class='uad-price']")).Text);
            md.Currency = GetCurrency(item.FindElement(By.XPath(".//div[@class='uad-price']")).Text);
            md.IsAuction = false;
            md.Location = item.FindElement(By.XPath(".//div[@class='uad-light']")).Text;
            md.Expire = NEVEREXPIRE;

            if (item.FindElements(By.XPath("//div[contains(text(),'Kiszállítás folyamatban')]")).Count > 0)
            {
                md.Status = MatchDataStatus.Sold;
            }
        }

        public By NextPageElement { get; } = By.XPath("//a[@rel='next']");

        public void UpdateMatchDetails(MatchData md)
        {
            
        }
    }
}
