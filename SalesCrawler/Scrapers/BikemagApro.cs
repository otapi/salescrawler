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

        
        override public void ScrapeList()
        {
            if (!Setting.IsSearchPatternURL)
            { 
                driver.Navigate().GoToUrl("https://apro.bikemag.hu/");

                var search = driver.FindElement(By.Id("SearchForm_needle"));
                search.Click();
                search.Clear();
                search.SendKeys(Setting.SearchPattern);
                driver.FindElement(By.XPath("//form[@id='clSearch']//button")).Click();
            }
            else
            {
                driver.Navigate().GoToUrl(Setting.SearchPattern);
                
            }
            //Wait().Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[@class='ad-list-pager-item ad-list-pager-item-next active-item js_hist_li js_hist jofogasicon-right']")));
            for (int page = 0; page < Setting.PagesToScrape; page++)
            {
                Waitfor().Until(c => c.FindElement(By.XPath("//h5")));
                foreach (var item in driver.FindElements(By.XPath("//figure")))
                {
                    var md = new MatchData();
                    var link = item.FindElement(By.XPath(".//a"));

                    md.Seller = null;
                    md.Title = link.GetAttribute("title");
                    md.Url =  link.GetAttribute("href");
                    md.ImageBinary = GetImage(item.FindElement(By.XPath(".//img")).GetAttribute("src"));
                    md.Description = null;
                    md.ActualPrice = StripToInt(item.FindElement(By.XPath(".//span[@class='d_block']")).Text);
                    md.Currency = GetCurrency(item.FindElement(By.XPath(".//span[@class='d_block']")).Text);
                    md.IsAuction = false;
                    md.Location = item.FindElement(By.XPath(".//span[@class='d_block fw_bold color_light']")).Text;

                    md.Expire = NEVEREXPIRE;

                    AddMatch(md);
                };

                if (driver.FindElements(By.XPath("i[@class='fa fa-angle-right d_inline_m']")).Count == 0)
                {
                    break;
                }
                driver.FindElement(By.XPath("i[@class='fa fa-angle-right d_inline_m']")).Click();
            }
        }

        
        
    }
}
