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
        const string DOWNLOADFOLDER = @"c:\i\FBDownload\";

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
            for (; 1==2;)
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
            md.Seller = null;
            md.Title = item.FindElement(By.XPath(".//a")).Text;
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
                
                driver.ExecuteScript("arguments[0].click()", element);

                /*
                Actions actions = new Actions(driver);
                actions.MoveToElement(item.FindElement(By.XPath(".//i[@class='img sp_S8pk_WlQaUU sx_7e1fad']"))).Click().Build().Perform();
                */


                Waitfor(By.XPath("//ul[@role='menu']"));
                var download = driver.FindElements(By.XPath("//ul[@role='menu']//a"))[1];

                DownloadFile(@"https://www.facebook.com" + download.GetAttribute("href"), DOWNLOADFOLDER + md.Title);
            }
        }

        public By NextPageElement { get; } = null;

        public void UpdateMatchDetails(MatchData md)
        {
            
        }
    }
}
