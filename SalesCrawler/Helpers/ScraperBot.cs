using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesCrawler.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Drawing;
using System.Net;
using System.IO;
using SalesCrawler.ViewModels;

namespace SalesCrawler.Helpers
{
    public class ScraperBot : BaseVM
    {
        private static readonly bool SAVE_DB_ONLYONCE = true;
        private readonly Type ScraperType;
        private List<Match> AddedMatches = new List<Match>();
        string _Message;
        public string Message
        {
            get { return _Message; }
            set { SetProperty(ref _Message, value); }
        }

        public ScraperBot(Type scraperType)
        {
            ScraperType = scraperType;
        }
        public void ScrapeListBase(IWebDriver driver, ScraperSetting scraperSettings)
        {
            AddedMatches = new List<Match>();
            Message = "Start Scrape List";
            var scraperOrig = Activator.CreateInstance(ScraperType);
            ((ScraperBase)scraperOrig).Init(driver);
            var scraper = (IScraper)scraperOrig;
            scraper.StartSearch(scraperSettings);
            for (int page = 0; page < scraperSettings.PagesToScrape; page++)
            {
                Message = $"Scraping page {page+1}/{scraperSettings.PagesToScrape}";
                var items = scraper.GetItemsOnPage();
                for (int i = 0; i<items.Count; i++)
                {
                    Message = $"Scraping page {page + 1}/{scraperSettings.PagesToScrape}, item {i+1}/{items.Count}";
                    var md = new MatchData();
                    scraper.GetItem(items.ElementAt(i), md);
                    if (md.Status != MatchDataStatus.Sold)
                    {
                        AddMatch(md, scraperSettings);
                    } else if (scraperSettings.DoOnlyTest) break;
                }
                if (scraper.NextPageElement == null)
                {
                    break;
                }
                var np = driver.FindElements(scraper.NextPageElement);
                if (np.Count == 0)
                {
                    break;
                }
                try
                {
                    np[0].SendKeys(Keys.Return);
                } catch
                {
                    np[0].Click();
                }
                
            }

            // If details need to be updated at new Matches...
            //UpdateMatchDetails(driver, AddedMatches, scraperSettings, scraper);
            Message = $"ScrapeList finished, found {AddedMatches.Count} items.";

            if (SAVE_DB_ONLYONCE)
            {
                App.DB.SaveChangesAsync().Wait();
            }
        }

        public void UpdateMatchDetails(IWebDriver driver, List<Match> matches, ScraperSetting scraperSettings, IScraper scraper = null)
        {
            int TabsInParallel = 10;
            if (scraper == null)
            {
                var scraperOrig = Activator.CreateInstance(ScraperType);
                ((ScraperBase)scraperOrig).Init(driver);
                scraper = (IScraper)scraperOrig;
            }

            string origtab = driver.CurrentWindowHandle;
            for (int tabRound = 0; tabRound < matches.Count/TabsInParallel +1 ; tabRound++)
            {
                int ifrom = tabRound * TabsInParallel;
                int ito = (tabRound + 1) * TabsInParallel;
                if (ito > matches.Count)
                {
                    ito = matches.Count;
                }

                Message = $"Load Details Pages: {ifrom}-{ito}/{AddedMatches.Count}";
                for (int i = ifrom; i < ito; i++)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript($"window.open(\"{matches[i].MatchData.Url}\")");
                }
                List<string> tabs = new List<string>();
                
                tabs.AddRange(driver.WindowHandles.ToList());
                int t = ito-ifrom;
                for (int i = ifrom; i < ito; i++)
                {
                    driver.SwitchTo().Window(tabs[t]);
                    t--;

                    scraper.UpdateMatchDetails(matches[i].MatchData);
                    driver.Close();
                    
                    if (!SAVE_DB_ONLYONCE)
                    {
                        App.DB.SaveChangesAsync().Wait();
                    }
                }
                driver.SwitchTo().Window(origtab);
            }

            if (SAVE_DB_ONLYONCE)
            {
                App.DB.SaveChangesAsync().Wait();
            }
            driver.SwitchTo().Window(origtab);
        }

        protected void AddMatch(MatchData matchData, ScraperSetting setting)
        {
            var m = new Match();
            m.LastScannedDate = DateTime.Now;
            m.MatchData = matchData;
            m.PriceHistories.Add(new PriceHistory()
            {
                Date = m.LastScannedDate,
                PriceHUF = matchData.ActualPriceHUF
            });
            m.ScraperSetting = setting;
            App.DB.Matches.Add(m);
            if (!SAVE_DB_ONLYONCE)
            {
                App.DB.SaveChangesAsync().Wait();
            }
            AddedMatches.Add(m);
            
        }
    }
}

