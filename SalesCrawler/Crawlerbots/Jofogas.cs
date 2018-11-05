using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesCrawler.Models;

namespace SalesCrawler.Crawlerbots
{
    public class Jofogas : Architecture.ICrawlerbot
    {
        public Crawlerbot Datasheet { get; } = new Crawlerbot()
        {
            CrawlerbotId = 1,
            Name = "Jofogas.hu"
        };
    }
}
