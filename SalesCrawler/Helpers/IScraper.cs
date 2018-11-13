using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using SalesCrawler.Models;

namespace SalesCrawler.Helpers
{
    public interface IScraper
    {
        Scraper Datasheet { get; }
        void ScrapeList();
        void UpdateMatchDetails(MatchData matchData);
    }
}
