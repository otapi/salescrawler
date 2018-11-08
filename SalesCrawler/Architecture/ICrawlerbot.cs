using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesCrawler.Models;

namespace SalesCrawler.Architecture
{
    public interface IScraper
    {
        Scraper Datasheet { get; }
        ScraperSetting Setting { get; }
        Task Task { get; set; }
        void Init(ScraperSetting crawlerbotSetting);
        Task StartAsync();
    }
}
