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
                np[0].Click(); 
            }

            // If details need to be updated at new Matches...
            for (int i=0; i<AddedMatches.Count;i++)
            {
                Message = $"Scrape Details: {i+1}/{AddedMatches.Count}";

                UpdateMatchDetailsBase(driver, AddedMatches[i], scraperSettings, scraper);
            }
            Message = $"ScrapeList finished, found {AddedMatches.Count} items.";

            if (SAVE_DB_ONLYONCE)
            {
                App.DB.SaveChangesAsync().Wait();
            }
        }

        public void UpdateMatchDetailsBase(IWebDriver driver, Match match, ScraperSetting scraperSettings, IScraper scraper = null)
        {
            if (scraper == null)
            {
                var scraperOrig = Activator.CreateInstance(ScraperType);
                ((ScraperBase)scraperOrig).Init(driver);
                scraper = (IScraper)scraperOrig;
            }
            scraper.UpdateMatchDetails(match.MatchData);
            if (!SAVE_DB_ONLYONCE)
            {
                App.DB.SaveChangesAsync().Wait();
            }

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

