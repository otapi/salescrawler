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
using System.IO;

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
            for (var i=0; true;)
            {
                i++;
                try
                {
                    var ret = GetItemsOnPageImpl();
                    return ret;
                }
                catch
                {
                    StartSearch(ScraperSettings);
                }
            }
        }
        public IReadOnlyCollection<IWebElement> GetItemsOnPageImpl()
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
                Wait(1);
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

            md.Title = titles[0].GetAttribute("data-tooltip-content");
            if (md.Title == null)
            {
                md.Title = titles[0].Text;
            }
            if (md.Title == null || md.Title == "" || md.Title == "Név" || File.Exists(@"C:\Users\otapi\Downloads\" + md.Title))
            {
                md.Expire = NEVEREXPIRE;
                return;
            }
            
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
            var elems = item.FindElements(By.XPath(".//i[@class='img sp_S8pk_WlQaUU sx_7e1fad']"));
            if (elems.Count == 0)
            {
                elems = item.FindElements(By.XPath(".//i[@class='img sp_nUm-Frhgfk0 sx_ae104b']"));
                if (elems.Count == 0)
                {
                    return;
                }
            }


            var elem = elems[0];
            MoveTo(elem);
            elem.Click();

            Waitfor(By.XPath("//ul[@role='menu']"));
            var downloads = driver.FindElements(By.XPath("//ul[@role='menu']//a/span/span[text()='Letöltés']"));
            if (downloads.Count > 0)
            {
                var download = downloads[0];
                MoveTo(download);
                download.Click();
            }
            foreach (var element in driver.FindElements(By.XPath("//ul[@role='menu']//a"))) {
                IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor) driver;
                jsExecutor.ExecuteScript(
                    "arguments[0].parentNode.removeChild(arguments[0])", element);
            }
            driver.FindElement(By.XPath("//div")).Click();
        }

        public By NextPageElement { get; } = null;

        public void UpdateMatchDetails(MatchData md)
        {
            
        }
    }
}
