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
using System.Windows.Forms;
using OpenQA.Selenium.Interactions;

namespace SalesCrawler.Scrapers
{
    public class FBGroupFilesDownload : Helpers.ScraperBase, Helpers.IScraper
    {
        public Scraper Datasheet { get; } = new Scraper()
        {
            Name = "FB Files Downloader"
            
        };
        // https://www.facebook.com/groups/269900133521924/files/
        
        ScraperSetting ScraperSettings;
        public void StartSearch(ScraperSetting scraperSettings)
        {
            ScraperSettings = scraperSettings;
            driver.Navigate().GoToUrl(scraperSettings.SearchPattern);
        }

        public IReadOnlyCollection<IWebElement> GetItemsOnPage()
        {
            driver.Manage().Window.Maximize();
            IReadOnlyCollection<IWebElement> ret = null;

            // Let's show all files on the page
            for (; true;)
            {
                var continuebuttons = driver.FindElements(By.XPath("//a[@class='pam uiBoxLightblue uiMorePagerPrimary']"));
                if (continuebuttons.Count == 0)
                {
                    break;
                }
                continuebuttons[0].Click();
                Waituntil(By.XPath("//div[@class='clearfix uiMorePager stat_elem _52jv async_saving']"));
            }
        
            ret = driver.FindElements(By.XPath("//div[@class='clearfix']"));

            driver.Manage().Window.Minimize();
            return ret;
        }

        public void GetItem(IWebElement item, MatchData md)
        {
            driver.Manage().Window.Maximize();
            md.Seller = null;
            var titles = item.FindElements(By.XPath(".//a"));
            if (titles.Count == 0)
            {
                md.Expire = NEVEREXPIRE;
                return;
            }
            md.Title = titles[0].Text;
            md.Url = item.FindElement(By.XPath(".//a")).GetAttribute("href");
            md.ImageBinary = null;
            md.Description = null;
            var dateoffile = item.FindElement(By.XPath(".//span[@class='timestampContent']")).Text;
            DateTime dateOfFile;
            if (DateTime.TryParse(dateoffile, out dateOfFile)) {
                md.Expire = dateOfFile;
            } else
            {
                md.Expire = NEVEREXPIRE;
            }
            if (md.Title != "Dokumentum létrehozása")
            {
                var elem = item.FindElement(By.XPath(".//i[@class='img sp_S8pk_WlQaUU sx_7e1fad']"));
                elem.Click();

                Waitfor(By.XPath("//ul[@role='menu']"));
                var download = driver.FindElements(By.XPath("//ul[@role='menu']//a"))[1];
                download.Click();

                foreach (var element in driver.FindElements(By.XPath("//ul[@role='menu']//a"))) {
                    IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor) driver;
                    jsExecutor.ExecuteScript(
                        "arguments[0].parentNode.removeChild(arguments[0])", element);
                }
                driver.FindElement(By.XPath("//div")).Click();
            }
        }

        public By NextPageElement { get; } = null;

        public void UpdateMatchDetails(MatchData md)
        {
            
        }
    }
}
