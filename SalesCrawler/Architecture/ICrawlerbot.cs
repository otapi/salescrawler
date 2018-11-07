using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesCrawler.Models;

namespace SalesCrawler.Architecture
{
    interface IScraper
    {
        Crawlerbot Datasheet { get; }
        CrawlerbotSetting Setting { get; }
        Task Task { get; set; }
        void Init(CrawlerbotSetting crawlerbotSetting);
        Task StartAsync();
    }
}
